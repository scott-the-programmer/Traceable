namespace EntityTrace.Tests;

public class TraceableStateTests
{
    [Fact]
    public void BaseEntity_WithArbitraryState_StoresAndRetrievesCorrectly()
    {
        var arbitraryState = new Dictionary<string, object>
        {
            ["source"] = "database",
            ["confidence"] = 0.95
        };

        var entity = new Traceable<int>("Test", 42, arbitraryState: arbitraryState);

        Assert.True(entity.HasArbitraryState);
        Assert.Equal(2, entity.ArbitraryState.Count);
        Assert.Equal("database", entity.ArbitraryState["source"]);
        Assert.Equal(0.95, entity.ArbitraryState["confidence"]);
    }

    [Fact]
    public void BaseEntity_WithValueState_StoresAndRetrievesCorrectly()
    {
        var valueState = new Dictionary<string, decimal>
        {
            ["target"] = 100m,
            ["min"] = 50m,
            ["max"] = 200m
        };

        var entity = new Traceable<decimal>("Test", 75m, valueState: valueState);

        Assert.True(entity.HasValueState);
        Assert.Equal(3, entity.ValueState.Count);
        Assert.Equal(100m, entity.ValueState["target"]);
        Assert.Equal(50m, entity.ValueState["min"]);
        Assert.Equal(200m, entity.ValueState["max"]);
    }

    [Fact]
    public void BaseEntity_WithBothStates_StoresAndRetrievesCorrectly()
    {
        var arbitraryState = new Dictionary<string, object>
        {
            ["currency"] = "USD",
            ["precision"] = 2
        };
        var valueState = new Dictionary<string, decimal>
        {
            ["threshold"] = 100m
        };

        var entity = new Traceable<decimal>("Revenue", 150m, arbitraryState, valueState);

        Assert.True(entity.HasArbitraryState);
        Assert.True(entity.HasValueState);
        Assert.Equal("USD", entity.ArbitraryState["currency"]);
        Assert.Equal(2, entity.ArbitraryState["precision"]);
        Assert.Equal(100m, entity.ValueState["threshold"]);
    }

    [Fact]
    public void BaseEntity_WithoutState_ReturnsEmptyDictionaries()
    {
        var entity = new Traceable<int>("Test", 42);

        Assert.False(entity.HasArbitraryState);
        Assert.False(entity.HasValueState);
        Assert.Empty(entity.ArbitraryState);
        Assert.Empty(entity.ValueState);
    }

    [Fact]
    public void CompositeEntity_WithState_HasIndependentState()
    {
        var arbitraryState1 = new Dictionary<string, object> { ["tag"] = "A" };
        var arbitraryState2 = new Dictionary<string, object> { ["tag"] = "B" };

        var entity1 = new Traceable<int>("A", 10, arbitraryState: arbitraryState1);
        var entity2 = new Traceable<int>("B", 20, arbitraryState: arbitraryState2);

        var composite = entity1 + entity2;

        Assert.False(composite.HasArbitraryState);
        Assert.False(composite.HasValueState);

        Assert.True(entity1.HasArbitraryState);
        Assert.True(entity2.HasArbitraryState);
        Assert.Equal("A", entity1.ArbitraryState["tag"]);
        Assert.Equal("B", entity2.ArbitraryState["tag"]);
    }

    [Fact]
    public void ArbitraryState_IsReadOnly_ExposesIReadOnlyDictionary()
    {
        var arbitraryState = new Dictionary<string, object> { ["key"] = "value" };
        var entity = new Traceable<int>("Test", 42, arbitraryState: arbitraryState);

        Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(entity.ArbitraryState);

        var property = typeof(Traceable<int>).GetProperty(nameof(entity.ArbitraryState));
        Assert.Equal(typeof(IReadOnlyDictionary<string, object>), property!.PropertyType);
    }

    [Fact]
    public void ValueState_IsReadOnly_ExposesIReadOnlyDictionary()
    {
        var valueState = new Dictionary<string, int> { ["key"] = 100 };
        var entity = new Traceable<int>("Test", 42, valueState: valueState);

        Assert.IsAssignableFrom<IReadOnlyDictionary<string, int>>(entity.ValueState);

        var property = typeof(Traceable<int>).GetProperty(nameof(entity.ValueState));
        Assert.Equal(typeof(IReadOnlyDictionary<string, int>), property!.PropertyType);
    }

    [Fact]
    public void BuildGraph_IncludesArbitraryState_InVisualization()
    {
        var arbitraryState = new Dictionary<string, object>
        {
            ["currency"] = "USD",
            ["precision"] = 2
        };
        var entity = new Traceable<decimal>("Revenue", 10000m, arbitraryState: arbitraryState);

        var graph = entity.BuildGraph();

        Assert.NotNull(graph.ArbitraryState);
        Assert.Equal("USD", graph.ArbitraryState["currency"]);
        Assert.Equal(2, graph.ArbitraryState["precision"]);
    }

    [Fact]
    public void BuildGraph_IncludesValueState_InVisualization()
    {
        var valueState = new Dictionary<string, decimal>
        {
            ["threshold"] = 100m,
            ["target"] = 200m
        };
        var entity = new Traceable<decimal>("Sales", 150m, valueState: valueState);

        var graph = entity.BuildGraph();

        Assert.NotNull(graph.ValueState);
        Assert.Equal(100m, graph.ValueState["threshold"]);
        Assert.Equal(200m, graph.ValueState["target"]);
    }

    [Fact]
    public void TryGetArbitraryState_WithExistingKey_ReturnsTrue()
    {
        var arbitraryState = new Dictionary<string, object> { ["key1"] = "value1" };
        var entity = new Traceable<int>("Test", 42, arbitraryState: arbitraryState);

        var success = entity.TryGetArbitraryState("key1", out var value);

        Assert.True(success);
        Assert.Equal("value1", value);
    }

    [Fact]
    public void TryGetArbitraryState_WithMissingKey_ReturnsFalse()
    {
        var arbitraryState = new Dictionary<string, object> { ["key1"] = "value1" };
        var entity = new Traceable<int>("Test", 42, arbitraryState: arbitraryState);

        var success = entity.TryGetArbitraryState("missing", out var value);

        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValueState_WithExistingKey_ReturnsTrue()
    {
        var valueState = new Dictionary<string, int> { ["threshold"] = 100 };
        var entity = new Traceable<int>("Test", 42, valueState: valueState);

        var success = entity.TryGetValueState("threshold", out var value);

        Assert.True(success);
        Assert.Equal(100, value);
    }

    [Fact]
    public void TryGetValueState_WithMissingKey_ReturnsFalse()
    {
        var valueState = new Dictionary<string, int> { ["threshold"] = 100 };
        var entity = new Traceable<int>("Test", 42, valueState: valueState);

        var success = entity.TryGetValueState("missing", out var value);

        Assert.False(success);
        Assert.Equal(default(int), value);
    }

    [Fact]
    public void HasArbitraryState_WithState_ReturnsTrue()
    {
        var arbitraryState = new Dictionary<string, object> { ["key"] = "value" };
        var entity = new Traceable<int>("Test", 42, arbitraryState: arbitraryState);

        Assert.True(entity.HasArbitraryState);
    }

    [Fact]
    public void HasValueState_WithState_ReturnsTrue()
    {
        var valueState = new Dictionary<string, int> { ["key"] = 100 };
        var entity = new Traceable<int>("Test", 42, valueState: valueState);

        Assert.True(entity.HasValueState);
    }

    [Fact]
    public void BuildGraph_WithBothStates_ShowsInCorrectFormat()
    {
        var base1 = new Traceable<decimal>("Base", 100m, arbitraryState: new Dictionary<string, object> { ["source"] = "db" });
        var base2 = new Traceable<decimal>("Tax", 50m, valueState: new Dictionary<string, decimal> { ["rate"] = 0.5m });

        var total = base1 + base2;
        total.Description = "Total";

        var graph = total.BuildGraph();

        Assert.Equal("Total", graph.Description);
        Assert.Equal(150m, graph.Value);
        Assert.Equal(2, graph.Children.Count);

        var child1 = graph.Children[0];
        Assert.NotNull(child1.ArbitraryState);
        Assert.Equal("db", child1.ArbitraryState["source"]);

        var child2 = graph.Children[1];
        Assert.NotNull(child2.ValueState);
        Assert.Equal(0.5m, child2.ValueState["rate"]);
    }
}
