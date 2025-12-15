using System;
using System.Linq;
using Xunit;

namespace EntityTrace.Tests
{
    public class TransformTests
    {
        #region Basic Single-Input Transforms

        [Fact]
        public void Transform_DoubleToInt_ShouldRoundCorrectly()
        {
            // Arrange
            var value = new Traceable<double>("Value", 5.7);

            // Act
            var rounded = value.Transform<double, int>("Round", x => (int)Math.Floor(x));

            // Assert
            Assert.Equal(5, rounded.Resolve());
            Assert.Equal("Round(Value)", rounded.Dependencies);

            var graph = rounded.BuildGraph();
            Assert.Equal("Round", graph.Operation);
            Assert.Equal(5, graph.Value);
            Assert.Single(graph.Children);
            Assert.Equal("Value", graph.Children[0].Name);
            Assert.Equal(5.7, graph.Children[0].Value);
        }

        [Fact]
        public void Transform_IntToString_ShouldConvertCorrectly()
        {
            // Arrange
            var number = new Traceable<int>("Number", 42);

            // Act
            var text = number.Transform<int, string>("ToString", x => x.ToString());

            // Assert
            Assert.Equal("42", text.Resolve());
            Assert.Equal("ToString(Number)", text.Dependencies);
        }

        [Fact]
        public void Transform_Dependencies_ShouldShowFunctionNotation()
        {
            // Arrange
            var value = new Traceable<double>("A", 3.14);

            // Act
            var transformed = value.Transform<double, int>("Floor", x => (int)Math.Floor(x));

            // Assert
            Assert.Equal("Floor(A)", transformed.Dependencies);
        }

        [Fact]
        public void Transform_BuildGraph_ShouldShowTransformNode()
        {
            // Arrange
            var value = new Traceable<double>("MyValue", 9.99);

            // Act
            var transformed = value.Transform<double, int>("Ceiling", x => (int)Math.Ceiling(x));

            // Assert
            var graph = transformed.BuildGraph();
            Assert.Equal("Ceiling", graph.Operation);
            Assert.Equal(10, graph.Value);
            Assert.Single(graph.Children);
            Assert.Equal("MyValue", graph.Children[0].Name);
            Assert.Equal(9.99, graph.Children[0].Value);
        }

        #endregion

        #region Multi-Input Transforms

        [Fact]
        public void Transform_TwoInputs_ShouldCombineValues()
        {
            // Arrange
            var a = new Traceable<int>("A", 3);
            var b = new Traceable<int>("B", 5);

            // Act
            var sum = TraceableExtensions.Transform(a, b, "Sum", (x, y) => x + y);

            // Assert
            Assert.Equal(8, sum.Resolve());
            Assert.Equal("Sum(A, B)", sum.Dependencies);
        }

        [Fact]
        public void Transform_ThreeInputs_ShouldWork()
        {
            // Arrange
            var a = new Traceable<int>("A", 2);
            var b = new Traceable<int>("B", 4);
            var c = new Traceable<int>("C", 6);

            // Act
            var avg = TraceableExtensions.Transform(a, b, c, "Average", (x, y, z) => (x + y + z) / 3);

            // Assert
            Assert.Equal(4, avg.Resolve());
            Assert.Equal("Average(A, B, C)", avg.Dependencies);
        }

        [Fact]
        public void Transform_MultiInput_Dependencies_ShouldShowAllOperands()
        {
            // Arrange
            var x = new Traceable<int>("X", 10);
            var y = new Traceable<int>("Y", 20);

            // Act
            var product = TraceableExtensions.Transform(x, y, "Multiply", (a, b) => a * b);

            // Assert
            Assert.Equal("Multiply(X, Y)", product.Dependencies);
        }

        [Fact]
        public void Transform_MultiInput_BuildGraph_ShouldShowAllChildren()
        {
            // Arrange
            var a = new Traceable<int>("A", 1);
            var b = new Traceable<int>("B", 2);
            var c = new Traceable<int>("C", 3);

            // Act
            var result = TraceableExtensions.Transform(a, b, c, "Combine", (x, y, z) => x + y + z);

            // Assert
            var graph = result.BuildGraph();
            Assert.Equal("Combine", graph.Operation);
            Assert.Equal(6, graph.Value);
            Assert.Equal(3, graph.Children.Count);
            Assert.Equal("A", graph.Children[0].Name);
            Assert.Equal(1, graph.Children[0].Value);
            Assert.Equal("B", graph.Children[1].Name);
            Assert.Equal(2, graph.Children[1].Value);
            Assert.Equal("C", graph.Children[2].Name);
            Assert.Equal(3, graph.Children[2].Value);
        }

        #endregion

        #region Complex Scenarios

        [Fact]
        public void Transform_Nested_ShouldChainCorrectly()
        {
            // Arrange
            var value = new Traceable<double>("Value", 10.7);

            // Act
            var floor = value.Transform<double, int>("Floor", x => (int)Math.Floor(x));
            var doubled = floor.Transform<int, int>("Double", x => x * 2);

            // Assert
            Assert.Equal(20, doubled.Resolve());
            Assert.Equal("Double(Floor(Value))", doubled.Dependencies);
        }

        [Fact]
        public void Transform_CombinedWithOperators_ShouldWork()
        {
            // Arrange
            var a = new Traceable<double>("A", 10.7);
            var b = new Traceable<double>("B", 5.3);

            // Act
            var sum = a + b;
            var rounded = sum.Transform<double, int>("Floor", x => (int)Math.Floor(x));

            // Assert
            Assert.Equal(16, rounded.Resolve());
            Assert.Equal("Floor(A + B)", rounded.Dependencies);

            var graph = rounded.BuildGraph();
            Assert.Equal("Floor", graph.Operation);
            Assert.Equal(16, graph.Value);
        }

        [Fact]
        public void Transform_GetDependencyNames_ShouldReturnAllBases()
        {
            // Arrange
            var a = new Traceable<int>("A", 1);
            var b = new Traceable<int>("B", 2);
            var sum = a + b;

            // Act
            var transformed = sum.Transform<int, int>("Double", x => x * 2);
            var dependencyNames = transformed.GetDependencyNames().ToList();

            // Assert
            Assert.Equal(2, dependencyNames.Count);
            Assert.Contains("A", dependencyNames);
            Assert.Contains("B", dependencyNames);
        }

        [Fact]
        public void Transform_WithDescription_ShouldDisplayInBuildGraph()
        {
            // Arrange
            var value = new Traceable<int>("Value", 100);

            // Act
            var transformed = value.Transform<int, string>("Format", x => $"${x}");
            transformed.Description = "Currency formatter";

            // Assert
            var graph = transformed.BuildGraph();
            Assert.Equal("Currency formatter", graph.Description);
        }

        #endregion

        #region Type Transformations

        [Fact]
        public void Transform_DoubleToInt_ShouldChangeType()
        {
            // Arrange
            var value = new Traceable<double>("Value", 7.5);

            // Act
            var intValue = value.Transform<double, int>("ToInt", x => (int)x);

            // Assert
            Assert.IsType<int>(intValue.Resolve());
            Assert.Equal(7, intValue.Resolve());
        }

        [Fact]
        public void Transform_NumericToString_ShouldWork()
        {
            // Arrange
            var number = new Traceable<decimal>("Price", 19.99m);

            // Act
            var formatted = number.Transform<decimal, string>("FormatPrice", x => $"${x:F2}");

            // Assert
            Assert.Equal("$19.99", formatted.Resolve());
        }

        [Fact]
        public void Transform_NumericToBool_ShouldWork()
        {
            // Arrange
            var temperature = new Traceable<int>("Temperature", 75);

            // Act
            var isHot = temperature.Transform<int, bool>("IsHot", x => x > 80);

            // Assert
            Assert.False(isHot.Resolve());
            Assert.Equal("IsHot(Temperature)", isHot.Dependencies);
        }

        [Fact]
        public void Transform_DifferentInputTypesForMultiInput_ShouldWork()
        {
            // Arrange
            var number = new Traceable<int>("Number", 42);
            var text = new Traceable<string>("Text", "Answer");

            // Act
            var combined = TraceableExtensions.Transform(text, number, "Combine", (t, n) => $"{t}: {n}");

            // Assert
            Assert.Equal("Answer: 42", combined.Resolve());
            Assert.Equal("Combine(Text, Number)", combined.Dependencies);
        }

        #endregion

        #region Error Handling

        [Fact]
        public void Transform_NullSource_ShouldThrowArgumentNullException()
        {
            // Arrange
            ITraceable<int> nullSource = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                nullSource.Transform<int, string>("ToString", x => x.ToString()));
        }

        [Fact]
        public void Transform_NullLabel_ShouldThrowArgumentException()
        {
            // Arrange
            var value = new Traceable<int>("Value", 5);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                value.Transform<int, int>(null, x => x * 2));
        }

        [Fact]
        public void Transform_EmptyLabel_ShouldThrowArgumentException()
        {
            // Arrange
            var value = new Traceable<int>("Value", 5);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                value.Transform<int, int>("", x => x * 2));
        }

        [Fact]
        public void Transform_WhitespaceLabel_ShouldThrowArgumentException()
        {
            // Arrange
            var value = new Traceable<int>("Value", 5);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                value.Transform<int, int>("   ", x => x * 2));
        }

        [Fact]
        public void Transform_NullTransformer_ShouldThrowArgumentNullException()
        {
            // Arrange
            var value = new Traceable<int>("Value", 5);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                value.Transform<int, int>("Double", null));
        }

        [Fact]
        public void Transform_ExceptionInTransformer_ShouldBubbleUp()
        {
            // Arrange
            var value = new Traceable<int>("Value", 0);
            var divided = value.Transform<int, int>("Reciprocal", x => 10 / x);

            // Act & Assert
            Assert.Throws<DivideByZeroException>(() => divided.Resolve());
        }

        [Fact]
        public void Transform_TwoInputs_NullFirstOperand_ShouldThrowArgumentNullException()
        {
            // Arrange
            var b = new Traceable<int>("B", 5);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                TraceableExtensions.Transform<int, int, int>(null, b, "Sum", (x, y) => x + y));
        }

        [Fact]
        public void Transform_ThreeInputs_NullThirdOperand_ShouldThrowArgumentNullException()
        {
            // Arrange
            var a = new Traceable<int>("A", 1);
            var b = new Traceable<int>("B", 2);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                TraceableExtensions.Transform<int, int, int, int>(a, b, null, "Avg", (x, y, z) => (x + y + z) / 3));
        }

        #endregion

        #region Reset Propagation

        [Fact]
        public void Transform_AfterReset_ShouldRecomputeValue()
        {
            // Arrange
            var value = new Traceable<int>("Value", 10);
            var doubled = value.Transform<int, int>("Double", x => x * 2);

            // Assert initial value
            Assert.Equal(20, doubled.Resolve());

            // Act - reset the base value
            value.Reset(15);

            // Assert - transformed value should update
            Assert.Equal(30, doubled.Resolve());
        }

        [Fact]
        public void Transform_InComplexChain_ResetShouldPropagate()
        {
            // Arrange
            var a = new Traceable<int>("A", 5);
            var b = new Traceable<int>("B", 3);
            var sum = a + b;
            var doubled = sum.Transform<int, int>("Double", x => x * 2);
            var final = doubled + new Traceable<int>("C", 10);

            // Assert initial value
            Assert.Equal(26, final.Resolve()); // (5 + 3) * 2 + 10 = 26

            // Act - reset base value
            a.Reset(10);

            // Assert - entire chain should update
            Assert.Equal(36, final.Resolve()); // (10 + 3) * 2 + 10 = 36
        }

        [Fact]
        public void Transform_MultiInput_AfterReset_ShouldRecompute()
        {
            // Arrange
            var a = new Traceable<int>("A", 2);
            var b = new Traceable<int>("B", 3);
            var product = TraceableExtensions.Transform(a, b, "Product", (x, y) => x * y);

            // Assert initial value
            Assert.Equal(6, product.Resolve());

            // Act - reset one operand
            a.Reset(5);

            // Assert - result should update
            Assert.Equal(15, product.Resolve());
        }

        #endregion

        #region Edge Cases and Additional Scenarios

        [Fact]
        public void Transform_ComplexNestedBuildGraph_ShouldDisplayCorrectly()
        {
            // Arrange
            var a = new Traceable<double>("A", 10.7);
            var b = new Traceable<double>("B", 5.3);
            var sum = a + b;
            var rounded = sum.Transform<double, int>("Floor", x => (int)Math.Floor(x));

            // Act
            var graph = rounded.BuildGraph();

            // Assert - Root is Floor transform
            Assert.Equal("Floor", graph.Operation);
            Assert.Equal(16, graph.Value);

            // Child is sum operation
            Assert.Single(graph.Children);
            var sumChild = graph.Children[0];
            Assert.Equal("+", sumChild.Operation);
            Assert.Equal(16.0, sumChild.Value);

            // Grandchildren are A and B
            Assert.Equal(2, sumChild.Children.Count);
            Assert.Equal("A", sumChild.Children[0].Name);
            Assert.Equal(10.7, sumChild.Children[0].Value);
            Assert.Equal("B", sumChild.Children[1].Name);
            Assert.Equal(5.3, sumChild.Children[1].Value);
        }

        [Fact]
        public void Transform_OperatorPrecedence_ShouldWork()
        {
            // Arrange
            var a = new Traceable<int>("A", 2);
            var b = new Traceable<int>("B", 3);
            var c = new Traceable<int>("C", 4);
            var product = b * c;
            var sum = a + product;

            // Act
            var transformed = sum.Transform<int, string>("Format", x => $"Result: {x}");

            // Assert
            Assert.Equal("Format(A + B * C)", transformed.Dependencies);
            Assert.Equal("Result: 14", transformed.Resolve());
        }

        [Fact]
        public void Transform_Name_ShouldMatchDependencies()
        {
            // Arrange
            var value = new Traceable<int>("Value", 42);

            // Act
            var transformed = value.Transform<int, string>("Stringify", x => x.ToString());

            // Assert
            Assert.Equal("Stringify(Value)", transformed.Name);
            Assert.Equal("Stringify(Value)", transformed.Dependencies);
        }

        [Fact]
        public void Transform_ValueAsObject_ShouldWork()
        {
            // Arrange
            var value = new Traceable<int>("Value", 42);

            // Act
            var transformed = value.Transform<int, string>("Format", x => $"Number: {x}");

            // Assert
            Assert.Equal("Number: 42", transformed.ValueAsObject);
        }

        #endregion
    }
}
