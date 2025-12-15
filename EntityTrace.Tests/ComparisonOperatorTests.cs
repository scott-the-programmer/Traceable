namespace EntityTrace.Tests;

public class ComparisonOperatorTests
{
    [Fact]
    public void GreaterThan_Int_ShouldReturnTraceableBool()
    {
        var a = new Traceable<int>("A", 10);
        var b = new Traceable<int>("B", 5);

        var result = a > b;

        Assert.IsType<Traceable<bool>>(result);
        Assert.True(result.Resolve());
        Assert.Equal("A > B", result.Dependencies);
    }

    [Fact]
    public void LessThan_Int_ShouldWork()
    {
        var a = new Traceable<int>("A", 5);
        var b = new Traceable<int>("B", 10);

        var result = a < b;

        Assert.True(result.Resolve());
    }

    [Fact]
    public void GreaterThanOrEqual_Int_ShouldWork()
    {
        var a = new Traceable<int>("A", 10);
        var b = new Traceable<int>("B", 10);

        var result = a >= b;

        Assert.True(result.Resolve());
    }

    [Fact]
    public void LessThanOrEqual_Int_ShouldWork()
    {
        var a = new Traceable<int>("A", 5);
        var b = new Traceable<int>("B", 10);

        var result = a <= b;

        Assert.True(result.Resolve());
    }

    [Fact]
    public void Equality_Int_ShouldWork()
    {
        var a = new Traceable<int>("A", 10);
        var b = new Traceable<int>("B", 10);

        var result = a == b;

        Assert.True(result.Resolve());
    }

    [Fact]
    public void Inequality_Int_ShouldWork()
    {
        var a = new Traceable<int>("A", 10);
        var b = new Traceable<int>("B", 5);

        var result = a != b;

        Assert.True(result.Resolve());
    }
}
