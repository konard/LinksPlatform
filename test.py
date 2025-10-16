#!/usr/bin/env python
# -*- coding: utf-8 -*-

"""
Triple Links Example using Python

This example demonstrates how to work with triple links (triplets) in the Links Platform.
A triple link represents a relationship with three components: Source-Verb-Target or Index-Source-Target.

The example shows:
1. Creating a link that points to itself (self-reference)
2. Creating links that reference other links
3. Creating a triple link (a link with source, verb/relation, and target)
4. Updating and deleting links
"""

import ctypes

# Load the Links Platform native library
# Note: This requires Platform.Data.Kernel.dll to be available
# You can get it from: https://github.com/linksplatform/Data.Triplets.Kernel
links = ctypes.CDLL('./Platform.Data.Kernel.dll')

# Initialize persistent memory manager
links.InitPersistentMemoryManager()

# Open storage file for links database
links.OpenStorageFile("db.links")
links.SetStorageFileMemoryMapping()

# Create initial self-referencing link (itself points to itself)
itself = 0

# Create links demonstrating triple structure
# isA represents "is a" relationship type
isA = links.CreateLink(itself, itself, itself)

# isNotA represents "is not a" relationship type
isNotA = links.CreateLink(itself, isA, itself)

# link represents a general link concept
link = links.CreateLink(itself, isA, itself)

# thing represents a general thing/object concept
# This creates a triple: link "isNotA" thing
thing = links.CreateLink(itself, isNotA, link)

# Print the created links
print(isA, isNotA, link, thing)

# Update a link - this demonstrates link modification
# After this, minimal core system can consider it properly formed
links.UpdateLink(isA, isA, isA, link)  # Update isA to point to: isA -> isA -> link

# Delete links to clean up
links.DeleteLink(isA)  # This operation will delete all 4 links since they reference each other

# Alternative: Delete individual link
links.DeleteLink(thing)

# Clean up: reset and close storage
links.ResetStorageFileMemoryMapping()
links.CloseStorageFile()
