namespace EntityTrace.Tests;

public class ComparisonOperatorTests
{
    [Fact]
    public void GreaterThan_Int_ShouldReturnTraceableBool()
    {
        var a = new Traceable<int>(10, "A");
        var b = new Traceable<int>(5, "B");

        var result = a > b;

        Assert.IsType<Traceable<bool>>(result);
        Assert.True(result.Resolve());
        Assert.Equal("A > B", result.Dependencies);
    }

    [Fact]
    public void LessThan_Int_ShouldWork()
    {
        var a = new Traceable<int>(5, "A");
        var b = new Traceable<int>(10, "B");

        var result = a < b;

        Assert.True(result.Resolve());
    }

    [Fact]
    public void GreaterThanOrEqual_Int_ShouldWork()
    {
        var a = new Traceable<int>(10, "A");
        var b = new Traceable<int>(10, "B");

        var result = a >= b;

        Assert.True(result.Resolve());
    }

    [Fact]
    public void LessThanOrEqual_Int_ShouldWork()
    {
        var a = new Traceable<int>(5, "A");
        var b = new Traceable<int>(10, "B");

        var result = a <= b;

        Assert.True(result.Resolve());
    }

    [Fact]
    public void Equality_Int_ShouldWork()
    {
        var a = new Traceable<int>(10, "A");
        var b = new Traceable<int>(10, "B");

        var result = a == b;

        Assert.True(result.Resolve());
    }

    [Fact]
    public void Inequality_Int_ShouldWork()
    {
        var a = new Traceable<int>(10, "A");
        var b = new Traceable<int>(5, "B");

        var result = a != b;

        Assert.True(result.Resolve());
    }
}
