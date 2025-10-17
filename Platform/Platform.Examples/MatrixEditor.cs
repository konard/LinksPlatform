using System;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// A multi-dimensional matrix editor that supports fixed-length lines,
    /// square selections, and storing symbols or links in cells.
    /// By default, the matrix is zeroed (0 means absence of symbol).
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the matrix (symbols or links)</typeparam>
    public class MatrixEditor<T> where T : struct
    {
        private readonly T[] _data;
        private readonly int[] _dimensions;
        private readonly int[] _multipliers;
        private readonly int _totalSize;

        /// <summary>
        /// Gets the dimensions of the matrix.
        /// </summary>
        public int[] Dimensions => (int[])_dimensions.Clone();

        /// <summary>
        /// Gets the total number of elements in the matrix.
        /// </summary>
        public int TotalSize => _totalSize;

        /// <summary>
        /// Initializes a new instance of the MatrixEditor class with specified dimensions.
        /// </summary>
        /// <param name="dimensions">The size of each dimension</param>
        public MatrixEditor(params int[] dimensions)
        {
            if (dimensions == null || dimensions.Length == 0)
            {
                throw new ArgumentException("At least one dimension must be specified.", nameof(dimensions));
            }

            if (dimensions.Any(d => d <= 0))
            {
                throw new ArgumentException("All dimensions must be positive.", nameof(dimensions));
            }

            _dimensions = (int[])dimensions.Clone();
            _multipliers = CalculateMultipliers(_dimensions);
            _totalSize = _dimensions.Aggregate(1, (acc, d) => acc * d);
            _data = new T[_totalSize];
        }

        /// <summary>
        /// Gets or sets the value at the specified coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates of the cell</param>
        /// <returns>The value at the specified coordinates</returns>
        public T this[params int[] coordinates]
        {
            get => Get(coordinates);
            set => Set(value, coordinates);
        }

        /// <summary>
        /// Gets the value at the specified coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates of the cell</param>
        /// <returns>The value at the specified coordinates</returns>
        public T Get(params int[] coordinates)
        {
            ValidateCoordinates(coordinates);
            int index = CalculateIndex(coordinates);
            return _data[index];
        }

        /// <summary>
        /// Sets the value at the specified coordinates.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="coordinates">The coordinates of the cell</param>
        public void Set(T value, params int[] coordinates)
        {
            ValidateCoordinates(coordinates);
            int index = CalculateIndex(coordinates);
            _data[index] = value;
        }

        /// <summary>
        /// Gets a square (rectangular) selection from the matrix.
        /// </summary>
        /// <param name="start">Starting coordinates of the selection</param>
        /// <param name="end">Ending coordinates of the selection (inclusive)</param>
        /// <returns>A new MatrixEditor containing the selected region</returns>
        public MatrixEditor<T> GetSquareSelection(int[] start, int[] end)
        {
            ValidateCoordinates(start);
            ValidateCoordinates(end);

            if (start.Length != end.Length)
            {
                throw new ArgumentException("Start and end coordinates must have the same dimension count.");
            }

            // Calculate dimensions of the selection
            int[] selectionDimensions = new int[start.Length];
            for (int i = 0; i < start.Length; i++)
            {
                if (start[i] > end[i])
                {
                    throw new ArgumentException($"Start coordinate at dimension {i} must be less than or equal to end coordinate.");
                }
                selectionDimensions[i] = end[i] - start[i] + 1;
            }

            var selection = new MatrixEditor<T>(selectionDimensions);

            // Copy the selected region
            CopyRegion(this, selection, start, new int[start.Length], selectionDimensions, 0);

            return selection;
        }

        /// <summary>
        /// Sets a square (rectangular) selection in the matrix.
        /// </summary>
        /// <param name="start">Starting coordinates where the selection will be placed</param>
        /// <param name="selection">The matrix to copy into this matrix</param>
        public void SetSquareSelection(int[] start, MatrixEditor<T> selection)
        {
            ValidateCoordinates(start);

            if (start.Length != selection._dimensions.Length)
            {
                throw new ArgumentException("Selection dimensions must match matrix dimensions.");
            }

            // Verify that the selection fits within the matrix
            for (int i = 0; i < start.Length; i++)
            {
                if (start[i] + selection._dimensions[i] > _dimensions[i])
                {
                    throw new ArgumentException($"Selection exceeds matrix bounds at dimension {i}.");
                }
            }

            // Copy the selection into this matrix
            CopyRegion(selection, this, new int[start.Length], start, selection._dimensions, 0);
        }

        /// <summary>
        /// Clears the entire matrix (sets all values to default).
        /// </summary>
        public void Clear()
        {
            Array.Clear(_data, 0, _data.Length);
        }

        /// <summary>
        /// Clears a square (rectangular) region of the matrix.
        /// </summary>
        /// <param name="start">Starting coordinates of the region</param>
        /// <param name="end">Ending coordinates of the region (inclusive)</param>
        public void ClearSquareRegion(int[] start, int[] end)
        {
            var selection = new MatrixEditor<T>(CalculateRegionDimensions(start, end));
            SetSquareSelection(start, selection);
        }

        private void ValidateCoordinates(int[] coordinates)
        {
            if (coordinates == null)
            {
                throw new ArgumentNullException(nameof(coordinates));
            }

            if (coordinates.Length != _dimensions.Length)
            {
                throw new ArgumentException($"Expected {_dimensions.Length} coordinates, but got {coordinates.Length}.", nameof(coordinates));
            }

            for (int i = 0; i < coordinates.Length; i++)
            {
                if (coordinates[i] < 0 || coordinates[i] >= _dimensions[i])
                {
                    throw new ArgumentOutOfRangeException(nameof(coordinates), $"Coordinate at dimension {i} is out of range. Expected [0, {_dimensions[i]}), but got {coordinates[i]}.");
                }
            }
        }

        private int CalculateIndex(int[] coordinates)
        {
            int index = 0;
            for (int i = 0; i < coordinates.Length; i++)
            {
                index += coordinates[i] * _multipliers[i];
            }
            return index;
        }

        private static int[] CalculateMultipliers(int[] dimensions)
        {
            int[] multipliers = new int[dimensions.Length];
            int multiplier = 1;
            for (int i = dimensions.Length - 1; i >= 0; i--)
            {
                multipliers[i] = multiplier;
                multiplier *= dimensions[i];
            }
            return multipliers;
        }

        private int[] CalculateRegionDimensions(int[] start, int[] end)
        {
            ValidateCoordinates(start);
            ValidateCoordinates(end);

            int[] dimensions = new int[start.Length];
            for (int i = 0; i < start.Length; i++)
            {
                if (start[i] > end[i])
                {
                    throw new ArgumentException($"Start coordinate at dimension {i} must be less than or equal to end coordinate.");
                }
                dimensions[i] = end[i] - start[i] + 1;
            }
            return dimensions;
        }

        private static void CopyRegion(MatrixEditor<T> source, MatrixEditor<T> destination, int[] sourceStart, int[] destStart, int[] regionDimensions, int dimension)
        {
            if (dimension == regionDimensions.Length - 1)
            {
                // Base case: copy a line
                for (int i = 0; i < regionDimensions[dimension]; i++)
                {
                    sourceStart[dimension] = i;
                    destStart[dimension] = i;
                    destination.Set(source.Get(sourceStart), destStart);
                }
            }
            else
            {
                // Recursive case: iterate through this dimension
                for (int i = 0; i < regionDimensions[dimension]; i++)
                {
                    sourceStart[dimension] = i;
                    destStart[dimension] = i;
                    CopyRegion(source, destination, sourceStart, destStart, regionDimensions, dimension + 1);
                }
            }
        }

        /// <summary>
        /// Returns a string representation of the matrix (for 1D and 2D matrices).
        /// </summary>
        public override string ToString()
        {
            if (_dimensions.Length == 1)
            {
                return $"[{string.Join(", ", _data)}]";
            }
            else if (_dimensions.Length == 2)
            {
                var lines = new string[_dimensions[0]];
                for (int i = 0; i < _dimensions[0]; i++)
                {
                    var row = new T[_dimensions[1]];
                    for (int j = 0; j < _dimensions[1]; j++)
                    {
                        row[j] = Get(i, j);
                    }
                    lines[i] = "[" + string.Join(", ", row) + "]";
                }
                return string.Join(Environment.NewLine, lines);
            }
            else
            {
                return $"MatrixEditor<{typeof(T).Name}> with dimensions [{string.Join(", ", _dimensions)}]";
            }
        }
    }
}
