CC = gcc
CFLAGS = -Wall -Wextra -std=c99 -pedantic -O2
LDFLAGS =

# Source files
SRC = pair_link.c
OBJ = $(SRC:.c=.o)

# Example programs
EXAMPLES = examples/basic_usage

.PHONY: all clean examples

all: examples

# Build pair_link library object
pair_link.o: pair_link.c pair_link.h
	$(CC) $(CFLAGS) -c $< -o $@

# Build examples
examples: $(EXAMPLES)

examples/basic_usage: examples/basic_usage.c pair_link.o
	$(CC) $(CFLAGS) $^ -o $@ $(LDFLAGS)

# Run examples
run: examples
	@echo "Running basic_usage example:"
	@./examples/basic_usage

# Clean build artifacts
clean:
	rm -f $(OBJ)
	rm -f $(EXAMPLES)
	rm -f examples/*.o

# Help
help:
	@echo "Available targets:"
	@echo "  all       - Build all examples (default)"
	@echo "  examples  - Build example programs"
	@echo "  run       - Build and run examples"
	@echo "  clean     - Remove build artifacts"
	@echo "  help      - Show this help message"
