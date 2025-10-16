// Graph Visualization JavaScript for WebTerminal
// Provides interactive D3.js-based graph visualization

(function() {
    'use strict';

    var svg, g, simulation, link, node, zoom;
    var width = 900;
    var height = 600;
    var showLabels = true;

    $(document).ready(function() {
        if (typeof graphData === 'undefined') {
            console.error('Graph data not found');
            return;
        }

        initializeGraph();
        setupControls();
    });

    function initializeGraph() {
        var container = d3.select('#graph-container');
        container.html('');

        width = document.getElementById('graph-container').clientWidth || 900;

        // Create SVG
        svg = container.append('svg')
            .attr('width', width)
            .attr('height', height);

        // Add zoom behavior
        zoom = d3.zoom()
            .scaleExtent([0.1, 4])
            .on('zoom', function(event) {
                g.attr('transform', event.transform);
            });

        svg.call(zoom);

        g = svg.append('g');

        // Create force simulation
        simulation = d3.forceSimulation(graphData.nodes)
            .force('link', d3.forceLink(graphData.links)
                .id(function(d) { return d.id; })
                .distance(100))
            .force('charge', d3.forceManyBody().strength(-300))
            .force('center', d3.forceCenter(width / 2, height / 2))
            .force('collision', d3.forceCollide().radius(30));

        // Create links
        link = g.append('g')
            .selectAll('line')
            .data(graphData.links)
            .enter().append('line')
            .attr('class', 'link');

        // Create nodes
        node = g.append('g')
            .selectAll('g')
            .data(graphData.nodes)
            .enter().append('g')
            .attr('class', 'node')
            .call(d3.drag()
                .on('start', dragstarted)
                .on('drag', dragged)
                .on('end', dragended));

        // Add circles to nodes
        node.append('circle')
            .attr('r', function(d) { return d.level === 0 ? 15 : 10; })
            .attr('fill', function(d) {
                if (d.level === 0) return '#4CAF50';
                if (d.level === 1) return '#2196F3';
                return '#9E9E9E';
            });

        // Add labels to nodes
        node.append('text')
            .attr('dx', 12)
            .attr('dy', '.35em')
            .text(function(d) { return d.label; })
            .style('display', showLabels ? 'block' : 'none');

        // Add title for tooltips
        node.append('title')
            .text(function(d) { return 'Link: ' + d.label; });

        // Update positions on tick
        simulation.on('tick', function() {
            link
                .attr('x1', function(d) { return d.source.x; })
                .attr('y1', function(d) { return d.source.y; })
                .attr('x2', function(d) { return d.target.x; })
                .attr('y2', function(d) { return d.target.y; });

            node
                .attr('transform', function(d) {
                    return 'translate(' + d.x + ',' + d.y + ')';
                });
        });
    }

    function setupControls() {
        $('#zoom-in').click(function() {
            svg.transition().call(zoom.scaleBy, 1.3);
        });

        $('#zoom-out').click(function() {
            svg.transition().call(zoom.scaleBy, 0.7);
        });

        $('#reset-view').click(function() {
            svg.transition().call(zoom.transform, d3.zoomIdentity);
        });

        $('#toggle-labels').click(function() {
            showLabels = !showLabels;
            node.selectAll('text').style('display', showLabels ? 'block' : 'none');
        });
    }

    function dragstarted(event, d) {
        if (!event.active) simulation.alphaTarget(0.3).restart();
        d.fx = d.x;
        d.fy = d.y;
    }

    function dragged(event, d) {
        d.fx = event.x;
        d.fy = event.y;
    }

    function dragended(event, d) {
        if (!event.active) simulation.alphaTarget(0);
        d.fx = null;
        d.fy = null;
    }

    // Resize handler
    $(window).resize(function() {
        width = document.getElementById('graph-container').clientWidth || 900;
        svg.attr('width', width);
        simulation.force('center', d3.forceCenter(width / 2, height / 2));
        simulation.alpha(0.3).restart();
    });
})();
