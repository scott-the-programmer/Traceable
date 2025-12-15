namespace EntityTrace.Tests;

public class LogicalOperatorTests
{
    [Fact]
    public void LogicalAnd_Bool_ShouldWork()
    {
        var a = new Traceable<bool>("A", true);
        var b = new Traceable<bool>("B", true);

        var result = a & b;

        Assert.True(result.Resolve());
        Assert.Equal("A & B", result.Dependencies);
    }

    [Fact]
    public void LogicalAnd_Bool_WithFalse_ShouldReturnFalse()
    {
        var a = new Traceable<bool>("A", true);
        var b = new Traceable<bool>("B", false);

        var result = a & b;

        Assert.False(result.Resolve());
    }

    [Fact]
    public void LogicalOr_Bool_ShouldWork()
    {
        var a = new Traceable<bool>("A", false);
        var b = new Traceable<bool>("B", true);

        var result = a | b;

        Assert.True(result.Resolve());
        Assert.Equal("A | B", result.Dependencies);
    }

    [Fact]
    public void BooleanAnd_OnNonBool_ShouldThrow()
    {
        var a = new Traceable<int>("A", 1);
        var b = new Traceable<int>("B", 2);

        var ex = Assert.Throws<InvalidOperationException>(() => { var result = a & b; });
        Assert.Contains("Operator & not supported for type Int32", ex.Message);
    }
}
