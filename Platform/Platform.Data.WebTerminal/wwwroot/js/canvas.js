// Links Canvas - Interactive Visualization with Write Access
// Allows users to create links by clicking and drawing

class LinksCanvas {
    constructor(canvasId) {
        this.canvas = document.getElementById(canvasId);
        this.ctx = this.canvas.getContext('2d');
        this.links = [];
        this.nodes = [];
        this.isDrawing = false;
        this.drawStartPos = null;
        this.drawCurrentPos = null;
        this.hoveredNode = null;

        this.setupCanvas();
        this.setupEventListeners();
        this.render();
    }

    setupCanvas() {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }

    setupEventListeners() {
        this.canvas.addEventListener('click', this.handleClick.bind(this));
        this.canvas.addEventListener('mousedown', this.handleMouseDown.bind(this));
        this.canvas.addEventListener('mousemove', this.handleMouseMove.bind(this));
        this.canvas.addEventListener('mouseup', this.handleMouseUp.bind(this));
        window.addEventListener('resize', () => {
            this.setupCanvas();
            this.render();
        });
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isDrawing) {
                this.cancelDrawing();
            }
        });
    }

    handleClick(e) {
        if (this.isDrawing) return;

        const pos = this.getMousePos(e);
        const clickedNode = this.findNodeAt(pos);

        if (!clickedNode) {
            // Create a point that transforms to point-link
            this.createPointLink(pos);
        }
    }

    handleMouseDown(e) {
        const pos = this.getMousePos(e);
        const node = this.findNodeAt(pos);

        // Start drawing a line
        this.isDrawing = true;
        this.drawStartPos = node ? { x: node.x, y: node.y, node: node } : { x: pos.x, y: pos.y, node: null };
        this.drawCurrentPos = { x: pos.x, y: pos.y };
    }

    handleMouseMove(e) {
        const pos = this.getMousePos(e);

        // Update hovered node
        const newHoveredNode = this.findNodeAt(pos);
        if (newHoveredNode !== this.hoveredNode) {
            this.hoveredNode = newHoveredNode;
            this.render();
        }

        if (this.isDrawing) {
            this.drawCurrentPos = { x: pos.x, y: pos.y };
            this.render();
        }
    }

    handleMouseUp(e) {
        if (!this.isDrawing) return;

        const pos = this.getMousePos(e);
        const endNode = this.findNodeAt(pos);
        const startNode = this.drawStartPos.node;

        // Determine the type of link to create
        if (!startNode && !endNode) {
            // Line with both ends free - creates a point-link (floating link)
            this.createFloatingLink(this.drawStartPos, pos);
        } else if (startNode && !endNode) {
            // One end on existing link - creates partial point-link
            this.createPartialLink(startNode, pos);
        } else if (!startNode && endNode) {
            // One end on existing link - creates partial point-link
            this.createPartialLink(endNode, this.drawStartPos);
        } else if (startNode && endNode && startNode !== endNode) {
            // Both ends on existing links - creates regular link
            this.createRegularLink(startNode, endNode);
        }

        this.cancelDrawing();
    }

    cancelDrawing() {
        this.isDrawing = false;
        this.drawStartPos = null;
        this.drawCurrentPos = null;
        this.render();
    }

    createPointLink(pos) {
        const node = {
            id: this.generateId(),
            x: pos.x,
            y: pos.y,
            radius: 3,
            type: 'point',
            animating: true
        };

        this.nodes.push(node);

        // Animate transformation from point to point-link
        setTimeout(() => {
            node.radius = 8;
            node.type = 'point-link';
            this.render();

            setTimeout(() => {
                node.animating = false;
                this.render();
            }, 300);
        }, 100);

        this.createLinkOnServer(node.id, 0, 0); // Point-link with source=0, target=0
        this.render();
    }

    createFloatingLink(start, end) {
        const midX = (start.x + end.x) / 2;
        const midY = (start.y + end.y) / 2;

        const node = {
            id: this.generateId(),
            x: midX,
            y: midY,
            radius: 3,
            type: 'floating',
            animating: true
        };

        this.nodes.push(node);

        // Animate transformation
        setTimeout(() => {
            node.radius = 8;
            node.type = 'point-link';
            this.render();

            setTimeout(() => {
                node.animating = false;
                this.render();
            }, 300);
        }, 100);

        this.createLinkOnServer(node.id, 0, 0); // Floating link with source=0, target=0
        this.render();
    }

    createPartialLink(existingNode, newPos) {
        const newNode = {
            id: this.generateId(),
            x: newPos.x,
            y: newPos.y,
            radius: 8,
            type: 'node',
            animating: true
        };

        this.nodes.push(newNode);

        const link = {
            id: this.generateId(),
            source: existingNode,
            target: newNode,
            type: 'partial',
            animating: true
        };

        this.links.push(link);

        setTimeout(() => {
            newNode.animating = false;
            link.animating = false;
            this.render();
        }, 300);

        this.createLinkOnServer(link.id, existingNode.id, newNode.id);
        this.render();
    }

    createRegularLink(sourceNode, targetNode) {
        const link = {
            id: this.generateId(),
            source: sourceNode,
            target: targetNode,
            type: 'regular',
            animating: true
        };

        this.links.push(link);

        setTimeout(() => {
            link.animating = false;
            this.render();
        }, 300);

        this.createLinkOnServer(link.id, sourceNode.id, targetNode.id);
        this.render();
    }

    createLinkOnServer(linkId, sourceId, targetId) {
        // Send to server to create the actual link
        console.log(`Creating link: ${linkId}, source: ${sourceId}, target: ${targetId}`);

        $.post('/Links/Create', { source: sourceId, target: targetId }, (response) => {
            if (response.success) {
                console.log('Link created successfully:', response);
            } else {
                console.error('Failed to create link:', response.error);
            }
        }).fail((error) => {
            console.error('API call failed:', error);
        });
    }

    findNodeAt(pos) {
        for (let i = this.nodes.length - 1; i >= 0; i--) {
            const node = this.nodes[i];
            const dx = pos.x - node.x;
            const dy = pos.y - node.y;
            const distance = Math.sqrt(dx * dx + dy * dy);

            if (distance <= node.radius + 5) {
                return node;
            }
        }
        return null;
    }

    getMousePos(e) {
        const rect = this.canvas.getBoundingClientRect();
        return {
            x: e.clientX - rect.left,
            y: e.clientY - rect.top
        };
    }

    generateId() {
        return Date.now() + Math.random();
    }

    render() {
        // Clear canvas
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

        // Draw links
        this.links.forEach(link => {
            this.ctx.beginPath();
            this.ctx.moveTo(link.source.x, link.source.y);
            this.ctx.lineTo(link.target.x, link.target.y);

            if (link.animating) {
                this.ctx.strokeStyle = '#00C800';
                this.ctx.lineWidth = 3;
            } else {
                this.ctx.strokeStyle = '#00C800';
                this.ctx.lineWidth = 2;
            }

            this.ctx.stroke();
        });

        // Draw nodes
        this.nodes.forEach(node => {
            this.ctx.beginPath();
            this.ctx.arc(node.x, node.y, node.radius, 0, Math.PI * 2);

            if (node === this.hoveredNode) {
                this.ctx.fillStyle = '#00C8FF';
                this.ctx.strokeStyle = '#00C8FF';
            } else {
                this.ctx.fillStyle = '#00ADFF';
                this.ctx.strokeStyle = '#00ADFF';
            }

            if (node.animating) {
                this.ctx.lineWidth = 3;
            } else {
                this.ctx.lineWidth = 2;
            }

            this.ctx.fill();
            this.ctx.stroke();
        });

        // Draw line being drawn
        if (this.isDrawing && this.drawStartPos && this.drawCurrentPos) {
            this.ctx.beginPath();
            this.ctx.moveTo(this.drawStartPos.x, this.drawStartPos.y);
            this.ctx.lineTo(this.drawCurrentPos.x, this.drawCurrentPos.y);
            this.ctx.strokeStyle = '#888';
            this.ctx.lineWidth = 2;
            this.ctx.setLineDash([5, 5]);
            this.ctx.stroke();
            this.ctx.setLineDash([]);
        }
    }
}

// Initialize when document is ready
$(document).ready(function() {
    new LinksCanvas('canvas');
});
