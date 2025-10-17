#!/bin/bash

# Test script for the Global Autocomplete Service API
# This script demonstrates how to use the autocomplete service

BASE_URL="http://localhost:5000/api/autocomplete"

echo "=== Global Autocomplete Service Test Script ==="
echo ""

# 1. Index some sample text
echo "1. Indexing sample text..."
curl -X POST "$BASE_URL/index" \
  -H "Content-Type: application/json" \
  -d '{
    "text": "To be, or not to be, that is the question.\nTo implement a feature, you need to write code.\nTo test your code, run the tests.\nTo deploy the application, follow the deployment guide."
  }'
echo -e "\n"

# 2. Get stats
echo "2. Getting service statistics..."
curl -X GET "$BASE_URL/stats"
echo -e "\n"

# 3. Test autocomplete suggestions
echo "3. Testing autocomplete suggestions..."

echo "   Suggestions for 'To':"
curl -X GET "$BASE_URL/suggest?prefix=To&maxResults=5"
echo -e "\n"

echo "   Suggestions for 'To be':"
curl -X GET "$BASE_URL/suggest?prefix=To%20be&maxResults=5"
echo -e "\n"

echo "   Suggestions for 'the':"
curl -X GET "$BASE_URL/suggest?prefix=the&maxResults=5"
echo -e "\n"

# 4. Test POST method for suggestions
echo "4. Testing POST method for suggestions..."
curl -X POST "$BASE_URL/suggest" \
  -H "Content-Type: application/json" \
  -d '{
    "prefix": "To",
    "maxResults": 10
  }'
echo -e "\n"

echo "=== Test Complete ==="
