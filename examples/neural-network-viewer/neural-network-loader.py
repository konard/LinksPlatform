#!/usr/bin/env python3
"""
Neural Network Loader for Doublets Database
Loads neural network topology and weights, converts to Doublets representation
"""

import json
import sys
import os
from typing import Dict, List, Any


class DoubletsSimulator:
    """Simulates a Doublets database using Python dictionaries"""

    def __init__(self):
        self.links = {}  # link_id -> (source, target)
        self.metadata = {}  # link_id -> metadata
        self.next_id = 1

    def create_link(self, source=None, target=None, metadata=None):
        """Create a new link (pair)"""
        if source is None:
            source = self.next_id
        if target is None:
            target = source

        link_id = self.next_id
        self.links[link_id] = (source, target)
        if metadata:
            self.metadata[link_id] = metadata
        self.next_id += 1
        return link_id

    def get_link(self, link_id):
        """Get a link by ID"""
        return self.links.get(link_id)

    def to_json(self):
        """Export to JSON for visualization"""
        return {
            'links': [
                {
                    'id': lid,
                    'source': src,
                    'target': tgt,
                    'metadata': self.metadata.get(lid, {})
                }
                for lid, (src, tgt) in self.links.items()
            ]
        }


class NeuralNetworkToDoublets:
    """Converts neural network structures to Doublets representation"""

    def __init__(self):
        self.db = DoubletsSimulator()
        self.network_root = None
        self.layer_links = []
        self.neuron_links = {}
        self.connection_links = []

        # Create semantic markers
        self.network_type = self.db.create_link(metadata={'type': 'NetworkType'})
        self.layer_type = self.db.create_link(metadata={'type': 'LayerType'})
        self.neuron_type = self.db.create_link(metadata={'type': 'NeuronType'})
        self.connection_type = self.db.create_link(metadata={'type': 'ConnectionType'})
        self.weight_type = self.db.create_link(metadata={'type': 'WeightType'})
        self.bias_type = self.db.create_link(metadata={'type': 'BiasType'})

    def load_network(self, network_data: Dict) -> int:
        """Load a neural network from JSON structure"""
        # Create network root
        self.network_root = self.db.create_link(
            metadata={
                'type': 'Network',
                'name': network_data.get('name', 'Unnamed Network')
            }
        )

        layers = network_data.get('layers', [])

        # Process each layer
        prev_layer_neurons = []
        for layer_idx, layer_data in enumerate(layers):
            neurons = self._create_layer(layer_idx, layer_data, prev_layer_neurons)
            prev_layer_neurons = neurons

        return self.network_root

    def _create_layer(self, layer_idx: int, layer_data: Dict, prev_neurons: List) -> List[int]:
        """Create a layer and its neurons"""
        layer_link = self.db.create_link(
            source=self.network_root,
            target=self.layer_type,
            metadata={
                'type': 'Layer',
                'index': layer_idx,
                'name': layer_data.get('name', f'layer_{layer_idx}'),
                'layer_type': layer_data.get('type', 'dense'),
                'size': layer_data.get('size'),
                'activation': layer_data.get('activation')
            }
        )
        self.layer_links.append(layer_link)

        # Create neurons for this layer
        neurons = []
        size = layer_data.get('size', 0)
        for neuron_idx in range(size):
            neuron_link = self.db.create_link(
                source=layer_link,
                target=self.neuron_type,
                metadata={
                    'type': 'Neuron',
                    'layer_index': layer_idx,
                    'neuron_index': neuron_idx
                }
            )
            neurons.append(neuron_link)
            self.neuron_links[(layer_idx, neuron_idx)] = neuron_link

        # Create connections if not input layer
        if layer_idx > 0 and layer_data.get('type') != 'input':
            self._create_connections(
                layer_idx,
                neurons,
                prev_neurons,
                layer_data.get('weights', []),
                layer_data.get('biases', [])
            )

        return neurons

    def _create_connections(self, layer_idx: int, target_neurons: List[int],
                           source_neurons: List[int], weights: List[List[float]],
                           biases: List[float]):
        """Create weighted connections between neurons"""
        for target_idx, target_neuron in enumerate(target_neurons):
            # Create bias if provided
            if biases and target_idx < len(biases):
                bias_value = biases[target_idx]
                bias_link = self.db.create_link(
                    source=target_neuron,
                    target=self.bias_type,
                    metadata={
                        'type': 'Bias',
                        'value': bias_value
                    }
                )

            # Create weighted connections from previous layer
            if weights and target_idx < len(source_neurons):
                for source_idx, source_neuron in enumerate(source_neurons):
                    if source_idx < len(weights) and target_idx < len(weights[source_idx]):
                        weight_value = weights[source_idx][target_idx]

                        # Create connection with weight
                        connection_link = self.db.create_link(
                            source=source_neuron,
                            target=target_neuron,
                            metadata={
                                'type': 'Connection',
                                'weight': weight_value,
                                'from_layer': layer_idx - 1,
                                'to_layer': layer_idx
                            }
                        )
                        self.connection_links.append(connection_link)

    def export_for_visualization(self) -> Dict:
        """Export network in format suitable for web visualization"""
        result = {
            'doublets': self.db.to_json(),
            'network': {
                'root': self.network_root,
                'layers': []
            }
        }

        # Organize by layers for easier visualization
        layers_data = {}
        for link_id, metadata in self.db.metadata.items():
            if metadata.get('type') == 'Layer':
                layer_idx = metadata['index']
                layers_data[layer_idx] = {
                    'link_id': link_id,
                    'name': metadata['name'],
                    'neurons': []
                }

        for (layer_idx, neuron_idx), neuron_link in self.neuron_links.items():
            if layer_idx in layers_data:
                layers_data[layer_idx]['neurons'].append({
                    'link_id': neuron_link,
                    'index': neuron_idx
                })

        result['network']['layers'] = [layers_data[i] for i in sorted(layers_data.keys())]

        return result


def main():
    if len(sys.argv) < 2:
        print("Usage: python neural-network-loader.py <network.json>")
        print("\nExample:")
        print("  python neural-network-loader.py sample-networks/xor-network.json")
        sys.exit(1)

    input_file = sys.argv[1]

    if not os.path.exists(input_file):
        print(f"Error: File not found: {input_file}")
        sys.exit(1)

    # Load network JSON
    print(f"Loading neural network from: {input_file}")
    with open(input_file, 'r') as f:
        network_data = json.load(f)

    # Convert to Doublets
    print("Converting to Doublets representation...")
    converter = NeuralNetworkToDoublets()
    network_root = converter.load_network(network_data)

    print(f"Network root link ID: {network_root}")
    print(f"Total links created: {len(converter.db.links)}")
    print(f"  - Layers: {len(converter.layer_links)}")
    print(f"  - Neurons: {len(converter.neuron_links)}")
    print(f"  - Connections: {len(converter.connection_links)}")

    # Export for visualization
    output_file = input_file.replace('.json', '-doublets.json')
    visualization_data = converter.export_for_visualization()

    with open(output_file, 'w') as f:
        json.dump(visualization_data, f, indent=2)

    print(f"\nVisualization data exported to: {output_file}")
    print("Open neural-network-viewer.html to visualize the network")

    # Print some statistics
    print("\nNetwork Statistics:")
    print(f"  Name: {network_data.get('name', 'Unnamed')}")
    print(f"  Layers: {len(network_data.get('layers', []))}")

    for idx, layer in enumerate(network_data.get('layers', [])):
        print(f"    Layer {idx} ({layer.get('name')}): {layer.get('size')} neurons")


if __name__ == '__main__':
    main()
