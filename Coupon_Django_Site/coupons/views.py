from django.http import HttpResponse
from django.views.decorators.csrf import csrf_exempt
import psycopg2
import os
from django.http import JsonResponse, Http404
from django.views.decorators.csrf import csrf_exempt
from django.db import connection
from django.core import serializers
from datetime import datetime
import psycopg2.extras
# from .models import Coupon
import json

import logging
from ddtrace import tracer

FORMAT = ('%(asctime)s %(levelname)s [%(name)s] [%(filename)s:%(lineno)d] '
          '[dd.service=%(dd.service)s dd.env=%(dd.env)s dd.version=%(dd.version)s dd.trace_id=%(dd.trace_id)s dd.span_id=%(dd.span_id)s] '
          '- %(message)s')
logging.basicConfig(format=FORMAT)
log = logging.getLogger(__name__)
log.level = logging.INFO

def index(request):
    return HttpResponse("Hello, world. You're at the coupon index.")

DB_URI = os.environ.get('COUPON_DATABASE_URL')
# Establish database connection
def get_db_connection():
    print("Establishing database connection...")
    # conn = psycopg2.connect(
    #     dbname=DB_NAME,
    #     user=DB_USER,
    #     host=DB_HOST,
    #     password=DB_PASSWORD
    # )
    conn = psycopg2.connect(DB_URI)
    print("Database connection established.")
    return conn

@csrf_exempt
@tracer.wrap()
def apply_coupon(request):
    if request.method == 'POST':
        data = json.loads(request.body)
        coupon_code = data.get('coupon_code')
        items = data.get('items')

        if not coupon_code or not items:
            raise Http404("Missing coupon code or items.")

        conn = get_db_connection()
        cursor = conn.cursor(cursor_factory=psycopg2.extras.NamedTupleCursor)
        cursor.execute("SELECT \"DiscountType\", \"DiscountValue\", \"ExpirationDate\" FROM public.\"Coupons\" WHERE \"Code\" = %s", (coupon_code,))
        coupon = cursor.fetchone()

        log.info(f"Coupon: {coupon}")
        
        if not coupon:
            raise Http404("Coupon code not found.")
        
        if coupon.ExpirationDate.date() < datetime.now().date():
            raise Http404("Coupon code is expired.")

        adjusted_item_prices = []
        total_discounted_price = 0
        
        for item in items:
            item_price = float(item['unit_price'])
            item_units = item['units']

            discount = float(0)
            if coupon.DiscountType == 'percentage':
                discount = item_price * (float(coupon.DiscountValue) / 100)
            elif coupon.DiscountType == 'fixed':
                discount = float(coupon.DiscountValue)

            adjusted_units = item_units
            adjusted_price = item_price - discount
            adjusted_item_prices.append({
                'id': item['id'],
                'name': item['name'],
                'original_unit_price': item_price,
                'adjusted_unit_price': adjusted_price,
                'original_units': item_units,
                'adjusted_units': adjusted_units
            })

            total_discounted_price += adjusted_price * adjusted_units
        
        response = {
            'final_price': total_discounted_price,
            'adjusted_items': adjusted_item_prices
        }
        
        return JsonResponse(response)

    else:
        raise Http404