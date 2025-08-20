#!/bin/bash

# Spinner.Net Temporary File Cleanup Script
# Removes all screenshots, deployment files, and testing artifacts from git repo

echo "🧹 Cleaning up temporary files from Spinner.Net repository..."

# Project root cleanup
cd /Users/peterclinton/Desktop/repos/Spinner.Net/Public

# Remove screenshot files (various extensions and naming patterns)
rm -f *.png *.jpg *.jpeg *.gif
rm -f *screenshot* *-screenshot* screenshot-*
rm -f *-test* *test* test-*
rm -f *-form-* *form* form-*
rm -f after-* before-* during-*
rm -f manual-* auto-* fixed-*
rm -f persona-* step*-* 
rm -f deployment*.zip *.zip

echo "  ✅ Cleaned project root"

# Src directory cleanup  
cd src
rm -f *.png *.jpg *.jpeg *.gif
rm -f *screenshot* *-screenshot* screenshot-*
rm -f *-test* *test* test-*
rm -f *-form-* *form* form-*
rm -f after-* before-* during-*
rm -f manual-* auto-* fixed-*
rm -f persona-* step*-*
rm -f deployment*.zip *.zip

echo "  ✅ Cleaned src directory"

# Ensure temp directory exists and is clean
mkdir -p /tmp/spinnernet-temp
rm -rf /tmp/spinnernet-temp/*

echo "  ✅ Reset temp directory: /tmp/spinnernet-temp/"

echo "🎉 Cleanup complete! Repository is clean of temporary files."
echo "📝 Remember: Always use /tmp/spinnernet-temp/ for future screenshots and deployment files!"