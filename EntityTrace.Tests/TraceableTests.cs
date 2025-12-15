using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EntityTrace.Tests
{
    public class TraceableTests
    {
        [Fact]
        public void Constructor_ShouldCreateBaseEntity()
        {
            // Arrange & Act
            var entity = new Traceable<int>("TestEntity", 42);

            // Assert
            Assert.Equal("TestEntity", entity.Name);
            Assert.Equal(42, entity.Value);
            Assert.Equal(42, entity.Resolve());
        }

        [Fact]
        public void Description_ShouldBeSettable()
        {
            // Arrange
            var entity = new Traceable<int>("Test", 10);

            // Act
            entity.Description = "A test entity";

            // Assert
            Assert.Equal("A test entity", entity.Description);
        }

        [Fact]
        public void Dependencies_ForBaseEntity_ShouldReturnName()
        {
            // Arrange & Act
            var entity = new Traceable<int>("MyEntity", 100);

            // Assert
            Assert.Equal("MyEntity", entity.Dependencies);
        }

        [Fact]
        public void GetDependencyNames_ForBaseEntity_ShouldReturnSingleName()
        {
            // Arrange & Act
            var entity = new Traceable<int>("BaseEntity", 5);

            // Assert
            Assert.Equal(new[] { "BaseEntity" }, entity.GetDependencyNames());
        }

        [Fact]
        public void Reset_ShouldUpdateValue()
        {
            // Arrange
            var entity = new Traceable<int>("Counter", 10);

            // Act
            entity.Reset(20);

            // Assert
            Assert.Equal(20, entity.Resolve());
            Assert.Equal(20, entity.Value);
        }

        [Fact]
        public void BuildGraph_ForBaseEntity_ShouldShowNameAndValue()
        {
            // Arrange
            var entity = new Traceable<int>("MyValue", 42);
            entity.Description = "Test Value";

            // Act
            var graph = entity.BuildGraph();

            // Assert
            Assert.Equal("MyValue", graph.Name);
            Assert.Equal("Test Value", graph.Description);
            Assert.Equal(42, graph.Value);
            Assert.True(graph.IsBase);
            Assert.Empty(graph.Children);
        }

        [Fact]
        public void BuildGraph_ForBaseEntityWithoutDescription_ShouldShowNameAndValue()
        {
            // Arrange
            var entity = new Traceable<int>("MyValue", 42);

            // Act
            var graph = entity.BuildGraph();

            // Assert
            Assert.Equal("MyValue", graph.Name);
            Assert.Null(graph.Description);
            Assert.Equal(42, graph.Value);
        }

        [Fact]
        public void Constructor_WithNullName_ShouldThrow()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new Traceable<int>(null!, 10));
            Assert.Contains("Name cannot be null or whitespace", ex.Message);
        }

        [Fact]
        public void Constructor_WithEmptyName_ShouldThrow()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new Traceable<int>("", 10));
            Assert.Contains("Name cannot be null or whitespace", ex.Message);
        }

        [Fact]
        public void ValueAsObject_ShouldReturnBoxedValue()
        {
            // Arrange
            var entity = new Traceable<int>("Test", 42);

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
            // Arrange & Act
            var entity = new Traceable<int>("Test", value);

            // Assert
            Assert.Equal(value, entity.Resolve());
        }

        [Theory]
        [InlineData(10.5)]
        [InlineData(0.0)]
        [InlineData(-5.75)]
        public void DecimalEntity_ShouldHandleVariousValues(double value)
        {
            // Arrange & Act
            var entity = new Traceable<decimal>("Test", (decimal)value);

            // Assert
            Assert.Equal((decimal)value, entity.Resolve());
        }

        [Fact]
        public void BooleanEntity_ShouldWork()
        {
            // Arrange & Act
            var trueEntity = new Traceable<bool>("IsTrue", true);
            var falseEntity = new Traceable<bool>("IsFalse", false);

            // Assert
            Assert.True(trueEntity.Resolve());
            Assert.False(falseEntity.Resolve());
        }

        [Fact]
        public void StringEntity_ShouldWork()
        {
            // Arrange & Act
            var entity = new Traceable<string>("Name", "Hello");

            // Assert
            Assert.Equal("Hello", entity.Resolve());
        }
    }

    public class TraceableStateTests
    {
        [Fact]
        public void BaseEntity_WithArbitraryState_StoresAndRetrievesCorrectly()
        {
            // Arrange
            var arbitraryState = new Dictionary<string, object>
            {
                ["source"] = "database",
                ["confidence"] = 0.95
            };

            // Act
            var entity = new Traceable<int>("Test", 42, arbitraryState: arbitraryState);

            // Assert
            Assert.True(entity.HasArbitraryState);
            Assert.Equal(2, entity.ArbitraryState.Count);
            Assert.Equal("database", entity.ArbitraryState["source"]);
            Assert.Equal(0.95, entity.ArbitraryState["confidence"]);
        }

        [Fact]
        public void BaseEntity_WithValueState_StoresAndRetrievesCorrectly()
        {
            // Arrange
            var valueState = new Dictionary<string, decimal>
            {
                ["target"] = 100m,
                ["min"] = 50m,
                ["max"] = 200m
            };

            // Act
            var entity = new Traceable<decimal>("Test", 75m, valueState: valueState);

            // Assert
            Assert.True(entity.HasValueState);
            Assert.Equal(3, entity.ValueState.Count);
            Assert.Equal(100m, entity.ValueState["target"]);
            Assert.Equal(50m, entity.ValueState["min"]);
            Assert.Equal(200m, entity.ValueState["max"]);
        }

        [Fact]
        public void BaseEntity_WithBothStates_StoresAndRetrievesCorrectly()
        {
            // Arrange
            var arbitraryState = new Dictionary<string, object>
            {
                ["currency"] = "USD",
                ["precision"] = 2
            };
            var valueState = new Dictionary<string, decimal>
            {
                ["threshold"] = 100m
            };

            // Act
            var entity = new Traceable<decimal>("Revenue", 150m, arbitraryState, valueState);

            // Assert
            Assert.True(entity.HasArbitraryState);
            Assert.True(entity.HasValueState);
            Assert.Equal("USD", entity.ArbitraryState["currency"]);
            Assert.Equal(2, entity.ArbitraryState["precision"]);
            Assert.Equal(100m, entity.ValueState["threshold"]);
        }

        [Fact]
        public void BaseEntity_WithoutState_ReturnsEmptyDictionaries()
        {
            // Arrange & Act
            var entity = new Traceable<int>("Test", 42);

            // Assert
            Assert.False(entity.HasArbitraryState);
            Assert.False(entity.HasValueState);
            Assert.Empty(entity.ArbitraryState);
            Assert.Empty(entity.ValueState);
        }

        [Fact]
        public void CompositeEntity_WithState_HasIndependentState()
        {
            // Arrange
            var arbitraryState1 = new Dictionary<string, object> { ["tag"] = "A" };
            var arbitraryState2 = new Dictionary<string, object> { ["tag"] = "B" };

            var entity1 = new Traceable<int>("A", 10, arbitraryState: arbitraryState1);
            var entity2 = new Traceable<int>("B", 20, arbitraryState: arbitraryState2);

            // Act
            var composite = entity1 + entity2;

            // Assert - composite has no state (independent)
            Assert.False(composite.HasArbitraryState);
            Assert.False(composite.HasValueState);

            // But operands still have their own state
            Assert.True(entity1.HasArbitraryState);
            Assert.True(entity2.HasArbitraryState);
            Assert.Equal("A", entity1.ArbitraryState["tag"]);
            Assert.Equal("B", entity2.ArbitraryState["tag"]);
        }

        [Fact]
        public void ArbitraryState_IsReadOnly_ExposesIReadOnlyDictionary()
        {
            // Arrange
            var arbitraryState = new Dictionary<string, object> { ["key"] = "value" };
            var entity = new Traceable<int>("Test", 42, arbitraryState: arbitraryState);

            // Act & Assert - Verify it's exposed as IReadOnlyDictionary
            Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(entity.ArbitraryState);

            // Verify the property type is IReadOnlyDictionary (prevents modification via public API)
            var property = typeof(Traceable<int>).GetProperty(nameof(entity.ArbitraryState));
            Assert.Equal(typeof(IReadOnlyDictionary<string, object>), property!.PropertyType);
        }

        [Fact]
        public void ValueState_IsReadOnly_ExposesIReadOnlyDictionary()
        {
            // Arrange
            var valueState = new Dictionary<string, int> { ["key"] = 100 };
            var entity = new Traceable<int>("Test", 42, valueState: valueState);

            // Act & Assert - Verify it's exposed as IReadOnlyDictionary
            Assert.IsAssignableFrom<IReadOnlyDictionary<string, int>>(entity.ValueState);

            // Verify the property type is IReadOnlyDictionary (prevents modification via public API)
            var property = typeof(Traceable<int>).GetProperty(nameof(entity.ValueState));
            Assert.Equal(typeof(IReadOnlyDictionary<string, int>), property!.PropertyType);
        }

        [Fact]
        public void BuildGraph_IncludesArbitraryState_InVisualization()
        {
            // Arrange
            var arbitraryState = new Dictionary<string, object>
            {
                ["currency"] = "USD",
                ["precision"] = 2
            };
            var entity = new Traceable<decimal>("Revenue", 10000m, arbitraryState: arbitraryState);

            // Act
            var graph = entity.BuildGraph();

            // Assert
            Assert.NotNull(graph.ArbitraryState);
            Assert.Equal("USD", graph.ArbitraryState["currency"]);
            Assert.Equal(2, graph.ArbitraryState["precision"]);
        }

        [Fact]
        public void BuildGraph_IncludesValueState_InVisualization()
        {
            // Arrange
            var valueState = new Dictionary<string, decimal>
            {
                ["threshold"] = 100m,
                ["target"] = 200m
            };
            var entity = new Traceable<decimal>("Sales", 150m, valueState: valueState);

            // Act
            var graph = entity.BuildGraph();

            // Assert
            Assert.NotNull(graph.ValueState);
            Assert.Equal(100m, graph.ValueState["threshold"]);
            Assert.Equal(200m, graph.ValueState["target"]);
        }

        [Fact]
        public void TryGetArbitraryState_WithExistingKey_ReturnsTrue()
        {
            // Arrange
            var arbitraryState = new Dictionary<string, object> { ["key1"] = "value1" };
            var entity = new Traceable<int>("Test", 42, arbitraryState: arbitraryState);

            // Act
            var success = entity.TryGetArbitraryState("key1", out var value);

            // Assert
            Assert.True(success);
            Assert.Equal("value1", value);
        }

        [Fact]
        public void TryGetArbitraryState_WithMissingKey_ReturnsFalse()
        {
            // Arrange
            var arbitraryState = new Dictionary<string, object> { ["key1"] = "value1" };
            var entity = new Traceable<int>("Test", 42, arbitraryState: arbitraryState);

            // Act
            var success = entity.TryGetArbitraryState("missing", out var value);

            // Assert
            Assert.False(success);
            Assert.Null(value);
        }

        [Fact]
        public void TryGetValueState_WithExistingKey_ReturnsTrue()
        {
            // Arrange
            var valueState = new Dictionary<string, int> { ["threshold"] = 100 };
            var entity = new Traceable<int>("Test", 42, valueState: valueState);

            // Act
            var success = entity.TryGetValueState("threshold", out var value);

            // Assert
            Assert.True(success);
            Assert.Equal(100, value);
        }

        [Fact]
        public void TryGetValueState_WithMissingKey_ReturnsFalse()
        {
            // Arrange
            var valueState = new Dictionary<string, int> { ["threshold"] = 100 };
            var entity = new Traceable<int>("Test", 42, valueState: valueState);

            // Act
            var success = entity.TryGetValueState("missing", out var value);

            // Assert
            Assert.False(success);
            Assert.Equal(default(int), value);
        }

        [Fact]
        public void HasArbitraryState_WithState_ReturnsTrue()
        {
            // Arrange
            var arbitraryState = new Dictionary<string, object> { ["key"] = "value" };
            var entity = new Traceable<int>("Test", 42, arbitraryState: arbitraryState);

            // Act & Assert
            Assert.True(entity.HasArbitraryState);
        }

        [Fact]
        public void HasValueState_WithState_ReturnsTrue()
        {
            // Arrange
            var valueState = new Dictionary<string, int> { ["key"] = 100 };
            var entity = new Traceable<int>("Test", 42, valueState: valueState);

            // Act & Assert
            Assert.True(entity.HasValueState);
        }

        [Fact]
        public void BuildGraph_WithBothStates_ShowsInCorrectFormat()
        {
            // Arrange
            var base1 = new Traceable<decimal>("Base", 100m, arbitraryState: new Dictionary<string, object> { ["source"] = "db" });
            var base2 = new Traceable<decimal>("Tax", 50m, valueState: new Dictionary<string, decimal> { ["rate"] = 0.5m });

            var total = base1 + base2;
            total.Description = "Total";

            // Act
            var graph = total.BuildGraph();

            // Assert
            Assert.Equal("Total", graph.Description);
            Assert.Equal(150m, graph.Value);
            Assert.Equal(2, graph.Children.Count);

            // Check base1 state
            var child1 = graph.Children[0];
            Assert.NotNull(child1.ArbitraryState);
            Assert.Equal("db", child1.ArbitraryState["source"]);

            // Check base2 state
            var child2 = graph.Children[1];
            Assert.NotNull(child2.ValueState);
            Assert.Equal(0.5m, child2.ValueState["rate"]);
        }
    }
}
