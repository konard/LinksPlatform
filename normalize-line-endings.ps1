# Cross-platform PowerShell script to normalize line endings in all text files
# Works on Windows PowerShell and PowerShell Core (Linux/macOS)

Write-Host "Normalizing line endings for all files in the repository..." -ForegroundColor Cyan
Write-Host "This will reset all files according to .gitattributes settings." -ForegroundColor Cyan
Write-Host ""

# Check if .gitattributes exists
if (-not (Test-Path ".gitattributes")) {
    Write-Host "Error: .gitattributes file not found!" -ForegroundColor Red
    Write-Host "Please ensure .gitattributes is present in the repository root." -ForegroundColor Red
    exit 1
}

# Check for uncommitted changes
Write-Host "Step 1: Checking for uncommitted changes..." -ForegroundColor Yellow
$hasChanges = git diff-index --quiet HEAD --
if ($LASTEXITCODE -ne 0) {
    Write-Host "Warning: You have uncommitted changes." -ForegroundColor Yellow
    $response = Read-Host "Do you want to continue? (y/n)"
    if ($response -ne "y" -and $response -ne "Y") {
        Write-Host "Aborted." -ForegroundColor Red
        exit 1
    }
}

# Remove all files from Git's index
Write-Host "Step 2: Removing all files from Git index..." -ForegroundColor Yellow
git rm --cached -r . 2>&1 | Out-Null

# Reset the index
Write-Host "Step 3: Re-adding files with normalized line endings..." -ForegroundColor Yellow
git reset 2>&1 | Out-Null

# Re-add all files (Git will normalize them according to .gitattributes)
git add -A

# Show what changed
Write-Host ""
Write-Host "Step 4: Checking for changes..." -ForegroundColor Yellow
$hasChanges = git diff --cached --quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "No line ending changes needed. All files already have correct line endings." -ForegroundColor Green
} else {
    Write-Host "The following files will have their line endings normalized:" -ForegroundColor Green
    git diff --cached --name-only
    Write-Host ""
    Write-Host "To commit these changes, run:" -ForegroundColor Cyan
    Write-Host '  git commit -m "Normalize line endings"' -ForegroundColor White
}

Write-Host ""
Write-Host "Done! Line endings have been normalized according to .gitattributes." -ForegroundColor Green
