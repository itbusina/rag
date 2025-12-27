#!/bin/bash

# Exit on error
set -e

echo "🔨 Building Next.js app..."
cd src/web
npm run build

echo "🧹 Cleaning api/wwwroot folder..."
cd ../api
rm -rf wwwroot/*

echo "📦 Copying built files to api/wwwroot..."

cp -r ../web/out/* wwwroot/

mkdir -p wwwroot/css
cp -r ../api/widget/css/* wwwroot/css/

echo "✅ Build and deploy completed successfully!"
echo "📁 Files copied to: $(pwd)/wwwroot"

