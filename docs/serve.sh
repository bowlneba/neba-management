#!/bin/bash
# Local development helper for GitHub Pages

set -e

echo "🎨 Syncing theme file..."
cp ../src/frontend/Neba.Web.Server/wwwroot/neba_theme.css assets/css/neba_theme.css

echo "📦 Installing dependencies..."
# Use local bundle path to avoid system-wide installation
bundle install --path vendor/bundle --quiet

echo "🚀 Starting Jekyll server..."
echo "Visit: http://localhost:5300/neba-management/"
echo ""
bundle exec jekyll serve --port 5300
