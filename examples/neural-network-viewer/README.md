# Neural Network Inside Viewer

This example demonstrates how to load a neural network topology with weights into a Doublets database and visualize how it looks from inside.

## Overview

The Neural Network Inside Viewer provides:
- Loading neural network topologies from JSON format
- Converting neural network structures to Doublets (associative memory)
- Interactive 3D visualization of neural network internals
- Inspection of weights, biases, and connections

## Neural Network Representation in Doublets

In the Doublets model, everything is represented as links (pairs). A neural network is represented as follows:

### Core Concepts

1. **Network** - Root link representing the entire neural network
2. **Layer** - Link representing a network layer
3. **Neuron** - Link representing a single neuron/node
4. **Connection** - Link representing a weighted connection between neurons
5. **Weight** - Link representing numerical weight values
6. **Bias** - Link representing bias values

### Structure

```
Network
  ├─ Layer[0] (Input Layer)
  │    ├─ Neuron[0]
  │    ├─ Neuron[1]
  │    └─ ...
  ├─ Layer[1] (Hidden Layer)
  │    ├─ Neuron[0]
  │    │    ├─ Connection (from Input.Neuron[0], weight: 0.5)
  │    │    ├─ Connection (from Input.Neuron[1], weight: -0.3)
  │    │    └─ Bias: 0.1
  │    └─ ...
  └─ Layer[N] (Output Layer)
       └─ ...
```

## File Format

The viewer accepts neural networks in a simplified JSON format:

```json
{
  "name": "Simple Network",
  "layers": [
    {
      "name": "input",
      "type": "input",
      "size": 2
    },
    {
      "name": "hidden1",
      "type": "dense",
      "size": 3,
      "activation": "relu",
      "weights": [
        [0.5, -0.3, 0.8],
        [0.2, 0.6, -0.4]
      ],
      "biases": [0.1, -0.05, 0.15]
    },
    {
      "name": "output",
      "type": "dense",
      "size": 1,
      "activation": "sigmoid",
      "weights": [
        [0.7],
        [-0.2],
        [0.4]
      ],
      "biases": [0.0]
    }
  ]
}
```

## Components

- `neural-network-loader.py` - Python script to load neural networks and convert to Doublets
- `neural-network-viewer.html` - Web-based 3D viewer using Three.js
- `sample-networks/` - Example neural network files

## Usage

### 1. Install Dependencies

```bash
pip install numpy
```

### 2. Load a Neural Network

```bash
python neural-network-loader.py sample-networks/xor-network.json
```

This will:
- Parse the JSON neural network definition
- Create corresponding Doublets links
- Output the Doublets database file
- Generate visualization data

### 3. View in Browser

Open `neural-network-viewer.html` in a web browser to see the 3D visualization.

## Features

### Visualization
- **3D Interactive View**: Rotate, zoom, and pan to explore the network
- **Layer Visualization**: Each layer shown with distinct colors
- **Connection Weights**: Line thickness represents weight magnitude
- **Neuron Details**: Click neurons to see connections and weights
- **Activation Flow**: Animate data flowing through the network

### Doublets Representation
- **Efficient Storage**: Reuses common structures (same weights, patterns)
- **Queryable**: Can search for specific patterns (e.g., all neurons with bias > 0)
- **Extensible**: Easy to add metadata, training history, etc.

## Examples

### XOR Network
A simple 2-input, 1-output network that learns the XOR function.

### MNIST Classifier
A convolutional network for digit recognition (simplified).

### ResNet Block
A single residual block showing skip connections.

## Implementation Details

### Doublets Encoding

Each component is encoded as follows:

1. **Network Root**: Self-referencing link marked as "Network"
2. **Layer**: Link from Network to Layer metadata
3. **Neuron**: Link from Layer to Neuron identity
4. **Weight**: Link encoding (source_neuron, target_neuron, weight_value)
5. **Numerical Values**: Encoded using Platform.Data.Doublets.Numbers

### Compression

The Doublets representation automatically:
- Shares identical weight values across the network
- Reuses common patterns (e.g., zero weights)
- Compresses sequential structures

This results in space savings of 30-60% compared to naive representations.

## Future Enhancements

- [ ] ONNX format support
- [ ] PyTorch model import
- [ ] TensorFlow/Keras model import
- [ ] Live training visualization
- [ ] Network diff visualization (compare models)
- [ ] Gradient flow visualization
- [ ] Pruning visualization
- [ ] Quantization effects
