using System.Collections.Generic;

namespace EntityTrace
{
    public class GraphNode
    {
        public GraphNode(
            string name,
            string? description,
            object? value,
            bool isBase,
            string? operation,
            IReadOnlyList<GraphNode> children,
            IReadOnlyDictionary<string, object?>? arbitraryState = null,
            IReadOnlyDictionary<string, object?>? valueState = null)
        {
            Name = name;
            Description = description;
            Value = value;
            IsBase = isBase;
            Operation = operation;
            Children = children;
            ArbitraryState = arbitraryState;
            ValueState = valueState;
        }

        public string Name { get; }
        public string? Description { get; }
        public object? Value { get; }
        public bool IsBase { get; }
        public string? Operation { get; }
        public IReadOnlyList<GraphNode> Children { get; }
        public IReadOnlyDictionary<string, object?>? ArbitraryState { get; }
        public IReadOnlyDictionary<string, object?>? ValueState { get; }
    }
}
