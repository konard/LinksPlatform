#!/bin/bash
# LinksPlatform API Documentation Generator
# Generates both HTML documentation and OpenAPI specifications

set -e # Exit with nonzero exit code if anything fails

SOURCE_BRANCH="master"
TARGET_BRANCH="gh-pages"

# Save some useful information
SHA=`git rev-parse --verify HEAD`
COMMIT_AUTHOR_EMAIL="konard@me.com"

echo "Starting documentation generation..."

# DocFX installation and generation
echo "Installing DocFX..."
nuget install docfx.console

echo "Generating HTML documentation..."
mono $(ls | grep "docfx.console.")/tools/docfx.exe docfx.json

echo "Converting HTML to OpenAPI specifications..."

# Create openapi directory in generated site
mkdir -p doc/generated/site/openapi

# Find all HTML files and convert them to OpenAPI
find doc/generated/site -name "*.html" -type f | while read html_file; do
    # Skip index and layout files
    if [[ "$html_file" == *"index.html"* ]] || [[ "$html_file" == *"_layout"* ]]; then
        continue
    fi

    # Get relative path and create OpenAPI filename
    rel_path=${html_file#doc/generated/site/}
    openapi_file="doc/generated/site/openapi/${rel_path%.*}.openapi.yaml"

    # Create directory if it doesn't exist
    mkdir -p "$(dirname "$openapi_file")"

    # Convert HTML to OpenAPI
    echo "Converting $html_file to $openapi_file"
    python3 improved_html_to_openapi.py "$html_file" "$openapi_file" 2>/dev/null || echo "  Warning: Could not convert $html_file"
done

# Generate a master OpenAPI index
echo "Generating OpenAPI index..."
cat > doc/generated/site/openapi/index.html << 'EOF'
<!DOCTYPE html>
<html>
<head>
    <title>LinksPlatform OpenAPI Specifications</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        .spec-list { list-style-type: none; }
        .spec-list li { margin: 10px 0; }
        .spec-list a { text-decoration: none; color: #007acc; }
        .spec-list a:hover { text-decoration: underline; }
    </style>
</head>
<body>
    <h1>LinksPlatform OpenAPI Specifications</h1>
    <p>Auto-generated OpenAPI specifications from HTML documentation.</p>

    <h2>Available API Specifications</h2>
    <ul class="spec-list">
EOF

# Add links to all generated OpenAPI files
find doc/generated/site/openapi -name "*.openapi.yaml" -type f | while read openapi_file; do
    rel_path=${openapi_file#doc/generated/site/openapi/}
    echo "        <li><a href=\"$rel_path\">$rel_path</a></li>" >> doc/generated/site/openapi/index.html
done

cat >> doc/generated/site/openapi/index.html << 'EOF'
    </ul>

    <h2>Usage</h2>
    <p>These OpenAPI specifications can be used with:</p>
    <ul>
        <li><a href="https://swagger.io/">Swagger UI</a> for interactive documentation</li>
        <li><a href="https://insomnia.rest/">Insomnia</a> for API testing</li>
        <li><a href="https://www.postman.com/">Postman</a> for API development</li>
        <li>Code generation tools for client libraries</li>
    </ul>

    <p><a href="../index.html">← Back to Documentation</a></p>
</body>
</html>
EOF

echo "Documentation generation complete!"
echo "HTML documentation: doc/generated/site/"
echo "OpenAPI specifications: doc/generated/site/openapi/"

# Optional: Deploy to GitHub Pages (same as original script)
if [ "$1" == "--deploy" ]; then
    echo "Deploying to GitHub Pages..."

    # Clone the existing gh-pages for this repo into out/
    git clone https://github.com/LinksPlatform/Documentation out
    cd out
    git checkout $TARGET_BRANCH || git checkout --orphan $TARGET_BRANCH
    cd ..

    # Clean out existing contents
    rm -rf out/**/* || exit 0

    # Copy generated docs site (including OpenAPI specs)
    cp -r doc/generated/site/* out

    # Now let's go have some fun with the cloned repo
    cd out
    git config user.name "GitHub Actions"
    git config user.email "$COMMIT_AUTHOR_EMAIL"
    git remote rm origin
    git remote add origin https://linksplatform-docs:$TOKEN@github.com/LinksPlatform/Documentation.git

    # Commit the "changes", i.e. the new version.
    git add -A .
    git commit -m "Deploy documentation with OpenAPI specs: ${SHA}"

    # Now that we're all set up, we can push.
    git push https://linksplatform-docs:$TOKEN@github.com/LinksPlatform/Documentation.git $TARGET_BRANCH

    echo "Deployment complete!"
fi