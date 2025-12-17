namespace Traceable.Tests;

public class TraceableTests
{
    [Fact]
    public void Constructor_ShouldCreateBaseEntity()
    {
        // Arrange
        var entity = new Traceable<int>(42, "TestEntity");

        // Act

        // Assert
        Assert.Multiple(
            () => Assert.Equal("TestEntity", entity.Name),
            () => Assert.Equal(42, entity.Value),
            () => Assert.Equal(42, entity.Resolve())
        );
    }

    [Fact]
    public void Description_ShouldBeSettable()
    {
        // Arrange
        var entity = new Traceable<int>(10, "Test");

        // Act
        entity.Description = "A test entity";

        // Assert
        Assert.Equal("A test entity", entity.Description);
    }

    [Fact]
    public void Dependencies_ForBaseEntity_ShouldReturnName()
    {
        // Arrange
        var entity = new Traceable<int>(100, "MyEntity");

        // Act

        // Assert
        Assert.Equal("MyEntity", entity.Dependencies);
    }

    [Fact]
    public void GetDependencyNames_ForBaseEntity_ShouldReturnSingleName()
    {
        // Arrange
        var entity = new Traceable<int>(5, "BaseEntity");

        // Act

        // Assert
        Assert.Equal(new[] { "BaseEntity" }, entity.GetDependencyNames());
    }

    [Fact]
    public void Reload_ShouldUpdateValue()
    {
        // Arrange
        var entity = new Traceable<int>(10, "Counter");

        // Act
        entity.Reload(20);

        // Assert
        Assert.Equal(20, entity.Resolve());
        Assert.Equal(20, entity.Value);
    }

    [Fact]
    public void BuildGraph_ForBaseEntity_ShouldShowNameAndValue()
    {
        // Arrange
        var entity = new Traceable<int>(42, "MyValue");
        entity.Description = "Test Value";

        // Act
        var graph = entity.BuildGraph();

        // Assert
        Assert.Multiple(
            () => Assert.Equal("MyValue", graph.Name),
            () => Assert.Equal("Test Value", graph.Description),
            () => Assert.Equal(42, graph.Value),
            () => Assert.True(graph.IsBase),
            () => Assert.Empty(graph.Children)
        );
    }

    [Fact]
    public void BuildGraph_ForBaseEntityWithoutDescription_ShouldShowNameAndValue()
    {
        // Arrange
        var entity = new Traceable<int>(42, "MyValue");

        // Act
        var graph = entity.BuildGraph();

        // Assert
        Assert.Multiple(
            () => Assert.Equal("MyValue", graph.Name),
            () => Assert.Null(graph.Description),
            () => Assert.Equal(42, graph.Value)
        );
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrow()
    {
        // Arrange

        // Act
        var ex = Assert.Throws<ArgumentException>(() => new Traceable<int>(10, null!));

        // Assert
        Assert.Contains("Name cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrow()
    {
        // Arrange

        // Act
        var ex = Assert.Throws<ArgumentException>(() => new Traceable<int>(10, ""));

        // Assert
        Assert.Contains("Name cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void ValueAsObject_ShouldReturnBoxedValue()
    {
        // Arrange
        var entity = new Traceable<int>(42, "Test");

        // Act
        var value = entity.ValueAsObject;

        // Assert
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
        // Arrange
        var entity = new Traceable<int>(value, "Test");

        // Act

        // Assert
        Assert.Equal(value, entity.Resolve());
    }

    [Theory]
    [InlineData(10.5)]
    [InlineData(0.0)]
    [InlineData(-5.75)]
    public void DecimalEntity_ShouldHandleVariousValues(double value)
    {
        // Arrange
        var entity = new Traceable<decimal>((decimal)value, "Test");

        // Act

        // Assert
        Assert.Equal((decimal)value, entity.Resolve());
    }

    [Fact]
    public void BooleanEntity_ShouldWork()
    {
        // Arrange
        var trueEntity = new Traceable<bool>(true, "IsTrue");
        var falseEntity = new Traceable<bool>(false, "IsFalse");

        // Act

        // Assert
        Assert.True(trueEntity.Resolve());
        Assert.False(falseEntity.Resolve());
    }

    [Fact]
    public void StringEntity_ShouldWork()
    {
        // Arrange
        var entity = new Traceable<string>("Hello", "Name");

        // Act

        // Assert
        Assert.Equal("Hello", entity.Resolve());
    }
}
