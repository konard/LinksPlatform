#!/bin/bash

# Script to check if wiki pages are in sync between English and Russian versions
# This helps maintain consistency across translations

set -e

WIKI_REPO="https://github.com/konard/LinksPlatform.wiki.git"
WIKI_DIR="wiki-temp-$$"
MAPPING_FILE=".github/wiki-pages-mapping.json"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "============================================"
echo "Wiki Pages Synchronization Check"
echo "============================================"
echo ""

# Check if jq is available
if ! command -v jq &> /dev/null; then
    echo -e "${YELLOW}Warning: jq is not installed. Using basic parsing.${NC}"
    echo "For better results, install jq: https://stedolan.github.io/jq/"
    USE_JQ=false
else
    USE_JQ=true
fi

# Clone wiki repository
echo "Cloning wiki repository..."
if [ -d "$WIKI_DIR" ]; then
    rm -rf "$WIKI_DIR"
fi
git clone --quiet "$WIKI_REPO" "$WIKI_DIR"

# Function to get last modification time of a file in the wiki repo
get_last_modified() {
    local file=$1
    local wiki_path="$WIKI_DIR/$file"

    if [ -f "$wiki_path" ]; then
        # Get the timestamp of the last commit that modified this file
        cd "$WIKI_DIR"
        timestamp=$(git log -1 --format="%at" -- "$file" 2>/dev/null || echo "0")
        cd - > /dev/null
        echo "$timestamp"
    else
        echo "0"
    fi
}

# Function to get human-readable date
get_date() {
    local timestamp=$1
    if [ "$timestamp" = "0" ]; then
        echo "Never"
    else
        date -d "@$timestamp" "+%Y-%m-%d %H:%M:%S" 2>/dev/null || date -r "$timestamp" "+%Y-%m-%d %H:%M:%S" 2>/dev/null || echo "Unknown"
    fi
}

# Check if mapping file exists
if [ ! -f "$MAPPING_FILE" ]; then
    echo -e "${RED}Error: Mapping file not found at $MAPPING_FILE${NC}"
    rm -rf "$WIKI_DIR"
    exit 1
fi

echo "Checking page pairs..."
echo ""

OUT_OF_SYNC=0
TOTAL_PAIRS=0

# Parse the mapping file and check each pair
if [ "$USE_JQ" = true ]; then
    # Use jq for proper JSON parsing
    pairs=$(jq -c '.pairs[]' "$MAPPING_FILE")

    while IFS= read -r pair; do
        english=$(echo "$pair" | jq -r '.english')
        russian=$(echo "$pair" | jq -r '.russian')
        description=$(echo "$pair" | jq -r '.description')

        TOTAL_PAIRS=$((TOTAL_PAIRS + 1))

        echo "Checking: $description"
        echo "  English: $english"
        echo "  Russian: $russian"

        en_time=$(get_last_modified "$english")
        ru_time=$(get_last_modified "$russian")

        en_date=$(get_date "$en_time")
        ru_date=$(get_date "$ru_time")

        echo "  Last modified (EN): $en_date"
        echo "  Last modified (RU): $ru_date"

        if [ "$en_time" = "0" ] || [ "$ru_time" = "0" ]; then
            echo -e "  ${RED}⚠ One or both pages don't exist!${NC}"
            OUT_OF_SYNC=$((OUT_OF_SYNC + 1))
        else
            time_diff=$((en_time - ru_time))
            time_diff_abs=${time_diff#-}

            # If modified within 1 hour of each other, consider them in sync
            if [ "$time_diff_abs" -lt 3600 ]; then
                echo -e "  ${GREEN}✓ Pages are in sync${NC}"
            else
                days_diff=$((time_diff_abs / 86400))
                if [ "$en_time" -gt "$ru_time" ]; then
                    echo -e "  ${YELLOW}⚠ English version is newer by ~$days_diff days${NC}"
                else
                    echo -e "  ${YELLOW}⚠ Russian version is newer by ~$days_diff days${NC}"
                fi
                OUT_OF_SYNC=$((OUT_OF_SYNC + 1))
            fi
        fi

        echo ""
    done <<< "$pairs"
else
    # Basic parsing without jq (less reliable but functional)
    echo -e "${YELLOW}Using basic JSON parsing (install jq for better results)${NC}"
    echo ""

    # Hardcoded pairs as fallback
    declare -a EN_PAGES=("FAQ.md" "How-it-all-began.md" "Coding-conventions.md" "Visualization-primes.md")
    declare -a RU_PAGES=("ЧАВО.md" "О-том,-как-всё-начиналось.md" "Соглашения-о-коде.md" "Примитивы-визуализации.md")
    declare -a DESCRIPTIONS=("FAQ" "About the beginning" "Coding conventions" "Visualization primitives")

    for i in "${!EN_PAGES[@]}"; do
        english="${EN_PAGES[$i]}"
        russian="${RU_PAGES[$i]}"
        description="${DESCRIPTIONS[$i]}"

        TOTAL_PAIRS=$((TOTAL_PAIRS + 1))

        echo "Checking: $description"
        echo "  English: $english"
        echo "  Russian: $russian"

        en_time=$(get_last_modified "$english")
        ru_time=$(get_last_modified "$russian")

        en_date=$(get_date "$en_time")
        ru_date=$(get_date "$ru_time")

        echo "  Last modified (EN): $en_date"
        echo "  Last modified (RU): $ru_date"

        if [ "$en_time" = "0" ] || [ "$ru_time" = "0" ]; then
            echo -e "  ${RED}⚠ One or both pages don't exist!${NC}"
            OUT_OF_SYNC=$((OUT_OF_SYNC + 1))
        else
            time_diff=$((en_time - ru_time))
            time_diff_abs=${time_diff#-}

            if [ "$time_diff_abs" -lt 3600 ]; then
                echo -e "  ${GREEN}✓ Pages are in sync${NC}"
            else
                days_diff=$((time_diff_abs / 86400))
                if [ "$en_time" -gt "$ru_time" ]; then
                    echo -e "  ${YELLOW}⚠ English version is newer by ~$days_diff days${NC}"
                else
                    echo -e "  ${YELLOW}⚠ Russian version is newer by ~$days_diff days${NC}"
                fi
                OUT_OF_SYNC=$((OUT_OF_SYNC + 1))
            fi
        fi

        echo ""
    done
fi

# Cleanup
rm -rf "$WIKI_DIR"

# Summary
echo "============================================"
echo "Summary"
echo "============================================"
echo "Total page pairs checked: $TOTAL_PAIRS"
echo "Pairs in sync: $((TOTAL_PAIRS - OUT_OF_SYNC))"
echo "Pairs out of sync: $OUT_OF_SYNC"
echo ""

if [ "$OUT_OF_SYNC" -eq 0 ]; then
    echo -e "${GREEN}✓ All wiki pages are in sync!${NC}"
    exit 0
else
    echo -e "${YELLOW}⚠ Some wiki pages may need synchronization.${NC}"
    echo "Please review the pages marked above and update them accordingly."
    echo ""
    echo "See .github/WIKI_SYNC_GUIDE.md for instructions on how to keep pages in sync."
    exit 1
fi
