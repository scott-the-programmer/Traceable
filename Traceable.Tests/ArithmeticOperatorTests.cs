namespace EntityTrace.Tests;

public class ArithmeticOperatorTests
{
    #region Integer

    [Fact]
    public void Addition_Int_ShouldCreateCompositeWithCorrectValue()
    {
        // Arrange
        var a = new Traceable<int>(5, "A");
        var b = new Traceable<int>(3, "B");

        // Act
        var sum = a + b;

        // Assert
        Assert.Equal(8, sum.Resolve());
        Assert.Equal("A + B", sum.Dependencies);
    }

    [Fact]
    public void Subtraction_Int_ShouldWork()
    {
        // Arrange
        var a = new Traceable<int>(10, "A");
        var b = new Traceable<int>(3, "B");

        // Act
        var diff = a - b;

        // Assert
        Assert.Equal(7, diff.Resolve());
        Assert.Equal("A - B", diff.Dependencies);
    }

    [Fact]
    public void Multiplication_Int_ShouldWork()
    {
        // Arrange
        var a = new Traceable<int>(5, "A");
        var b = new Traceable<int>(3, "B");

        // Act
        var product = a * b;

        // Assert
        Assert.Equal(15, product.Resolve());
        Assert.Equal("A * B", product.Dependencies);
    }

    [Fact]
    public void Division_Int_ShouldWork()
    {
        // Arrange
        var a = new Traceable<int>(15, "A");
        var b = new Traceable<int>(3, "B");

        // Act
        var quotient = a / b;

        // Assert
        Assert.Equal(5, quotient.Resolve());
        Assert.Equal("A / B", quotient.Dependencies);
    }

    #endregion

    #region Decimal

    [Fact]
    public void Addition_Decimal_ShouldWork()
    {
        // Arrange
        var a = new Traceable<decimal>(5.5m, "A");
        var b = new Traceable<decimal>(3.2m, "B");

        // Act
        var sum = a + b;

        // Assert
        Assert.Equal(8.7m, sum.Resolve());
    }

    [Fact]
    public void Multiplication_Decimal_ShouldWork()
    {
        // Arrange
        var a = new Traceable<decimal>(5.5m, "A");
        var b = new Traceable<decimal>(2.0m, "B");

        // Act
        var product = a * b;

        // Assert
        Assert.Equal(11.0m, product.Resolve());
    }

    #endregion

    #region Double

    [Fact]
    public void Addition_Double_ShouldWork()
    {
        // Arrange
        var a = new Traceable<double>(5.5, "A");
        var b = new Traceable<double>(3.2, "B");

        // Act
        var sum = a + b;

        // Assert
        Assert.Equal(8.7, sum.Resolve(), 4);
    }

    #endregion

    #region String Concatenation

    [Fact]
    public void Addition_String_ShouldConcatenate()
    {
        // Arrange
        var first = new Traceable<string>("Hello", "First");
        var last = new Traceable<string>("World", "Last");

        // Act
        var full = first + last;

        // Assert
        Assert.Equal("HelloWorld", full.Resolve());
        Assert.Equal("First + Last", full.Dependencies);
    }

    [Fact]
    public void Subtraction_OnString_ShouldThrow()
    {
        // Arrange
        var a = new Traceable<string>("Hello", "A");
        var b = new Traceable<string>("World", "B");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => { var result = a - b; });

        // Assert
        Assert.Contains("Operator - not supported for type String", ex.Message);
    }

    #endregion

    #region Complex Expressions

    [Fact]
    public void ComplexExpression_ShouldMaintainPrecedence()
    {
        // Arrange
        var a = new Traceable<int>(2, "A");
        var b = new Traceable<int>(3, "B");
        var c = new Traceable<int>(4, "C");

        // Act
        var result = a + b * c;

        // Assert
        Assert.Equal(14, result.Resolve()); // 2 + (3 * 4) = 14
        Assert.Equal("A + B * C", result.Dependencies);
    }

    [Fact]
    public void ComplexExpressionWithParentheses_ShouldWork()
    {
        // Arrange
        var a = new Traceable<int>(2, "A");
        var b = new Traceable<int>(3, "B");
        var c = new Traceable<int>(4, "C");

        // Act
        var result = (a + b) * c;

        // Assert
        Assert.Equal(20, result.Resolve()); // (2 + 3) * 4 = 20
        Assert.Equal("(A + B) * C", result.Dependencies);
    }

    [Fact]
    public void GetDependencyNames_ForComplexExpression_ShouldReturnAllBaseEntities()
    {
        // Arrange
        var a = new Traceable<int>(1, "A");
        var b = new Traceable<int>(2, "B");
        var c = new Traceable<int>(3, "C");

        // Act
        var result = (a + b) * c;

        // Assert
        Assert.Equal(new[] { "A", "B", "C" }, result.GetDependencyNames());
    }

    #endregion

    #region Reload Propagation

    [Fact]
    public void Reload_ShouldPropagateToComposites()
    {
        // Arrange
        var a = new Traceable<int>(10, "A");
        var b = new Traceable<int>(5, "B");
        var sum = a + b;
        Assert.Equal(15, sum.Resolve());

        // Act
        a.Reload(20);

        // Assert
        Assert.Equal(25, sum.Resolve());
    }

    [Fact]
    public void Reload_OnComposite_ShouldThrow()
    {
        // Arrange
        var a = new Traceable<int>(1, "A");
        var b = new Traceable<int>(2, "B");
        var sum = a + b;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => sum.Reload(10));

        // Assert
        Assert.Contains("Cannot reload a composite entity", ex.Message);
    }

    #endregion
}
