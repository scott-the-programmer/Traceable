using System.Collections.Generic;

namespace EntityTrace
{
    /// <summary>
    /// Non-generic base interface for traceable entities.
    /// Provides access to common properties across different generic types.
    /// </summary>
    public interface ITraceableBase
    {
        /// <summary>
        /// Gets the name/identifier of the entity.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets an optional human-readable description of the entity.
        /// </summary>
        string? Description { get; set; }

        /// <summary>
        /// Gets the wrapped value as an object.
        /// </summary>
        object? ValueAsObject { get; }

        /// <summary>
        /// Gets the dependency expression showing how this value is computed.
        /// For base entities, returns the name. For composite entities, returns the full expression.
        /// </summary>
        string Dependencies { get; }

        /// <summary>
        /// Builds and returns the computation dependency graph as a data structure.
        /// </summary>
        GraphNode BuildGraph();

        /// <summary>
        /// Prints the computation dependency graph to the console.
        /// </summary>
        void PrintConsole();

        /// <summary>
        /// Gets the arbitrary state attached to this entity.
        /// </summary>
        IReadOnlyDictionary<string, object> ArbitraryState { get; }
    }

    /// <summary>
    /// Generic interface for traceable entities that wrap values and track their computation history.
    /// </summary>
    /// <typeparam name="T">The type of value being wrapped.</typeparam>
    public interface ITraceable<T> : ITraceableBase
    {
        /// <summary>
        /// Gets the wrapped value.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Resolves and computes the final value by evaluating all dependencies.
        /// </summary>
        /// <returns>The computed value.</returns>
        T Resolve();

        /// <summary>
        /// Gets all base entity names that this entity depends on.
        /// </summary>
        /// <returns>A collection of unique base entity names.</returns>
        IEnumerable<string> GetDependencyNames();

        /// <summary>
        /// Resets the value of a base entity. Can only be called on base entities.
        /// </summary>
        /// <param name="newValue">The new value to set.</param>
        /// <exception cref="System.InvalidOperationException">Thrown when called on a composite entity.</exception>
        void Reset(T newValue);

        /// <summary>
        /// Gets the value-typed state attached to this entity.
        /// </summary>
        IReadOnlyDictionary<string, T> ValueState { get; }

        /// <summary>
        /// Checks if this entity has any arbitrary state.
        /// </summary>
        bool HasArbitraryState { get; }

        /// <summary>
        /// Checks if this entity has any value state.
        /// </summary>
        bool HasValueState { get; }

        /// <summary>
        /// Tries to get an arbitrary state value by key.
        /// </summary>
        bool TryGetArbitraryState(string key, out object? value);

        /// <summary>
        /// Tries to get a value state entry by key.
        /// </summary>
        bool TryGetValueState(string key, out T value);
    }
}
