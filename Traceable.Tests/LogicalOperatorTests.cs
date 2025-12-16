namespace Traceable.Tests;

public class LogicalOperatorTests
{
    [Fact]
    public void LogicalAnd_Bool_ShouldWork()
    {
        var a = new Traceable<bool>(true, "A");
        var b = new Traceable<bool>(true, "B");

        var result = a & b;

        Assert.True(result.Resolve());
        Assert.Equal("A & B", result.Dependencies);
    }

    [Fact]
    public void LogicalAnd_Bool_WithFalse_ShouldReturnFalse()
    {
        var a = new Traceable<bool>(true, "A");
        var b = new Traceable<bool>(false, "B");

        var result = a & b;

        Assert.False(result.Resolve());
    }

    [Fact]
    public void LogicalOr_Bool_ShouldWork()
    {
        var a = new Traceable<bool>(false, "A");
        var b = new Traceable<bool>(true, "B");

        var result = a | b;

        Assert.True(result.Resolve());
        Assert.Equal("A | B", result.Dependencies);
    }

    [Fact]
    public void BooleanAnd_OnNonBool_ShouldThrow()
    {
        var a = new Traceable<int>(1, "A");
        var b = new Traceable<int>(2, "B");

        var ex = Assert.Throws<InvalidOperationException>(() => { var result = a & b; });
        Assert.Contains("Operator & not supported for type Int32", ex.Message);
    }
}
