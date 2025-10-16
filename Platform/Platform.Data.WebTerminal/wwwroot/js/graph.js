var cy = null;
var currentLayout = 'cose';

// Convert LinkModel hierarchy to Cytoscape elements
function convertModelToElements(linkModel, visitedIds = new Set()) {
    var elements = [];

    function processLink(model, parentId = null) {
        var linkId = model.Link.Id.toString();

        // Avoid infinite loops with circular references
        if (visitedIds.has(linkId)) {
            return;
        }
        visitedIds.add(linkId);

        // Add node
        var nodeLabel = linkId;
        if (model.Link.Source && model.Link.Target && model.Link.Linker) {
            nodeLabel = linkId + '\n(' + model.Link.Source.Id + '→' + model.Link.Target.Id + ')';
        }

        elements.push({
            data: {
                id: linkId,
                label: nodeLabel,
                linkData: model.Link
            }
        });

        // Add edge from parent if exists
        if (parentId !== null) {
            elements.push({
                data: {
                    id: parentId + '-' + linkId,
                    source: parentId,
                    target: linkId
                }
            });
        }

        // Process referers (children)
        if (model.ReferersModels && model.ReferersModels.length > 0) {
            model.ReferersModels.forEach(function(referer) {
                processLink(referer, linkId);
            });
        }
    }

    processLink(linkModel);
    return elements;
}

// Apply layout to graph
function applyLayout(layoutName) {
    if (!cy) return;

    var layoutOptions = {
        name: layoutName,
        animate: true,
        animationDuration: 500,
        fit: true,
        padding: 50
    };

    // Specific options for different layouts
    if (layoutName === 'cose') {
        layoutOptions.nodeRepulsion = 8000;
        layoutOptions.idealEdgeLength = 100;
        layoutOptions.edgeElasticity = 100;
        layoutOptions.nestingFactor = 5;
    } else if (layoutName === 'breadthfirst') {
        layoutOptions.directed = true;
        layoutOptions.spacingFactor = 1.5;
    } else if (layoutName === 'concentric') {
        layoutOptions.minNodeSpacing = 100;
    }

    cy.layout(layoutOptions).run();
    currentLayout = layoutName;
}

// Update info display
function updateInfo() {
    if (!cy) return;

    var nodeCount = cy.nodes().length;
    var edgeCount = cy.edges().length;

    $('#node-count').text('Nodes: ' + nodeCount);
    $('#edge-count').text('Edges: ' + edgeCount);
}

// Show node details
function showNodeInfo(node) {
    var data = node.data();
    var linkData = data.linkData;

    var html = '<p><strong>ID:</strong> ' + data.id + '</p>';

    if (linkData) {
        if (linkData.Source) {
            html += '<p><strong>Source:</strong> ' + linkData.Source.Id + '</p>';
        }
        if (linkData.Linker) {
            html += '<p><strong>Linker:</strong> ' + linkData.Linker.Id + '</p>';
        }
        if (linkData.Target) {
            html += '<p><strong>Target:</strong> ' + linkData.Target.Id + '</p>';
        }
        if (linkData.TotalReferers !== undefined) {
            html += '<p><strong>Total Referers:</strong> ' + linkData.TotalReferers + '</p>';
        }
    }

    $('#node-info-content').html(html);
    $('#node-info').fadeIn(200);
}

// Hide node details
function hideNodeInfo() {
    $('#node-info').fadeOut(200);
}

// Initialize Cytoscape
function initCytoscape(elements) {
    cy = cytoscape({
        container: document.getElementById('cy'),

        elements: elements,

        style: [
            {
                selector: 'node',
                style: {
                    'background-color': '#0099cc',
                    'label': 'data(label)',
                    'text-valign': 'center',
                    'text-halign': 'center',
                    'font-size': '12px',
                    'color': '#fff',
                    'text-outline-color': '#0099cc',
                    'text-outline-width': 2,
                    'width': 40,
                    'height': 40,
                    'text-wrap': 'wrap',
                    'text-max-width': '100px'
                }
            },
            {
                selector: 'node:selected',
                style: {
                    'background-color': '#00d4ff',
                    'border-width': 3,
                    'border-color': '#fff'
                }
            },
            {
                selector: 'node:active',
                style: {
                    'overlay-color': '#00d4ff',
                    'overlay-padding': 10,
                    'overlay-opacity': 0.3
                }
            },
            {
                selector: 'edge',
                style: {
                    'width': 2,
                    'line-color': '#666',
                    'target-arrow-color': '#666',
                    'target-arrow-shape': 'triangle',
                    'curve-style': 'bezier',
                    'arrow-scale': 1
                }
            },
            {
                selector: 'edge:selected',
                style: {
                    'line-color': '#00d4ff',
                    'target-arrow-color': '#00d4ff',
                    'width': 3
                }
            },
            {
                selector: '.highlighted',
                style: {
                    'background-color': '#00c800',
                    'line-color': '#00c800',
                    'target-arrow-color': '#00c800',
                    'transition-property': 'background-color, line-color, target-arrow-color',
                    'transition-duration': '0.3s'
                }
            }
        ],

        layout: {
            name: 'cose',
            animate: true,
            animationDuration: 500,
            nodeRepulsion: 8000,
            idealEdgeLength: 100,
            edgeElasticity: 100,
            nestingFactor: 5,
            fit: true,
            padding: 50
        },

        // Interaction options
        wheelSensitivity: 0.2,
        minZoom: 0.1,
        maxZoom: 3
    });

    // Event handlers
    cy.on('tap', 'node', function(evt) {
        var node = evt.target;

        // Highlight node and its neighbors
        cy.elements().removeClass('highlighted');
        node.addClass('highlighted');
        node.neighborhood().addClass('highlighted');

        // Show node info
        showNodeInfo(node);
    });

    cy.on('tap', function(evt) {
        if (evt.target === cy) {
            cy.elements().removeClass('highlighted');
            hideNodeInfo();
        }
    });

    // Double-click to navigate (if we implement navigation)
    cy.on('dbltap', 'node', function(evt) {
        var node = evt.target;
        var linkId = node.data('id');
        window.location.href = '/Links/Graph?id=' + linkId;
    });

    updateInfo();
}

$(document).ready(function() {
    // Check if linkModelData is available
    if (typeof linkModelData === 'undefined') {
        console.error('Link model data not found');
        return;
    }

    // Convert model to elements
    var elements = convertModelToElements(linkModelData);

    // Initialize Cytoscape
    initCytoscape(elements);

    // Toolbar controls
    $('#btn-fit').click(function() {
        cy.fit(null, 50);
    });

    $('#btn-center').click(function() {
        cy.center();
    });

    $('#btn-layout').click(function() {
        applyLayout(currentLayout);
    });

    $('#layout-selector').change(function() {
        var layoutName = $(this).val();
        applyLayout(layoutName);
    });

    // Keyboard shortcuts
    $(document).keydown(function(e) {
        // F - Fit
        if (e.which === 70) {
            cy.fit(null, 50);
            return false;
        }
        // C - Center
        if (e.which === 67) {
            cy.center();
            return false;
        }
        // R - Re-layout
        if (e.which === 82) {
            applyLayout(currentLayout);
            return false;
        }
    });
});

// Handle window resize
$(window).resize(function() {
    if (cy) {
        cy.resize();
        cy.fit(null, 50);
    }
});
