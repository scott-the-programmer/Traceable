namespace EntityTrace.Tests;

public class TraceableTests
{
    [Fact]
    public void Constructor_ShouldCreateBaseEntity()
    {
        var entity = new Traceable<int>("TestEntity", 42);

        Assert.Equal("TestEntity", entity.Name);
        Assert.Equal(42, entity.Value);
        Assert.Equal(42, entity.Resolve());
    }

    [Fact]
    public void Description_ShouldBeSettable()
    {
        var entity = new Traceable<int>("Test", 10);

        entity.Description = "A test entity";

        Assert.Equal("A test entity", entity.Description);
    }

    [Fact]
    public void Dependencies_ForBaseEntity_ShouldReturnName()
    {
        var entity = new Traceable<int>("MyEntity", 100);

        Assert.Equal("MyEntity", entity.Dependencies);
    }

    [Fact]
    public void GetDependencyNames_ForBaseEntity_ShouldReturnSingleName()
    {
        var entity = new Traceable<int>("BaseEntity", 5);

        Assert.Equal(new[] { "BaseEntity" }, entity.GetDependencyNames());
    }

    [Fact]
    public void Reset_ShouldUpdateValue()
    {
        var entity = new Traceable<int>("Counter", 10);

        entity.Reset(20);

        Assert.Equal(20, entity.Resolve());
        Assert.Equal(20, entity.Value);
    }

    [Fact]
    public void BuildGraph_ForBaseEntity_ShouldShowNameAndValue()
    {
        var entity = new Traceable<int>("MyValue", 42);
        entity.Description = "Test Value";

        var graph = entity.BuildGraph();

        Assert.Equal("MyValue", graph.Name);
        Assert.Equal("Test Value", graph.Description);
        Assert.Equal(42, graph.Value);
        Assert.True(graph.IsBase);
        Assert.Empty(graph.Children);
    }

    [Fact]
    public void BuildGraph_ForBaseEntityWithoutDescription_ShouldShowNameAndValue()
    {
        var entity = new Traceable<int>("MyValue", 42);

        var graph = entity.BuildGraph();

        Assert.Equal("MyValue", graph.Name);
        Assert.Null(graph.Description);
        Assert.Equal(42, graph.Value);
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Traceable<int>(null!, 10));
        Assert.Contains("Name cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Traceable<int>("", 10));
        Assert.Contains("Name cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void ValueAsObject_ShouldReturnBoxedValue()
    {
        var entity = new Traceable<int>("Test", 42);

        var value = entity.ValueAsObject;

        Assert.Equal(42, value);
        Assert.IsType<int>(value);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void IntegerEntity_ShouldHandleVariousValues(int value)
    {
        var entity = new Traceable<int>("Test", value);

        Assert.Equal(value, entity.Resolve());
    }

    [Theory]
    [InlineData(10.5)]
    [InlineData(0.0)]
    [InlineData(-5.75)]
    public void DecimalEntity_ShouldHandleVariousValues(double value)
    {
        var entity = new Traceable<decimal>("Test", (decimal)value);

        Assert.Equal((decimal)value, entity.Resolve());
    }

    [Fact]
    public void BooleanEntity_ShouldWork()
    {
        var trueEntity = new Traceable<bool>("IsTrue", true);
        var falseEntity = new Traceable<bool>("IsFalse", false);

        Assert.True(trueEntity.Resolve());
        Assert.False(falseEntity.Resolve());
    }

    [Fact]
    public void StringEntity_ShouldWork()
    {
        var entity = new Traceable<string>("Name", "Hello");

        Assert.Equal("Hello", entity.Resolve());
    }
}
