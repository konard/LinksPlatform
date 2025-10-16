# Makefile for Minimal Links Platform Example
#
# This can be adapted for BareMetal OS by changing the compiler
# and removing standard library dependencies

CC = gcc
CFLAGS = -Wall -Wextra -std=c99 -O2
TARGET = links_example
SOURCES = links.c example.c
OBJECTS = $(SOURCES:.c=.o)

# Default target
all: $(TARGET)

# Link the executable
$(TARGET): $(OBJECTS)
	$(CC) $(CFLAGS) -o $@ $^

# Compile source files
%.o: %.c links.h
	$(CC) $(CFLAGS) -c $<

# Clean build artifacts
clean:
	rm -f $(OBJECTS) $(TARGET)

# Run the example
run: $(TARGET)
	./$(TARGET)

# For BareMetal OS compilation (commented out - requires BareMetal SDK)
# baremetal: links.c example.c links.h
#	# This would use BareMetal's compiler and linker
#	# Adjust paths and flags according to BareMetal SDK documentation
#	# Example: bmcc -o links_example.app links.c example.c

.PHONY: all clean run baremetal
