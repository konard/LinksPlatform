using System;
using System.Linq;
using Xunit;

namespace LinksPlatform.Experiments.Tests
{
    /// <summary>
    /// Unit tests for InfiniteDimensionalModel
    /// </summary>
    public class InfiniteDimensionalModelTests
    {
        [Fact]
        public void Constructor_SetsDefaultValue()
        {
            // Arrange & Act
            var model = new InfiniteDimensionalModel<int>(0);

            // Assert
            Assert.Equal(0, model.DefaultValue);
            Assert.Equal(0, model.OverrideCount);
        }

        [Fact]
        public void GetValue_ReturnsDefaultValue_WhenNoDimensionOverride()
        {
            // Arrange
            var model = new InfiniteDimensionalModel<int>(0);

            // Act & Assert
            Assert.Equal(0, model.GetValue(0));
            Assert.Equal(0, model.GetValue(1));
            Assert.Equal(0, model.GetValue(100));
            Assert.Equal(0, model.GetValue(-1));
            Assert.Equal(0, model.GetValue(long.MaxValue));
        }

        [Fact]
        public void SetValue_OverridesSpecificDimension()
        {
            // Arrange
            var model = new InfiniteDimensionalModel<int>(0);

            // Act
            model.SetValue(5, 42);

            // Assert
            Assert.Equal(42, model.GetValue(5));
            Assert.Equal(0, model.GetValue(4));
            Assert.Equal(0, model.GetValue(6));
            Assert.Equal(1, model.OverrideCount);
        }

        [Fact]
        public void SetValue_ToDefaultValue_RemovesOverride()
        {
            // Arrange
            var model = new InfiniteDimensionalModel<int>(0);
            model.SetValue(5, 42);

            // Act
            model.SetValue(5, 0); // Setting back to default

            // Assert
            Assert.Equal(0, model.GetValue(5));
            Assert.Equal(0, model.OverrideCount);
        }

        [Fact]
        public void GetOverriddenDimensions_ReturnsCorrectIndices()
        {
            // Arrange
            var model = new InfiniteDimensionalModel<int>(0);
            model.SetValue(1, 10);
            model.SetValue(5, 50);
            model.SetValue(100, 1000);

            // Act
            var overrides = model.GetOverriddenDimensions().OrderBy(x => x).ToList();

            // Assert
            Assert.Equal(3, overrides.Count);
            Assert.Contains(1L, overrides);
            Assert.Contains(5L, overrides);
            Assert.Contains(100L, overrides);
        }

        [Fact]
        public void ClearOverrides_RemovesAllOverrides()
        {
            // Arrange
            var model = new InfiniteDimensionalModel<int>(0);
            model.SetValue(1, 10);
            model.SetValue(5, 50);

            // Act
            model.ClearOverrides();

            // Assert
            Assert.Equal(0, model.OverrideCount);
            Assert.Equal(0, model.GetValue(1));
            Assert.Equal(0, model.GetValue(5));
        }

        [Fact]
        public void ForEachOverride_IteratesAllOverrides()
        {
            // Arrange
            var model = new InfiniteDimensionalModel<int>(0);
            model.SetValue(1, 10);
            model.SetValue(2, 20);
            model.SetValue(3, 30);

            var sum = 0;
            var count = 0;

            // Act
            model.ForEachOverride((dim, value) =>
            {
                sum += value;
                count++;
            });

            // Assert
            Assert.Equal(3, count);
            Assert.Equal(60, sum);
        }

        [Fact]
        public void Clone_CreatesIndependentCopy()
        {
            // Arrange
            var original = new InfiniteDimensionalModel<int>(0);
            original.SetValue(1, 10);
            original.SetValue(2, 20);

            // Act
            var clone = original.Clone();
            clone.SetValue(3, 30);

            // Assert
            Assert.Equal(10, clone.GetValue(1));
            Assert.Equal(20, clone.GetValue(2));
            Assert.Equal(30, clone.GetValue(3));
            Assert.Equal(2, original.OverrideCount);
            Assert.Equal(3, clone.OverrideCount);
        }

        [Fact]
        public void TransformOverrides_AppliesTransformation()
        {
            // Arrange
            var model = new InfiniteDimensionalModel<int>(0);
            model.SetValue(1, 10);
            model.SetValue(2, 20);

            // Act
            model.TransformOverrides(x => x * 2);

            // Assert
            Assert.Equal(20, model.GetValue(1));
            Assert.Equal(40, model.GetValue(2));
            Assert.Equal(0, model.GetValue(3)); // Default unchanged
        }

        [Fact]
        public void OperateWith_PerformsElementWiseOperation()
        {
            // Arrange
            var model1 = new InfiniteDimensionalModel<int>(0);
            model1.SetValue(1, 10);
            model1.SetValue(2, 20);

            var model2 = new InfiniteDimensionalModel<int>(0);
            model2.SetValue(2, 5);
            model2.SetValue(3, 15);

            // Act
            var result = model1.OperateWith(model2, (a, b) => a + b, 0);

            // Assert
            Assert.Equal(10, result.GetValue(1)); // 10 + 0
            Assert.Equal(25, result.GetValue(2)); // 20 + 5
            Assert.Equal(15, result.GetValue(3)); // 0 + 15
            Assert.Equal(0, result.GetValue(4));  // 0 + 0 (default)
        }

        [Fact]
        public void ToString_ShowsDefaultValueAndOverrideCount()
        {
            // Arrange
            var model1 = new InfiniteDimensionalModel<int>(0);
            var model2 = new InfiniteDimensionalModel<int>(1);
            model2.SetValue(5, 99);

            // Act & Assert
            Assert.Contains("0", model1.ToString());
            Assert.Contains("1", model2.ToString());
            Assert.Contains("Overrides: 1", model2.ToString());
        }

        [Fact]
        public void Model_HandlesStringType()
        {
            // Arrange
            var model = new InfiniteDimensionalModel<string>("default");

            // Act
            model.SetValue(0, "custom");

            // Assert
            Assert.Equal("custom", model.GetValue(0));
            Assert.Equal("default", model.GetValue(1));
        }

        [Fact]
        public void Model_HandlesNegativeDimensions()
        {
            // Arrange
            var model = new InfiniteDimensionalModel<int>(0);

            // Act
            model.SetValue(-5, 100);
            model.SetValue(-1, 200);

            // Assert
            Assert.Equal(100, model.GetValue(-5));
            Assert.Equal(200, model.GetValue(-1));
            Assert.Equal(0, model.GetValue(0));
        }

        [Fact]
        public void ChangeDefaultValue_AffectsAllNonOverriddenDimensions()
        {
            // Arrange
            var model = new InfiniteDimensionalModel<int>(0);
            model.SetValue(5, 50);

            // Act
            model.DefaultValue = 1;

            // Assert
            Assert.Equal(50, model.GetValue(5)); // Override preserved
            Assert.Equal(1, model.GetValue(0));  // Default changed
            Assert.Equal(1, model.GetValue(1));  // Default changed
            Assert.Equal(1, model.GetValue(100)); // Default changed
        }

        [Fact]
        public void ZeroModel_AsDescribedInIssue()
        {
            // Zero can be defined like so: 0, 0, 0, ...
            var zero = new InfiniteDimensionalModel<int>(0);

            // Assert all dimensions return 0
            Assert.Equal(0, zero.GetValue(0));
            Assert.Equal(0, zero.GetValue(1));
            Assert.Equal(0, zero.GetValue(1000));
            Assert.Equal(0, zero.OverrideCount);
        }

        [Fact]
        public void OneModel_AsDescribedInIssue()
        {
            // One can be defined like so: 1, 1, 1, ...
            var one = new InfiniteDimensionalModel<int>(1);

            // Assert all dimensions return 1
            Assert.Equal(1, one.GetValue(0));
            Assert.Equal(1, one.GetValue(1));
            Assert.Equal(1, one.GetValue(1000));
            Assert.Equal(0, one.OverrideCount);
        }

        [Fact]
        public void CalculationsNeededOnlyOnChanges_AsDescribedInIssue()
        {
            // We do not need to store all infinite dimensions
            // We just define values on them
            var model = new InfiniteDimensionalModel<int>(0);

            // Calculations needed only on changes to that values
            model.SetValue(42, 100);

            // Only one dimension is actually stored
            Assert.Equal(1, model.OverrideCount);

            // But infinitely many dimensions are conceptually defined
            Assert.Equal(0, model.GetValue(0));
            Assert.Equal(0, model.GetValue(41));
            Assert.Equal(100, model.GetValue(42)); // The override
            Assert.Equal(0, model.GetValue(43));
            Assert.Equal(0, model.GetValue(long.MaxValue));
        }
    }
}
