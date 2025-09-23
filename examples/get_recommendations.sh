#!/bin/bash

# Example script to get code recommendations

echo "Code Recommendations Example"
echo "============================"

cd Platform/Platform.CodeEvolution

echo "Getting recommendations for C# code with old-style string concatenation..."
echo ""

dotnet run -- recommend \
    --code "string result = \"Hello \" + name + \"!\";" \
    --language "C#"

echo ""
echo "Getting recommendations for JavaScript code with var declaration..."
echo ""

dotnet run -- recommend \
    --code "var users = getUsers(); var count = users.length;" \
    --language "JavaScript"

echo ""
echo "Note: To get more accurate recommendations based on real project data,"
echo "first run the analyze command on a repository with substantial commit history."