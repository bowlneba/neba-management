#!/bin/bash
# Docker-based local development helper for GitHub Pages
# This avoids native gem compilation issues

set -e

echo "🎨 Syncing theme file..."
cp ../src/frontend/Neba.Web.Server/wwwroot/neba_theme.css assets/css/neba_theme.css

echo "🐳 Starting Jekyll server with Docker..."
echo "Visit: http://localhost:5300/neba-management/"
echo ""
echo "Press Ctrl+C to stop the server"
echo ""

docker run --rm \
  --volume="$PWD:/srv/jekyll:Z" \
  --publish 5300:4000 \
  jekyll/jekyll:latest \
  jekyll serve --baseurl /neba-management --host 0.0.0.0
