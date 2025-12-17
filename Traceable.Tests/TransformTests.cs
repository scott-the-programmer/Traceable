namespace Traceable.Tests;

public class TransformTests
{
    #region Basic Single-Input Transforms

    [Fact]
    public void Transform_DoubleToInt_ShouldRoundCorrectly()
    {
        // Arrange
        var value = new Traceable<double>(5.7, "Value");

        // Act
        var rounded = value.Transform<double, int>(x => (int)Math.Floor(x), "Round");

        // Assert
        Assert.Multiple(
            () => Assert.Equal(5, rounded.Resolve()),
            () => Assert.Equal("Round(Value)", rounded.Dependencies)
        );

        var graph = rounded.BuildGraph();
        Assert.Multiple(
            () => Assert.Equal("Round", graph.Operation),
            () => Assert.Equal(5, graph.Value),
            () => Assert.Single(graph.Children),
            () => Assert.Equal("Value", graph.Children[0].Name),
            () => Assert.Equal(5.7, graph.Children[0].Value)
        );
    }

    [Fact]
    public void Transform_IntToString_ShouldConvertCorrectly()
    {
        // Arrange
        var number = new Traceable<int>(42, "Number");

        // Act
        var text = number.Transform<int, string>(x => x.ToString(), "ToString");

        // Assert
        Assert.Equal("42", text.Resolve());
        Assert.Equal("ToString(Number)", text.Dependencies);
    }

    [Fact]
    public void Transform_Dependencies_ShouldShowFunctionNotation()
    {
        // Arrange
        var value = new Traceable<double>(3.14, "A");

        // Act
        var transformed = value.Transform<double, int>(x => (int)Math.Floor(x), "Floor");

        // Assert
        Assert.Equal("Floor(A)", transformed.Dependencies);
    }

    [Fact]
    public void Transform_BuildGraph_ShouldShowTransformNode()
    {
        // Arrange
        var value = new Traceable<double>(9.99, "MyValue");

        // Act
        var transformed = value.Transform<double, int>(x => (int)Math.Ceiling(x), "Ceiling");

        // Assert
        var graph = transformed.BuildGraph();
        Assert.Multiple(
            () => Assert.Equal("Ceiling", graph.Operation),
            () => Assert.Equal(10, graph.Value),
            () => Assert.Single(graph.Children),
            () => Assert.Equal("MyValue", graph.Children[0].Name),
            () => Assert.Equal(9.99, graph.Children[0].Value)
        );
    }

    #endregion

    #region Multi-Input Transforms

    [Fact]
    public void Transform_TwoInputs_ShouldCombineValues()
    {
        // Arrange
        var a = new Traceable<int>(3, "A");
        var b = new Traceable<int>(5, "B");

        // Act
        var sum = TraceableExtensions.Transform(a, b, (x, y) => x + y, "Sum");

        // Assert
        Assert.Equal(8, sum.Resolve());
        Assert.Equal("Sum(A, B)", sum.Dependencies);
    }

    [Fact]
    public void Transform_ThreeInputs_ShouldWork()
    {
        // Arrange
        var a = new Traceable<int>(2, "A");
        var b = new Traceable<int>(4, "B");
        var c = new Traceable<int>(6, "C");

        // Act
        var avg = TraceableExtensions.Transform(a, b, c, (x, y, z) => (x + y + z) / 3, "Average");

        // Assert
        Assert.Equal(4, avg.Resolve());
        Assert.Equal("Average(A, B, C)", avg.Dependencies);
    }

    [Fact]
    public void Transform_MultiInput_Dependencies_ShouldShowAllOperands()
    {
        // Arrange
        var x = new Traceable<int>(10, "X");
        var y = new Traceable<int>(20, "Y");

        // Act
        var product = TraceableExtensions.Transform(x, y, (a, b) => a * b, "Multiply");

        // Assert
        Assert.Equal("Multiply(X, Y)", product.Dependencies);
    }

    [Fact]
    public void Transform_MultiInput_BuildGraph_ShouldShowAllChildren()
    {
        // Arrange
        var a = new Traceable<int>(1, "A");
        var b = new Traceable<int>(2, "B");
        var c = new Traceable<int>(3, "C");

        // Act
        var result = TraceableExtensions.Transform(a, b, c, (x, y, z) => x + y + z, "Combine");

        // Assert
        var graph = result.BuildGraph();
        Assert.Multiple(
            () => Assert.Equal("Combine", graph.Operation),
            () => Assert.Equal(6, graph.Value),
            () => Assert.Equal(3, graph.Children.Count),
            () => Assert.Equal("A", graph.Children[0].Name),
            () => Assert.Equal(1, graph.Children[0].Value),
            () => Assert.Equal("B", graph.Children[1].Name),
            () => Assert.Equal(2, graph.Children[1].Value),
            () => Assert.Equal("C", graph.Children[2].Name),
            () => Assert.Equal(3, graph.Children[2].Value)
        );
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Transform_Nested_ShouldChainCorrectly()
    {
        // Arrange
        var value = new Traceable<double>(10.7, "Value");

        // Act
        var floor = value.Transform<double, int>(x => (int)Math.Floor(x), "Floor");
        var doubled = floor.Transform<int, int>(x => x * 2, "Double");

        // Assert
        Assert.Equal(20, doubled.Resolve());
        Assert.Equal("Double(Floor(Value))", doubled.Dependencies);
    }

    [Fact]
    public void Transform_CombinedWithOperators_ShouldWork()
    {
        // Arrange
        var a = new Traceable<double>(10.7, "A");
        var b = new Traceable<double>(5.3, "B");

        // Act
        var sum = a + b;
        var rounded = sum.Transform<double, int>(x => (int)Math.Floor(x), "Floor");

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
        var a = new Traceable<int>(1, "A");
        var b = new Traceable<int>(2, "B");
        var sum = a + b;

        // Act
        var transformed = sum.Transform<int, int>(x => x * 2, "Double");
        var dependencyNames = transformed.GetDependencyNames().ToList();

        // Assert
        Assert.Multiple(
            () => Assert.Equal(2, dependencyNames.Count),
            () => Assert.Contains("A", dependencyNames),
            () => Assert.Contains("B", dependencyNames)
        );
    }

    [Fact]
    public void Transform_WithDescription_ShouldDisplayInBuildGraph()
    {
        // Arrange
        var value = new Traceable<int>(100, "Value");

        // Act
        var transformed = value.Transform<int, string>(x => $"${x}", "Format");
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
        var value = new Traceable<double>(7.5, "Value");

        // Act
        var intValue = value.Transform<double, int>(x => (int)x, "ToInt");

        // Assert
        Assert.IsType<int>(intValue.Resolve());
        Assert.Equal(7, intValue.Resolve());
    }

    [Fact]
    public void Transform_NumericToString_ShouldWork()
    {
        // Arrange
        var number = new Traceable<decimal>(19.99m, "Price");

        // Act
        var formatted = number.Transform<decimal, string>(x => $"${x:F2}", "FormatPrice");

        // Assert
        Assert.Equal("$19.99", formatted.Resolve());
    }

    [Fact]
    public void Transform_NumericToBool_ShouldWork()
    {
        // Arrange
        var temperature = new Traceable<int>(75, "Temperature");

        // Act
        var isHot = temperature.Transform<int, bool>(x => x > 80, "IsHot");

        // Assert
        Assert.False(isHot.Resolve());
        Assert.Equal("IsHot(Temperature)", isHot.Dependencies);
    }

    [Fact]
    public void Transform_DifferentInputTypesForMultiInput_ShouldWork()
    {
        // Arrange
        var number = new Traceable<int>(42, "Number");
        var text = new Traceable<string>("Answer", "Text");

        // Act
        var combined = TraceableExtensions.Transform(text, number, (t, n) => $"{t}: {n}", "Combine");

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
            nullSource.Transform<int, string>(x => x.ToString(), "ToString"));
    }

    [Fact]
    public void Transform_NullLabel_ShouldThrowArgumentException()
    {
        // Arrange
        var value = new Traceable<int>(5, "Value");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            value.Transform<int, int>(x => x * 2, null));
    }

    [Fact]
    public void Transform_EmptyLabel_ShouldThrowArgumentException()
    {
        // Arrange
        var value = new Traceable<int>(5, "Value");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            value.Transform<int, int>(x => x * 2, ""));
    }

    [Fact]
    public void Transform_WhitespaceLabel_ShouldThrowArgumentException()
    {
        // Arrange
        var value = new Traceable<int>(5, "Value");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            value.Transform<int, int>(x => x * 2, "   "));
    }

    [Fact]
    public void Transform_NullTransformer_ShouldThrowArgumentNullException()
    {
        // Arrange
        var value = new Traceable<int>(5, "Value");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            value.Transform<int, int>(null, "Double"));
    }

    [Fact]
    public void Transform_ExceptionInTransformer_ShouldBubbleUp()
    {
        // Arrange
        var value = new Traceable<int>(0, "Value");
        var divided = value.Transform<int, int>(x => 10 / x, "Reciprocal");

        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => divided.Resolve());
    }

    [Fact]
    public void Transform_TwoInputs_NullFirstOperand_ShouldThrowArgumentNullException()
    {
        // Arrange
        var b = new Traceable<int>(5, "B");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            TraceableExtensions.Transform<int, int, int>(null, b, (x, y) => x + y, "Sum"));
    }

    [Fact]
    public void Transform_ThreeInputs_NullThirdOperand_ShouldThrowArgumentNullException()
    {
        // Arrange
        var a = new Traceable<int>(1, "A");
        var b = new Traceable<int>(2, "B");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            TraceableExtensions.Transform<int, int, int, int>(a, b, null, (x, y, z) => (x + y + z) / 3, "Avg"));
    }

    #endregion

    #region Reload Propagation

    [Fact]
    public void Transform_AfterReload_ShouldRecomputeValue()
    {
        // Arrange
        var value = new Traceable<int>(10, "Value");
        var doubled = value.Transform<int, int>(x => x * 2, "Double");
        Assert.Equal(20, doubled.Resolve());

        // Act
        value.Reload(15);

        // Assert
        Assert.Equal(30, doubled.Resolve());
    }

    [Fact]
    public void Transform_InComplexChain_ReloadShouldPropagate()
    {
        // Arrange
        var a = new Traceable<int>(5, "A");
        var b = new Traceable<int>(3, "B");
        var sum = a + b;
        var doubled = sum.Transform<int, int>(x => x * 2, "Double");
        var final = doubled + new Traceable<int>(10, "C");
        Assert.Equal(26, final.Resolve()); // (5 + 3) * 2 + 10 = 26

        // Act
        a.Reload(10);

        // Assert
        Assert.Equal(36, final.Resolve()); // (10 + 3) * 2 + 10 = 36
    }

    [Fact]
    public void Transform_MultiInput_AfterReload_ShouldRecompute()
    {
        // Arrange
        var a = new Traceable<int>(2, "A");
        var b = new Traceable<int>(3, "B");
        var product = TraceableExtensions.Transform(a, b, (x, y) => x * y, "Product");
        Assert.Equal(6, product.Resolve());

        // Act
        a.Reload(5);

        // Assert
        Assert.Equal(15, product.Resolve());
    }

    #endregion

    #region Edge Cases and Additional Scenarios

    [Fact]
    public void Transform_ComplexNestedBuildGraph_ShouldDisplayCorrectly()
    {
        // Arrange
        var a = new Traceable<double>(10.7, "A");
        var b = new Traceable<double>(5.3, "B");
        var sum = a + b;
        var rounded = sum.Transform<double, int>(x => (int)Math.Floor(x), "Floor");

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
        var a = new Traceable<int>(2, "A");
        var b = new Traceable<int>(3, "B");
        var c = new Traceable<int>(4, "C");
        var product = b * c;
        var sum = a + product;

        // Act
        var transformed = sum.Transform<int, string>(x => $"Result: {x}", "Format");

        // Assert
        Assert.Equal("Format(A + B * C)", transformed.Dependencies);
        Assert.Equal("Result: 14", transformed.Resolve());
    }

    [Fact]
    public void Transform_Name_ShouldMatchDependencies()
    {
        // Arrange
        var value = new Traceable<int>(42, "Value");

        // Act
        var transformed = value.Transform<int, string>(x => x.ToString(), "Stringify");

        // Assert
        Assert.Equal("Stringify(Value)", transformed.Name);
        Assert.Equal("Stringify(Value)", transformed.Dependencies);
    }

    [Fact]
    public void Transform_ValueAsObject_ShouldWork()
    {
        // Arrange
        var value = new Traceable<int>(42, "Value");

        // Act
        var transformed = value.Transform<int, string>(x => $"Number: {x}", "Format");

        // Assert
        Assert.Equal("Number: 42", transformed.ValueAsObject);
    }

    #endregion

    #region Split Tests

    [Fact]
    public void Split_TwoOutputs_ShouldCreateTwoTraceables()
    {
        // Arrange
        var value = new Traceable<decimal>(100m, "Price");

        // Act
        var (low, high) = value.Split(x => (x * 0.9m, x * 1.1m), "Low", "High");

        // Assert
        Assert.Multiple(
            () => Assert.Equal(90m, low.Resolve()),
            () => Assert.Equal(110m, high.Resolve()),
            () => Assert.Equal("Low(Price)", low.Dependencies),
            () => Assert.Equal("High(Price)", high.Dependencies)
        );
    }

    [Fact]
    public void Split_ThreeOutputs_ShouldCreateThreeTraceables()
    {
        // Arrange
        var value = new Traceable<int>(100, "Value");

        // Act
        var (min, mid, max) = value.Split(x => (x - 10, x, x + 10), "Min", "Mid", "Max");

        // Assert
        Assert.Multiple(
            () => Assert.Equal(90, min.Resolve()),
            () => Assert.Equal(100, mid.Resolve()),
            () => Assert.Equal(110, max.Resolve()),
            () => Assert.Equal("Min(Value)", min.Dependencies),
            () => Assert.Equal("Mid(Value)", mid.Dependencies),
            () => Assert.Equal("Max(Value)", max.Dependencies)
        );
    }

    [Fact]
    public void Split_DifferentOutputTypes_ShouldWork()
    {
        // Arrange
        var value = new Traceable<double>(3.14159, "Pi");

        // Act
        var (truncated, formatted) = value.Split(x => ((int)x, x.ToString("F2")), "Truncated", "Formatted");

        // Assert
        Assert.Equal(3, truncated.Resolve());
        Assert.Equal("3.14", formatted.Resolve());
    }

    [Fact]
    public void Split_FromComposite_ShouldTrackDependencies()
    {
        // Arrange
        var a = new Traceable<int>(10, "A");
        var b = new Traceable<int>(5, "B");
        var sum = a + b;

        // Act
        var (doubled, halved) = sum.Split(x => (x * 2, x / 2), "Doubled", "Halved");

        // Assert
        Assert.Multiple(
            () => Assert.Equal(30, doubled.Resolve()),
            () => Assert.Equal(7, halved.Resolve()),
            () => Assert.Equal("Doubled(A + B)", doubled.Dependencies),
            () => Assert.Equal("Halved(A + B)", halved.Dependencies)
        );
    }

    [Fact]
    public void Split_ReloadShouldPropagate()
    {
        // Arrange
        var value = new Traceable<int>(100, "Value");
        var (low, high) = value.Split(x => (x - 10, x + 10), "Low", "High");

        Assert.Equal(90, low.Resolve());
        Assert.Equal(110, high.Resolve());

        // Act
        value.Reload(200);

        // Assert
        Assert.Equal(190, low.Resolve());
        Assert.Equal(210, high.Resolve());
    }

    [Fact]
    public void Split_BuildGraph_ShouldShowSourceAsChild()
    {
        // Arrange
        var value = new Traceable<int>(50, "Value");
        var (low, high) = value.Split(x => (x - 5, x + 5), "Low", "High");

        // Act
        var lowGraph = low.BuildGraph();
        var highGraph = high.BuildGraph();

        // Assert
        Assert.Multiple(
            () => Assert.Equal("Low", lowGraph.Operation),
            () => Assert.Single(lowGraph.Children),
            () => Assert.Equal("Value", lowGraph.Children[0].Name),
            () => Assert.Equal("High", highGraph.Operation),
            () => Assert.Single(highGraph.Children),
            () => Assert.Equal("Value", highGraph.Children[0].Name)
        );
    }

    [Fact]
    public void Split_OutputsCanBeUsedInFurtherOperations()
    {
        // Arrange
        var value = new Traceable<int>(100, "Value");
        var (low, high) = value.Split(x => (x - 20, x + 20), "Low", "High");

        // Act
        var range = high - low;

        // Assert
        Assert.Equal(40, range.Resolve());
        Assert.Equal("High(Value) - Low(Value)", range.Dependencies);
    }

    [Fact]
    public void Split_NullSource_ShouldThrow()
    {
        // Arrange
        ITraceable<int> nullSource = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            nullSource.Split(x => (x, x), "A", "B"));
    }

    [Fact]
    public void Split_NullSplitter_ShouldThrow()
    {
        // Arrange
        var value = new Traceable<int>(10, "Value");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            value.Split<int, int, int>(null, "A", "B"));
    }

    [Fact]
    public void Split_NullLabel_ShouldThrow()
    {
        // Arrange
        var value = new Traceable<int>(10, "Value");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            value.Split(x => (x, x), null, "B"));
        Assert.Throws<ArgumentException>(() =>
            value.Split(x => (x, x), "A", null));
    }

    #endregion
}
