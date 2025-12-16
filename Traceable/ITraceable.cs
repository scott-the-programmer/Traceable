using System.Collections.Generic;

namespace Traceable;

/// <summary>Non-generic base interface for cross-type operations.</summary>
public interface ITraceableBase
{
    string Name { get; }
    string? Description { get; set; }
    object? ValueAsObject { get; }
    string Dependencies { get; }
    GraphNode BuildGraph();
    void PrintConsole();
    IReadOnlyDictionary<string, object?> ArbitraryState { get; }
}

/// <summary>A traceable entity that wraps a value and tracks computation history.</summary>
public interface ITraceable<T> : ITraceableBase
{
    T Value { get; }
    T Resolve();
    IEnumerable<string> GetDependencyNames();

    /// <summary>Reloads a base entity's value. Throws on composite entities.</summary>
    void Reload(T newValue);

    IReadOnlyDictionary<string, T> ValueState { get; }
    bool HasArbitraryState { get; }
    bool HasValueState { get; }
    bool TryGetArbitraryState(string key, out object? value);
    bool TryGetValueState(string key, out T value);
}
