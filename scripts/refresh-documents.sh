#!/bin/bash

# Script to refresh cached documents
# This regenerates the HTML from Google Docs with the latest DocumentMapper code

set -e

# Default base URL (can be overridden with environment variable)
BASE_URL="${BASE_URL:-http://localhost:5150}"

echo "🔄 Refreshing NEBA documents..."
echo "Using base URL: $BASE_URL"
echo ""

# Refresh Bylaws
echo "📄 Refreshing Bylaws..."
BYLAWS_RESPONSE=$(curl -s -X POST "$BASE_URL/bylaws/refresh" \
  -H "Content-Type: application/json" \
  -w "\n%{http_code}")

BYLAWS_HTTP_CODE=$(echo "$BYLAWS_RESPONSE" | tail -n1)
BYLAWS_BODY=$(echo "$BYLAWS_RESPONSE" | sed '$d')

if [[ "$BYLAWS_HTTP_CODE" -eq 200 ]]; then
  echo "✅ Bylaws refresh triggered successfully"
  echo "   Job ID: $(echo "$BYLAWS_BODY" | grep -o '"data":"[^"]*"' | cut -d'"' -f4)"
else
  echo "❌ Failed to refresh Bylaws (HTTP $BYLAWS_HTTP_CODE)"
  echo "   Response: $BYLAWS_BODY"
  exit 1
fi

echo ""

# Refresh Tournament Rules
echo "📄 Refreshing Tournament Rules..."
RULES_RESPONSE=$(curl -s -X POST "$BASE_URL/tournaments/rules/refresh" \
  -H "Content-Type: application/json" \
  -w "\n%{http_code}")

RULES_HTTP_CODE=$(echo "$RULES_RESPONSE" | tail -n1)
RULES_BODY=$(echo "$RULES_RESPONSE" | sed '$d')

if [[ "$RULES_HTTP_CODE" -eq 200 ]]; then
  echo "✅ Tournament Rules refresh triggered successfully"
  echo "   Job ID: $(echo "$RULES_BODY" | grep -o '"data":"[^"]*"' | cut -d'"' -f4)"
else
  echo "❌ Failed to refresh Tournament Rules (HTTP $RULES_HTTP_CODE)"
  echo "   Response: $RULES_BODY"
  exit 1
fi

echo ""
echo "✨ All documents refreshed! The cache should be updated shortly."
echo ""
echo "💡 You can monitor the refresh status at:"
echo "   - Bylaws: $BASE_URL/bylaws/refresh/status"
echo "   - Tournament Rules: $BASE_URL/tournaments/rules/refresh/status"
