namespace EntityTrace.Tests;

public class ArithmeticOperatorTests
{
    #region Integer

    [Fact]
    public void Addition_Int_ShouldCreateCompositeWithCorrectValue()
    {
        var a = new Traceable<int>("A", 5);
        var b = new Traceable<int>("B", 3);

        var sum = a + b;

        Assert.Equal(8, sum.Resolve());
        Assert.Equal("A + B", sum.Dependencies);
    }

    [Fact]
    public void Subtraction_Int_ShouldWork()
    {
        var a = new Traceable<int>("A", 10);
        var b = new Traceable<int>("B", 3);

        var diff = a - b;

        Assert.Equal(7, diff.Resolve());
        Assert.Equal("A - B", diff.Dependencies);
    }

    [Fact]
    public void Multiplication_Int_ShouldWork()
    {
        var a = new Traceable<int>("A", 5);
        var b = new Traceable<int>("B", 3);

        var product = a * b;

        Assert.Equal(15, product.Resolve());
        Assert.Equal("A * B", product.Dependencies);
    }

    [Fact]
    public void Division_Int_ShouldWork()
    {
        var a = new Traceable<int>("A", 15);
        var b = new Traceable<int>("B", 3);

        var quotient = a / b;

        Assert.Equal(5, quotient.Resolve());
        Assert.Equal("A / B", quotient.Dependencies);
    }

    #endregion

    #region Decimal

    [Fact]
    public void Addition_Decimal_ShouldWork()
    {
        var a = new Traceable<decimal>("A", 5.5m);
        var b = new Traceable<decimal>("B", 3.2m);

        var sum = a + b;

        Assert.Equal(8.7m, sum.Resolve());
    }

    [Fact]
    public void Multiplication_Decimal_ShouldWork()
    {
        var a = new Traceable<decimal>("A", 5.5m);
        var b = new Traceable<decimal>("B", 2.0m);

        var product = a * b;

        Assert.Equal(11.0m, product.Resolve());
    }

    #endregion

    #region Double

    [Fact]
    public void Addition_Double_ShouldWork()
    {
        var a = new Traceable<double>("A", 5.5);
        var b = new Traceable<double>("B", 3.2);

        var sum = a + b;

        Assert.Equal(8.7, sum.Resolve(), 4);
    }

    #endregion

    #region String Concatenation

    [Fact]
    public void Addition_String_ShouldConcatenate()
    {
        var first = new Traceable<string>("First", "Hello");
        var last = new Traceable<string>("Last", "World");

        var full = first + last;

        Assert.Equal("HelloWorld", full.Resolve());
        Assert.Equal("First + Last", full.Dependencies);
    }

    [Fact]
    public void Subtraction_OnString_ShouldThrow()
    {
        var a = new Traceable<string>("A", "Hello");
        var b = new Traceable<string>("B", "World");

        var ex = Assert.Throws<InvalidOperationException>(() => { var result = a - b; });
        Assert.Contains("Operator - not supported for type String", ex.Message);
    }

    #endregion

    #region Complex Expressions

    [Fact]
    public void ComplexExpression_ShouldMaintainPrecedence()
    {
        var a = new Traceable<int>("A", 2);
        var b = new Traceable<int>("B", 3);
        var c = new Traceable<int>("C", 4);

        var result = a + b * c;

        Assert.Equal(14, result.Resolve()); // 2 + (3 * 4) = 14
        Assert.Equal("A + B * C", result.Dependencies);
    }

    [Fact]
    public void ComplexExpressionWithParentheses_ShouldWork()
    {
        var a = new Traceable<int>("A", 2);
        var b = new Traceable<int>("B", 3);
        var c = new Traceable<int>("C", 4);

        var result = (a + b) * c;

        Assert.Equal(20, result.Resolve()); // (2 + 3) * 4 = 20
        Assert.Equal("(A + B) * C", result.Dependencies);
    }

    [Fact]
    public void GetDependencyNames_ForComplexExpression_ShouldReturnAllBaseEntities()
    {
        var a = new Traceable<int>("A", 1);
        var b = new Traceable<int>("B", 2);
        var c = new Traceable<int>("C", 3);

        var result = (a + b) * c;

        Assert.Equal(new[] { "A", "B", "C" }, result.GetDependencyNames());
    }

    #endregion

    #region Reset Propagation

    [Fact]
    public void Reset_ShouldPropagateToComposites()
    {
        var a = new Traceable<int>("A", 10);
        var b = new Traceable<int>("B", 5);
        var sum = a + b;

        Assert.Equal(15, sum.Resolve());

        a.Reset(20);

        Assert.Equal(25, sum.Resolve());
    }

    [Fact]
    public void Reset_OnComposite_ShouldThrow()
    {
        var a = new Traceable<int>("A", 1);
        var b = new Traceable<int>("B", 2);
        var sum = a + b;

        var ex = Assert.Throws<InvalidOperationException>(() => sum.Reset(10));
        Assert.Contains("Cannot reset a composite entity", ex.Message);
    }

    #endregion
}
