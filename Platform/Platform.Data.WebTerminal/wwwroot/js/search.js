// Universal Search Engine JavaScript
(function () {
    'use strict';

    let filterCount = 0;

    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function () {
        initializeEventListeners();
        addFilterRow(); // Add initial filter row
    });

    function initializeEventListeners() {
        document.getElementById('add-filter-btn').addEventListener('click', addFilterRow);
        document.getElementById('search-btn').addEventListener('click', performSearch);
        document.getElementById('clear-btn').addEventListener('click', clearAllFilters);
    }

    function addFilterRow() {
        const container = document.getElementById('filters-container');
        const filterId = `filter-${filterCount++}`;

        const filterRow = document.createElement('div');
        filterRow.className = 'filter-row';
        filterRow.id = filterId;

        // Create property select
        const propertySelect = document.createElement('select');
        propertySelect.className = 'form-control filter-property';
        propertySelect.innerHTML = '<option value="">Choose a Filter</option>';

        availableProperties.forEach(function (prop) {
            const option = document.createElement('option');
            option.value = prop;
            option.textContent = prop;
            propertySelect.appendChild(option);
        });

        // Create min value input
        const minInput = document.createElement('input');
        minInput.type = 'text';
        minInput.className = 'form-control filter-min';
        minInput.placeholder = 'Minimum';

        // Create max value input
        const maxInput = document.createElement('input');
        maxInput.type = 'text';
        maxInput.className = 'form-control filter-max';
        maxInput.placeholder = 'Maximum';

        // Create remove button
        const removeBtn = document.createElement('button');
        removeBtn.className = 'btn btn-danger filter-remove';
        removeBtn.innerHTML = '✕';
        removeBtn.addEventListener('click', function () {
            removeFilterRow(filterId);
        });

        // Create arrow icon
        const arrow = document.createElement('span');
        arrow.className = 'filter-arrow';
        arrow.innerHTML = '→';

        // Assemble the row
        filterRow.appendChild(propertySelect);
        filterRow.appendChild(arrow);
        filterRow.appendChild(minInput);
        filterRow.appendChild(maxInput);
        filterRow.appendChild(removeBtn);

        container.appendChild(filterRow);
    }

    function removeFilterRow(filterId) {
        const filterRow = document.getElementById(filterId);
        if (filterRow) {
            filterRow.remove();
        }

        // Ensure at least one filter row exists
        const remainingFilters = document.querySelectorAll('.filter-row');
        if (remainingFilters.length === 0) {
            addFilterRow();
        }
    }

    function clearAllFilters() {
        const container = document.getElementById('filters-container');
        container.innerHTML = '';
        filterCount = 0;
        addFilterRow();

        // Clear results
        document.getElementById('results-count').innerHTML = '';
        document.getElementById('results-list').innerHTML = '';
    }

    function collectFilters() {
        const filters = [];
        const filterRows = document.querySelectorAll('.filter-row');

        filterRows.forEach(function (row) {
            const property = row.querySelector('.filter-property').value;
            const minValue = row.querySelector('.filter-min').value;
            const maxValue = row.querySelector('.filter-max').value;

            if (property) {
                filters.push({
                    PropertyName: property,
                    MinValue: minValue,
                    MaxValue: maxValue
                });
            }
        });

        return filters;
    }

    function performSearch() {
        const filters = collectFilters();
        const searchBtn = document.getElementById('search-btn');

        // Show loading state
        searchBtn.disabled = true;
        searchBtn.innerHTML = '<span>⏳</span> Searching...';

        // Prepare request
        const request = {
            Filters: filters
        };

        // Send AJAX request
        fetch('/Search/Search', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(request)
        })
            .then(response => response.json())
            .then(data => {
                displayResults(data);
            })
            .catch(error => {
                console.error('Error:', error);
                alert('An error occurred while searching: ' + error.message);
            })
            .finally(() => {
                // Reset button state
                searchBtn.disabled = false;
                searchBtn.innerHTML = '<span>🔍</span> Search';
            });
    }

    function displayResults(data) {
        const countDiv = document.getElementById('results-count');
        const listDiv = document.getElementById('results-list');

        if (data.error) {
            countDiv.innerHTML = `<div class="alert alert-danger">Error: ${data.error}</div>`;
            listDiv.innerHTML = '';
            return;
        }

        const totalCount = data.totalCount || 0;
        countDiv.innerHTML = `<p>Found <strong>${totalCount}</strong> result(s)</p>`;

        if (totalCount === 0) {
            listDiv.innerHTML = '<p class="text-muted">No results found. Try adjusting your filters.</p>';
            return;
        }

        // Display results
        let html = '<div class="list-group">';
        data.results.forEach(function (result) {
            const link = result.link;
            const linkId = link.id || link.toInt || 'N/A';
            const source = link.source || 'N/A';
            const target = link.target || 'N/A';
            const referers = link.totalReferers || 0;

            html += `
                <div class="list-group-item">
                    <h5 class="mb-1">Link #${linkId}</h5>
                    <p class="mb-1">
                        <strong>Source:</strong> ${source} |
                        <strong>Target:</strong> ${target} |
                        <strong>Referers:</strong> ${referers}
                    </p>
                    <small><a href="/Links/Index/${linkId}" target="_blank">View Details →</a></small>
                </div>
            `;
        });
        html += '</div>';

        listDiv.innerHTML = html;
    }
})();
