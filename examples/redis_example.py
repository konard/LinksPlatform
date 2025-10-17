#!/usr/bin/env python3
"""
Example: Using Redis for Data Storage
This demonstrates how to use Redis for common data storage patterns
comparable to the Doublets example
"""

import redis
import time
from typing import List

def main():
    # Connect to Redis (make sure Redis server is running)
    try:
        r = redis.Redis(host='localhost', port=6379, db=0, decode_responses=True)
        r.ping()
        print("Connected to Redis successfully\n")
    except redis.ConnectionError:
        print("Error: Cannot connect to Redis. Make sure Redis server is running.")
        print("Start Redis with: redis-server")
        return

    # Clear any existing data for clean example
    r.flushdb()

    print("=== Redis Example: Creating a Knowledge Graph ===\n")

    # Example 1: Create basic entities (using hashes)
    print("1. Creating basic entities:")
    r.hset("entity:person", mapping={"type": "concept", "name": "Person"})
    r.hset("entity:company", mapping={"type": "concept", "name": "Company"})
    r.hset("entity:city", mapping={"type": "concept", "name": "City"})

    r.hset("entity:alice", mapping={"type": "person", "name": "Alice"})
    r.hset("entity:bob", mapping={"type": "person", "name": "Bob"})
    r.hset("entity:techcorp", mapping={"type": "company", "name": "TechCorp"})
    r.hset("entity:sf", mapping={"type": "city", "name": "San Francisco"})

    print("  Created entities: Person, Company, City, Alice, Bob, TechCorp, San Francisco")

    # Example 2: Create relationships (using hashes and sets)
    print("\n2. Creating relationships:")

    # Alice works at TechCorp
    r.hset("entity:alice", "works_at", "techcorp")
    r.sadd("relationship:works_at:techcorp", "alice")
    print("  Created: Alice works_at TechCorp")

    # Bob works at TechCorp
    r.hset("entity:bob", "works_at", "techcorp")
    r.sadd("relationship:works_at:techcorp", "bob")
    print("  Created: Bob works_at TechCorp")

    # Alice lives in San Francisco
    r.hset("entity:alice", "lives_in", "sf")
    r.sadd("relationship:lives_in:sf", "alice")
    print("  Created: Alice lives_in San Francisco")

    # TechCorp located in San Francisco
    r.hset("entity:techcorp", "located_in", "sf")
    r.sadd("relationship:located_in:sf", "techcorp")
    print("  Created: TechCorp located_in San Francisco")

    # Example 3: Query - Find all entities
    print("\n3. Querying all entities:")
    entity_keys = r.keys("entity:*")
    for key in entity_keys:
        entity_data = r.hgetall(key)
        print(f"  {key}: {entity_data}")
    print(f"Total entities: {len(entity_keys)}")

    # Example 4: Query - Find who works at TechCorp
    print("\n4. Query: Who works at TechCorp?")
    employees = r.smembers("relationship:works_at:techcorp")
    for emp in employees:
        emp_data = r.hgetall(f"entity:{emp}")
        print(f"  - {emp_data.get('name', emp)}")

    # Example 5: Create a sequence (using list)
    print("\n5. Creating a sequence: [1, 2, 3, 4, 5]:")
    r.rpush("sequence:numbers", "1", "2", "3", "4", "5")
    sequence = r.lrange("sequence:numbers", 0, -1)
    print(f"  Sequence: {sequence}")

    # Example 6: Performance test - simple key-value operations
    print("\n6. Performance test: Creating 10,000 simple keys:")
    start = time.time()
    pipeline = r.pipeline()
    for i in range(10000):
        pipeline.set(f"test:point:{i}", f"point_{i}")
    pipeline.execute()
    elapsed_ms = (time.time() - start) * 1000
    print(f"  Created 10,000 keys in {elapsed_ms:.2f}ms")
    print(f"  Average: {elapsed_ms / 10:.2f}µs per key")

    # Example 7: Performance test - hash operations
    print("\n7. Performance test: Creating 10,000 relationships:")
    entities = [f"entity:test{i}" for i in range(100)]

    start = time.time()
    pipeline = r.pipeline()
    for i in range(10000):
        source = entities[i % 100]
        target = entities[(i + 1) % 100]
        pipeline.hset(source, f"link_{i}", target)
    pipeline.execute()
    elapsed_ms = (time.time() - start) * 1000
    print(f"  Created 10,000 relationships in {elapsed_ms:.2f}ms")
    print(f"  Average: {elapsed_ms / 10:.2f}µs per relationship")

    # Example 8: Using Redis as a graph (alternative approach with sorted sets)
    print("\n8. Graph representation using sorted sets:")

    # Add edges with weights (timestamp or priority)
    r.zadd("graph:alice:outgoing", {"bob": 1, "techcorp": 2, "sf": 3})
    r.zadd("graph:bob:outgoing", {"techcorp": 1, "alice": 2})
    r.zadd("graph:techcorp:incoming", {"alice": 1, "bob": 2})

    # Query: Get all outgoing connections from Alice
    alice_connections = r.zrange("graph:alice:outgoing", 0, -1)
    print(f"  Alice's connections: {alice_connections}")

    # Example 9: Memory usage
    print("\n9. Memory usage:")
    info = r.info("memory")
    used_memory = info['used_memory_human']
    print(f"  Used memory: {used_memory}")

    # Example 10: Key statistics
    print("\n10. Database statistics:")
    dbsize = r.dbsize()
    print(f"  Total keys: {dbsize}")

    print("\n=== Example Complete ===")

    # Cleanup option
    print("\nTo clean up test data, run: redis-cli FLUSHDB")

if __name__ == "__main__":
    main()
