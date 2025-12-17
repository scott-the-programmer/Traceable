namespace Traceable.Tests;

public class CustomTypeTests
{
    #region Vector2D Tests

    [Fact]
    public void Vector2D_Addition_WithInterface_Works()
    {
        // Arrange
        var v1 = new Traceable<Vector2D>(new Vector2D(1, 2), "V1");
        var v2 = new Traceable<Vector2D>(new Vector2D(3, 4), "V2");

        // Act
        var sum = v1 + v2;

        // Assert
        Assert.Equal(new Vector2D(4, 6), sum.Resolve());
        Assert.Equal("V1 + V2", sum.Dependencies);
    }

    [Fact]
    public void Vector2D_Subtraction_WithInterface_Works()
    {
        // Arrange
        var v1 = new Traceable<Vector2D>(new Vector2D(5, 7), "V1");
        var v2 = new Traceable<Vector2D>(new Vector2D(2, 3), "V2");

        // Act
        var diff = v1 - v2;

        // Assert
        Assert.Equal(new Vector2D(3, 4), diff.Resolve());
        Assert.Equal("V1 - V2", diff.Dependencies);
    }

    [Fact]
    public void Vector2D_ComplexExpression_TracksCorrectly()
    {
        // Arrange
        var a = new Traceable<Vector2D>(new Vector2D(1, 1), "A");
        var b = new Traceable<Vector2D>(new Vector2D(2, 2), "B");
        var c = new Traceable<Vector2D>(new Vector2D(3, 3), "C");

        // Act
        var result = a + b - c;

        // Assert
        Assert.Equal(new Vector2D(0, 0), result.Resolve());
        Assert.Equal("A + B - C", result.Dependencies);
    }

    [Fact]
    public void Vector2D_Multiplication_WithoutInterface_Throws()
    {
        // Arrange
        var v1 = new Traceable<Vector2D>(new Vector2D(1, 2), "V1");
        var v2 = new Traceable<Vector2D>(new Vector2D(3, 4), "V2");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => { var result = v1 * v2; });

        // Assert
        Assert.Contains("Operator * not supported for type Vector2D", ex.Message);
        Assert.Contains("ITraceableMultiplicable", ex.Message);
    }

    [Fact]
    public void Vector2D_Division_WithoutInterface_Throws()
    {
        // Arrange
        var v1 = new Traceable<Vector2D>(new Vector2D(1, 2), "V1");
        var v2 = new Traceable<Vector2D>(new Vector2D(3, 4), "V2");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => { var result = v1 / v2; });

        // Assert
        Assert.Contains("Operator / not supported for type Vector2D", ex.Message);
        Assert.Contains("ITraceableDividable", ex.Message);
    }

    [Fact]
    public void Vector2D_BuildGraph_DisplaysCorrectly()
    {
        // Arrange
        var v1 = new Traceable<Vector2D>(new Vector2D(1, 2), "V1");
        var v2 = new Traceable<Vector2D>(new Vector2D(3, 4), "V2");
        var sum = v1 + v2;

        // Act
        var graph = sum.BuildGraph();

        // Assert
        Assert.Multiple(
            () => Assert.Equal("+", graph.Operation),
            () => Assert.Equal(new Vector2D(4, 6), graph.Value),
            () => Assert.Equal(2, graph.Children.Count),
            () => Assert.Equal("V1", graph.Children[0].Name),
            () => Assert.Equal("V2", graph.Children[1].Name)
        );
    }

    [Fact]
    public void Vector2D_Reload_PropagatesChanges()
    {
        // Arrange
        var v1 = new Traceable<Vector2D>(new Vector2D(1, 2), "V1");
        var v2 = new Traceable<Vector2D>(new Vector2D(3, 4), "V2");
        var sum = v1 + v2;
        Assert.Equal(new Vector2D(4, 6), sum.Resolve());

        // Act
        v1.Reload(new Vector2D(10, 20));

        // Assert
        Assert.Equal(new Vector2D(13, 24), sum.Resolve());
    }

    #endregion

    #region Money Tests

    [Fact]
    public void Money_Addition_SameCurrency_Works()
    {
        // Arrange
        var price = new Traceable<Money>(new Money(100m, "USD"), "Price");
        var tax = new Traceable<Money>(new Money(10m, "USD"), "Tax");

        // Act
        var total = price + tax;

        // Assert
        Assert.Equal(new Money(110m, "USD"), total.Resolve());
        Assert.Equal("Price + Tax", total.Dependencies);
    }

    [Fact]
    public void Money_Subtraction_SameCurrency_Works()
    {
        // Arrange
        var total = new Traceable<Money>(new Money(100m, "USD"), "Total");
        var discount = new Traceable<Money>(new Money(15m, "USD"), "Discount");

        // Act
        var final = total - discount;

        // Assert
        Assert.Equal(new Money(85m, "USD"), final.Resolve());
        Assert.Equal("Total - Discount", final.Dependencies);
    }

    [Fact]
    public void Money_Multiplication_Works()
    {
        // Arrange
        var price = new Traceable<Money>(new Money(25m, "USD"), "Price");
        var quantity = new Traceable<Money>(new Money(4m, "USD"), "Quantity");

        // Act
        var total = price * quantity;

        // Assert
        Assert.Equal(new Money(100m, "USD"), total.Resolve());
        Assert.Equal("Price * Quantity", total.Dependencies);
    }

    [Fact]
    public void Money_Division_Works()
    {
        // Arrange
        var total = new Traceable<Money>(new Money(100m, "USD"), "Total");
        var divisor = new Traceable<Money>(new Money(4m, "USD"), "Divisor");

        // Act
        var result = total / divisor;

        // Assert
        Assert.Equal(new Money(25m, "USD"), result.Resolve());
        Assert.Equal("Total / Divisor", result.Dependencies);
    }

    [Fact]
    public void Money_ComplexExpression_Works()
    {
        // Arrange
        var price = new Traceable<Money>(new Money(100m, "USD"), "Price");
        var quantity = new Traceable<Money>(new Money(5m, "USD"), "Quantity");
        var discount = new Traceable<Money>(new Money(50m, "USD"), "Discount");

        // Act
        var final = price * quantity - discount;

        // Assert
        Assert.Equal(new Money(450m, "USD"), final.Resolve());
        Assert.Equal("Price * Quantity - Discount", final.Dependencies);
    }

    [Fact]
    public void Money_Addition_DifferentCurrency_Throws()
    {
        // Arrange
        var usd = new Traceable<Money>(new Money(100m, "USD"), "USD");
        var eur = new Traceable<Money>(new Money(100m, "EUR"), "EUR");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => { var result = (usd + eur).Resolve(); });

        // Assert
        Assert.Contains("different currencies", ex.Message);
    }

    [Fact]
    public void Money_Comparison_SameCurrency_Works()
    {
        // Arrange
        var a = new Traceable<Money>(new Money(100m, "USD"), "A");
        var b = new Traceable<Money>(new Money(50m, "USD"), "B");

        // Act
        var isGreater = a > b;
        var isLess = a < b;

        // Assert
        Assert.Multiple(
            () => Assert.True(isGreater.Resolve()),
            () => Assert.False(isLess.Resolve()),
            () => Assert.Equal("A > B", isGreater.Dependencies),
            () => Assert.Equal("A < B", isLess.Dependencies)
        );
    }

    [Fact]
    public void Money_Comparison_DifferentCurrency_Throws()
    {
        // Arrange
        var usd = new Traceable<Money>(new Money(100m, "USD"), "USD");
        var eur = new Traceable<Money>(new Money(50m, "EUR"), "EUR");

        // Act
        var comparison = usd > eur;
        var ex = Assert.Throws<InvalidOperationException>(() => comparison.Resolve());

        // Assert
        Assert.Contains("different currencies", ex.Message);
    }

    #endregion

    #region TriState Tests

    [Fact]
    public void TriState_And_TrueAndTrue_ReturnsTrue()
    {
        // Arrange
        var a = new Traceable<TriState>(new TriState(TriState.State.True), "A");
        var b = new Traceable<TriState>(new TriState(TriState.State.True), "B");

        // Act
        var result = a & b;

        // Assert
        Assert.Equal(new TriState(TriState.State.True), result.Resolve());
        Assert.Equal("A & B", result.Dependencies);
    }

    [Fact]
    public void TriState_And_TrueAndFalse_ReturnsFalse()
    {
        // Arrange
        var a = new Traceable<TriState>(new TriState(TriState.State.True), "A");
        var b = new Traceable<TriState>(new TriState(TriState.State.False), "B");

        // Act
        var result = a & b;

        // Assert
        Assert.Equal(new TriState(TriState.State.False), result.Resolve());
    }

    [Fact]
    public void TriState_And_TrueAndUnknown_ReturnsUnknown()
    {
        // Arrange
        var a = new Traceable<TriState>(new TriState(TriState.State.True), "A");
        var b = new Traceable<TriState>(new TriState(TriState.State.Unknown), "B");

        // Act
        var result = a & b;

        // Assert
        Assert.Equal(new TriState(TriState.State.Unknown), result.Resolve());
    }

    [Fact]
    public void TriState_Or_FalseOrFalse_ReturnsFalse()
    {
        // Arrange
        var a = new Traceable<TriState>(new TriState(TriState.State.False), "A");
        var b = new Traceable<TriState>(new TriState(TriState.State.False), "B");

        // Act
        var result = a | b;

        // Assert
        Assert.Equal(new TriState(TriState.State.False), result.Resolve());
        Assert.Equal("A | B", result.Dependencies);
    }

    [Fact]
    public void TriState_Or_TrueOrFalse_ReturnsTrue()
    {
        // Arrange
        var a = new Traceable<TriState>(new TriState(TriState.State.True), "A");
        var b = new Traceable<TriState>(new TriState(TriState.State.False), "B");

        // Act
        var result = a | b;

        // Assert
        Assert.Equal(new TriState(TriState.State.True), result.Resolve());
    }

    [Fact]
    public void TriState_Or_FalseOrUnknown_ReturnsUnknown()
    {
        // Arrange
        var a = new Traceable<TriState>(new TriState(TriState.State.False), "A");
        var b = new Traceable<TriState>(new TriState(TriState.State.Unknown), "B");

        // Act
        var result = a | b;

        // Assert
        Assert.Equal(new TriState(TriState.State.Unknown), result.Resolve());
    }

    [Fact]
    public void TriState_ComplexExpression_TracksCorrectly()
    {
        // Arrange
        var a = new Traceable<TriState>(new TriState(TriState.State.True), "A");
        var b = new Traceable<TriState>(new TriState(TriState.State.False), "B");
        var c = new Traceable<TriState>(new TriState(TriState.State.Unknown), "C");

        // Act
        var result = a & b | c;

        // Assert
        Assert.Equal(new TriState(TriState.State.Unknown), result.Resolve());
        Assert.Equal("A & B | C", result.Dependencies);
    }

    #endregion

    #region Error Message Tests

    [Fact]
    public void UnsupportedType_Addition_ShowsHelpfulErrorMessage()
    {
        // Arrange
        var a = new Traceable<DateTime>(DateTime.Now, "A");
        var b = new Traceable<DateTime>(DateTime.Now, "B");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => { var result = a + b; });

        // Assert
        Assert.Contains("Operator + not supported for type DateTime", ex.Message);
        Assert.Contains("ITraceableAddable<DateTime>", ex.Message);
    }

    [Fact]
    public void UnsupportedType_Subtraction_ShowsHelpfulErrorMessage()
    {
        // Arrange
        var a = new Traceable<DateTime>(DateTime.Now, "A");
        var b = new Traceable<DateTime>(DateTime.Now, "B");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => { var result = a - b; });

        // Assert
        Assert.Contains("Operator - not supported for type DateTime", ex.Message);
        Assert.Contains("ITraceableSubtractable<DateTime>", ex.Message);
    }

    [Fact]
    public void UnsupportedType_Multiplication_ShowsHelpfulErrorMessage()
    {
        // Arrange
        var a = new Traceable<DateTime>(DateTime.Now, "A");
        var b = new Traceable<DateTime>(DateTime.Now, "B");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => { var result = a * b; });

        // Assert
        Assert.Contains("Operator * not supported for type DateTime", ex.Message);
        Assert.Contains("ITraceableMultiplicable<DateTime>", ex.Message);
    }

    [Fact]
    public void UnsupportedType_Division_ShowsHelpfulErrorMessage()
    {
        // Arrange
        var a = new Traceable<DateTime>(DateTime.Now, "A");
        var b = new Traceable<DateTime>(DateTime.Now, "B");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => { var result = a / b; });

        // Assert
        Assert.Contains("Operator / not supported for type DateTime", ex.Message);
        Assert.Contains("ITraceableDividable<DateTime>", ex.Message);
    }

    [Fact]
    public void UnsupportedType_LogicalAnd_ShowsHelpfulErrorMessage()
    {
        // Arrange
        var a = new Traceable<int>(1, "A");
        var b = new Traceable<int>(2, "B");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => { var result = a & b; });

        // Assert
        Assert.Contains("Operator & not supported for type Int32", ex.Message);
        Assert.Contains("ITraceableLogical<Int32>", ex.Message);
    }

    [Fact]
    public void UnsupportedType_LogicalOr_ShowsHelpfulErrorMessage()
    {
        // Arrange
        var a = new Traceable<int>(1, "A");
        var b = new Traceable<int>(2, "B");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => { var result = a | b; });

        // Assert
        Assert.Contains("Operator | not supported for type Int32", ex.Message);
        Assert.Contains("ITraceableLogical<Int32>", ex.Message);
    }

    #endregion
}
