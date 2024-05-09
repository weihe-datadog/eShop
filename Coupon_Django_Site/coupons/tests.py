import unittest
from unittest.mock import MagicMock, patch
from django.test import TestCase, Client
from datetime import datetime, timedelta
from django.http import request
import json

from django.urls import reverse

class ApplyCouponTest(TestCase):
    def setUp(self):
        self.client = Client()

    @patch('psycopg2.connect')
    @patch('psycopg2.extras.NamedTupleCursor')
    def test_apply_coupon(self, mock_cursor, mock_connect):
        # Mocking the database connection
        mock_conn = mock_connect.return_value
        mock_cursor = mock_conn.cursor.return_value
        mock_cursor.__enter__.return_value = mock_cursor

        # Mocking coupon from database
        # Assure to include future date to be valid coupon
        mock_coupon = MagicMock(DiscountType='percentage',
                                DiscountValue=25,
                                ExpirationDate=datetime.now() + timedelta(days=10))
        mock_cursor.fetchone.return_value = mock_coupon

        # Provide the coupon and items information
        coupon = {"coupon_code": "DISCOUNT25", "items": [{"id": "xxx", "name": "item1", "unit_price": 100, "units": 2}]}
        
        # response = self.client.post('/coupons/apply/', json.dumps(coupon), content_type='application/json')
        response = self.client.post(reverse('coupons:apply'), json.dumps(coupon), content_type='application/json')

        # Verify the response
        self.assertEqual(response.status_code, 200)
        response_json = response.json()
        self.assertEqual(response_json['final_price'], 150)

if __name__ == '__main__':
    unittest.main()