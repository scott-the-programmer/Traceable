using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityTrace
{
    /// <summary>
    /// A traceable entity that wraps a value and tracks its computation history.
    /// Supports operator overloading for natural arithmetic syntax while maintaining dependency information.
    /// </summary>
    /// <typeparam name="T">The type of value being wrapped.</typeparam>
    public class Traceable<T> : ITraceable<T>
    {
        // Discriminator to determine if this is a base or composite entity
        private readonly bool _isBase;

        // Base entity state
        private T _baseValue = default!;

        // Composite entity state
        private readonly string? _operation;
        private readonly ITraceableBase[]? _operands;
        private readonly Func<T>? _computeFunc;

        // Shared state
        private readonly string _name;
        private string? _description;

        // Immutable state
        private readonly IReadOnlyDictionary<string, object> _arbitraryState;
        private readonly IReadOnlyDictionary<string, T> _valueState;

        // Shared empty dictionaries to avoid allocations
        private static readonly IReadOnlyDictionary<string, object> EmptyArbitraryState =
            new Dictionary<string, object>();
        private static readonly IReadOnlyDictionary<string, T> EmptyValueState =
            new Dictionary<string, T>();

        /// <summary>
        /// Creates a new base traceable entity with the specified name and value.
        /// </summary>
        /// <param name="name">The identifier for this entity.</param>
        /// <param name="value">The initial value.</param>
        /// <param name="arbitraryState">Optional arbitrary state dictionary.</param>
        /// <param name="valueState">Optional value state dictionary.</param>
        public Traceable(
            string name,
            T value,
            IReadOnlyDictionary<string, object>? arbitraryState = null,
            IReadOnlyDictionary<string, T>? valueState = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));

            _isBase = true;
            _name = name;
            _baseValue = value!;
            _arbitraryState = arbitraryState ?? EmptyArbitraryState;
            _valueState = valueState ?? EmptyValueState;
        }

        /// <summary>
        /// Internal constructor for creating composite entities from operations.
        /// </summary>
        internal Traceable(
            string operation,
            ITraceableBase[] operands,
            Func<T> computeFunc,
            IReadOnlyDictionary<string, object>? arbitraryState = null,
            IReadOnlyDictionary<string, T>? valueState = null)
        {
            _isBase = false;
            _operation = operation;
            _operands = operands;
            _computeFunc = computeFunc;
            _name = BuildName();
            _arbitraryState = arbitraryState ?? EmptyArbitraryState;
            _valueState = valueState ?? EmptyValueState;
        }

        /// <inheritdoc />
        public string Name => _name;

        /// <inheritdoc />
        public string? Description
        {
            get => _description;
            set => _description = value;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, object> ArbitraryState => _arbitraryState;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, T> ValueState => _valueState;

        /// <inheritdoc />
        public bool HasArbitraryState => _arbitraryState.Count > 0;

        /// <inheritdoc />
        public bool HasValueState => _valueState.Count > 0;

        /// <inheritdoc />
        public bool TryGetArbitraryState(string key, out object? value)
        {
            return _arbitraryState.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public bool TryGetValueState(string key, out T value)
        {
            return _valueState.TryGetValue(key, out value!);
        }

        /// <inheritdoc />
        public T Value => Resolve();

        /// <inheritdoc />
        public object? ValueAsObject => Resolve();

        /// <inheritdoc />
        public string Dependencies => BuildDependencies();

        /// <inheritdoc />
        public GraphNode BuildGraph()
        {
            var children = new List<GraphNode>();
            if (!_isBase && _operands != null)
            {
                foreach (var operand in _operands)
                {
                    children.Add(operand.BuildGraph());
                }
            }

            return new GraphNode(
                Name,
                Description,
                Resolve(),
                _isBase,
                _operation,
                children,
                HasArbitraryState ? _arbitraryState : null,
                HasValueState ? _valueState.ToDictionary(k => k.Key, v => (object?)v.Value) : null
            );
        }

        /// <inheritdoc />
        public void PrintConsole()
        {
            Console.WriteLine(RenderGraph("", true, true));
        }

        /// <inheritdoc />
        public T Resolve()
        {
            if (_isBase)
            {
                return _baseValue;
            }

            // Composite entity - compute the value
            return _computeFunc!();
        }

        /// <inheritdoc />
        public IEnumerable<string> GetDependencyNames()
        {
            if (_isBase)
            {
                return new[] { _name };
            }

            // Recursively collect all base entity names
            var dependencies = new List<string>();
            foreach (var operand in _operands!)
            {
                if (operand is ITraceable<T> traceable)
                {
                    dependencies.AddRange(traceable.GetDependencyNames());
                }
                else if (operand is ITraceableBase baseTraceable)
                {
                    // Handle cross-type operands (e.g., from comparison operators)
                    dependencies.Add(baseTraceable.Name);
                }
            }
            return dependencies.Distinct();
        }

        /// <inheritdoc />
        public void Reset(T newValue)
        {
            if (!_isBase)
            {
                throw new InvalidOperationException("Cannot reset a composite entity. Only base entities can be reset.");
            }

            _baseValue = newValue!;
        }

        /// <summary>
        /// Builds the name for a composite entity from its operands and operation.
        /// </summary>
        private string BuildName()
        {
            if (_isBase)
                return _name;

            if (_operands == null || _operands.Length == 0)
                return "Unknown";

            // Handle single-operand (transformations)
            if (_operands.Length == 1)
            {
                return $"{_operation}({_operands[0].Name})";
            }

            // Handle two-operand: distinguish between binary operators and transforms
            if (_operands.Length == 2)
            {
                // If it's a binary operator, use infix notation
                if (IsBinaryOperator(_operation!))
                {
                    return $"{_operands[0].Name} {_operation} {_operands[1].Name}";
                }
                else
                {
                    // It's a transform function with 2 inputs - use function notation
                    var operandsList = string.Join(", ", _operands.Select(o => o.Name));
                    return $"{_operation}({operandsList})";
                }
            }

            // Handle 3+ operands with function notation
            var operandsList2 = string.Join(", ", _operands.Select(o => o.Name));
            return $"{_operation}({operandsList2})";
        }

        /// <summary>
        /// Builds the dependency expression string.
        /// </summary>
        private string BuildDependencies(int parentPrecedence = int.MaxValue)
        {
            if (_isBase)
                return _name;

            int currentPrecedence = GetPrecedence(_operation!);

            // Handle single-operand (transformations)
            if (_operands!.Length == 1)
            {
                string operandStr = _operands[0].Dependencies;
                return $"{_operation}({operandStr})";
            }

            // Handle two-operand: distinguish between binary operators and transforms
            if (_operands.Length == 2)
            {
                // If it's a binary operator, use infix notation
                if (IsBinaryOperator(_operation!))
                {
                    string leftStr, rightStr;

                    if (_operands[0] is Traceable<T> leftTraceable)
                    {
                        leftStr = leftTraceable.BuildDependencies(currentPrecedence);
                    }
                    else
                    {
                        leftStr = _operands[0].Dependencies;
                    }

                    if (_operands[1] is Traceable<T> rightTraceable)
                    {
                        rightStr = rightTraceable.BuildDependencies(currentPrecedence);
                    }
                    else
                    {
                        rightStr = _operands[1].Dependencies;
                    }

                    string expr = $"{leftStr} {_operation} {rightStr}";

                    // Add parentheses if parent has higher precedence (but not at root level)
                    if (parentPrecedence != int.MaxValue && currentPrecedence < parentPrecedence)
                        return $"({expr})";

                    return expr;
                }
                else
                {
                    // It's a transform function with 2 inputs - use function notation
                    var operandsList = string.Join(", ", _operands.Select(o => o.Dependencies));
                    return $"{_operation}({operandsList})";
                }
            }

            // Handle 3+ operands with function notation
            var operandsList2 = string.Join(", ", _operands.Select(o => o.Dependencies));
            return $"{_operation}({operandsList2})";
        }

        /// <summary>
        /// Gets the precedence level for an operation.
        /// Higher values = higher precedence.
        /// </summary>
        private static int GetPrecedence(string operation)
        {
            return operation switch
            {
                "*" or "/" => 5,
                "+" or "-" => 4,
                ">" or "<" or ">=" or "<=" => 3,
                "==" or "!=" => 2,
                "&" => 1,
                "|" => 0,
                _ => int.MaxValue
            };
        }

        /// <summary>
        /// Checks if an operation is a binary operator (vs a transform function).
        /// </summary>
        private static bool IsBinaryOperator(string operation)
        {
            return operation switch
            {
                "+" or "-" or "*" or "/" or ">" or "<" or ">=" or "<=" or "==" or "!=" or "&" or "|" => true,
                _ => false
            };
        }

        /// <summary>
        /// Renders a tree-structured graph visualization as a string.
        /// </summary>
        private string RenderGraph(string prefix, bool isLast, bool isRoot)
        {
            // Current node line
            string connector = isRoot ? "" : (isLast ? "└── " : "├── ");
            string label = Description ?? Name;
            string value = Resolve()?.ToString() ?? "null";

            var sb = new StringBuilder();
            sb.AppendLine($"{prefix}{connector}{label} = {value}");

            // Add state visualization if present
            if (HasArbitraryState || HasValueState)
            {
                string statePrefix = prefix + (isRoot ? "" : (isLast ? "    " : "│   "));

                // Show arbitrary state
                if (HasArbitraryState)
                {
                    foreach (var kvp in _arbitraryState)
                    {
                        sb.AppendLine($"{statePrefix}  [arbitrary] {kvp.Key}: {kvp.Value?.ToString() ?? "null"}");
                    }
                }

                // Show value state
                if (HasValueState)
                {
                    foreach (var kvp in _valueState)
                    {
                        sb.AppendLine($"{statePrefix}  [value] {kvp.Key}: {kvp.Value?.ToString() ?? "null"}");
                    }
                }
            }

            // If composite, recurse for operands
            if (!_isBase && _operands != null)
            {
                string childPrefix = prefix + (isRoot ? "" : (isLast ? "    " : "│   "));

                for (int i = 0; i < _operands.Length; i++)
                {
                    bool isLastChild = (i == _operands.Length - 1);
                    var operand = _operands[i];

                    if (operand is Traceable<T> traceable)
                    {
                        sb.Append(traceable.RenderGraph(childPrefix, isLastChild, false));
                    }
                    else if (operand is ITraceableBase baseTraceable)
                    {
                        // Handle cross-type operands - render their graph recursively
                        string opGraph = RenderGraphNode(baseTraceable.BuildGraph(), "", true, true);
                        var lines = opGraph.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        for (int j = 0; j < lines.Length; j++)
                        {
                            if (j == 0)
                            {
                                // First line gets the connector
                                string opConnector = isLastChild ? "└── " : "├── ";
                                sb.AppendLine($"{childPrefix}{opConnector}{lines[j]}");
                            }
                            else
                            {
                                // Subsequent lines get proper indentation
                                string indent = isLastChild ? "    " : "│   ";
                                sb.AppendLine($"{childPrefix}{indent}{lines[j]}");
                            }
                        }
                    }
                }
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        /// <summary>
        /// Renders a GraphNode to a string representation (used for cross-type operands).
        /// </summary>
        private static string RenderGraphNode(GraphNode node, string prefix, bool isLast, bool isRoot)
        {
            string connector = isRoot ? "" : (isLast ? "└── " : "├── ");
            string label = node.Description ?? node.Name;
            string value = node.Value?.ToString() ?? "null";

            var sb = new StringBuilder();
            sb.AppendLine($"{prefix}{connector}{label} = {value}");

            // Add state visualization if present
            if (node.ArbitraryState != null || node.ValueState != null)
            {
                string statePrefix = prefix + (isRoot ? "" : (isLast ? "    " : "│   "));

                if (node.ArbitraryState != null)
                {
                    foreach (var kvp in node.ArbitraryState)
                    {
                        sb.AppendLine($"{statePrefix}  [arbitrary] {kvp.Key}: {kvp.Value?.ToString() ?? "null"}");
                    }
                }

                if (node.ValueState != null)
                {
                    foreach (var kvp in node.ValueState)
                    {
                        sb.AppendLine($"{statePrefix}  [value] {kvp.Key}: {kvp.Value?.ToString() ?? "null"}");
                    }
                }
            }

            // Recurse for children
            if (node.Children.Count > 0)
            {
                string childPrefix = prefix + (isRoot ? "" : (isLast ? "    " : "│   "));

                for (int i = 0; i < node.Children.Count; i++)
                {
                    bool isLastChild = (i == node.Children.Count - 1);
                    sb.Append(RenderGraphNode(node.Children[i], childPrefix, isLastChild, false));
                }
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        #region Arithmetic Operators

        private static Traceable<T> BinaryOp(Traceable<T> left, Traceable<T> right, string op,
            Func<bool> isSupported, Func<T, T, T> compute, string supportedTypes)
        {
            if (left is null) throw new ArgumentNullException(nameof(left));
            if (right is null) throw new ArgumentNullException(nameof(right));
            if (!isSupported())
                throw new InvalidOperationException(
                    $"Operator {op} not supported for type {typeof(T).Name}. Supported types: {supportedTypes}");

            return new Traceable<T>(op, new ITraceableBase[] { left, right }, () => compute(left.Resolve(), right.Resolve()));
        }

        public static Traceable<T> operator +(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "+", IsAdditionSupported, Add,
                $"int, decimal, double, string, or types implementing ITraceableAddable<{typeof(T).Name}>");

        public static Traceable<T> operator -(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "-", IsSubtractionSupported, Subtract,
                $"int, decimal, double, or types implementing ITraceableSubtractable<{typeof(T).Name}>");

        public static Traceable<T> operator *(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "*", IsMultiplicationSupported, Multiply,
                $"int, decimal, double, or types implementing ITraceableMultiplicable<{typeof(T).Name}>");

        public static Traceable<T> operator /(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "/", IsDivisionSupported, Divide,
                $"int, decimal, double, or types implementing ITraceableDividable<{typeof(T).Name}>");

        #endregion

        #region Comparison Operators

        private static Traceable<bool> CompareOp(Traceable<T> left, Traceable<T> right, string op, Func<int, bool> predicate)
        {
            if (left is null) throw new ArgumentNullException(nameof(left));
            if (right is null) throw new ArgumentNullException(nameof(right));
            if (!typeof(IComparable).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException($"Type {typeof(T).Name} does not implement IComparable");

            return new Traceable<bool>(op, new ITraceableBase[] { left, right },
                () => predicate(Compare(left.Resolve(), right.Resolve())));
        }

        public static Traceable<bool> operator >(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, ">", c => c > 0);
        public static Traceable<bool> operator <(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, "<", c => c < 0);
        public static Traceable<bool> operator >=(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, ">=", c => c >= 0);
        public static Traceable<bool> operator <=(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, "<=", c => c <= 0);

        public static Traceable<bool> operator ==(Traceable<T> left, Traceable<T> right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return new Traceable<bool>("true", true);
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return new Traceable<bool>("false", false);
            return new Traceable<bool>("==", new ITraceableBase[] { left, right },
                () => EqualityComparer<T>.Default.Equals(left.Resolve(), right.Resolve()));
        }

        public static Traceable<bool> operator !=(Traceable<T> left, Traceable<T> right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return new Traceable<bool>("false", false);
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return new Traceable<bool>("true", true);
            return new Traceable<bool>("!=", new ITraceableBase[] { left, right },
                () => !EqualityComparer<T>.Default.Equals(left.Resolve(), right.Resolve()));
        }

        public static Traceable<T> operator &(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "&", IsLogicalSupported, And,
                $"bool, or types implementing ITraceableLogical<{typeof(T).Name}>");

        public static Traceable<T> operator |(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "|", IsLogicalSupported, Or,
                $"bool, or types implementing ITraceableLogical<{typeof(T).Name}>");

        #endregion

        #region Operator Implementation Helpers

        private static bool IsNumericType() =>
            typeof(T) == typeof(int) || typeof(T) == typeof(decimal) || typeof(T) == typeof(double);

        private static bool IsAdditionSupported() =>
            IsNumericType() || typeof(T) == typeof(string) || typeof(ITraceableAddable<T>).IsAssignableFrom(typeof(T));

        private static bool IsSubtractionSupported() =>
            IsNumericType() || typeof(ITraceableSubtractable<T>).IsAssignableFrom(typeof(T));

        private static bool IsMultiplicationSupported() =>
            IsNumericType() || typeof(ITraceableMultiplicable<T>).IsAssignableFrom(typeof(T));

        private static bool IsDivisionSupported() =>
            IsNumericType() || typeof(ITraceableDividable<T>).IsAssignableFrom(typeof(T));

        private static bool IsLogicalSupported() =>
            typeof(T) == typeof(bool) || typeof(ITraceableLogical<T>).IsAssignableFrom(typeof(T));

        private static T NumericOp(T left, T right,
            Func<int, int, int> intOp, Func<decimal, decimal, decimal> decOp, Func<double, double, double> dblOp)
        {
            if (typeof(T) == typeof(int)) return (T)(object)intOp((int)(object)left!, (int)(object)right!);
            if (typeof(T) == typeof(decimal)) return (T)(object)decOp((decimal)(object)left!, (decimal)(object)right!);
            if (typeof(T) == typeof(double)) return (T)(object)dblOp((double)(object)left!, (double)(object)right!);
            return default!;
        }

        private static T Add(T left, T right)
        {
            if (IsNumericType()) return NumericOp(left, right, (l, r) => l + r, (l, r) => l + r, (l, r) => l + r);
            if (typeof(T) == typeof(string)) return (T)(object)((string)(object)left! + (string)(object)right!);
            if (left is ITraceableAddable<T> addable) return addable.Add(left, right);
            throw new InvalidOperationException(
                $"ADD operation not supported for type {typeof(T).Name}. " +
                $"Type must be int, decimal, double, string, or implement ITraceableAddable<{typeof(T).Name}>");
        }

        private static T Subtract(T left, T right)
        {
            if (IsNumericType()) return NumericOp(left, right, (l, r) => l - r, (l, r) => l - r, (l, r) => l - r);
            if (left is ITraceableSubtractable<T> subtractable) return subtractable.Subtract(left, right);
            throw new InvalidOperationException(
                $"SUBTRACT operation not supported for type {typeof(T).Name}. " +
                $"Type must be int, decimal, double, or implement ITraceableSubtractable<{typeof(T).Name}>");
        }

        private static T Multiply(T left, T right)
        {
            if (IsNumericType()) return NumericOp(left, right, (l, r) => l * r, (l, r) => l * r, (l, r) => l * r);
            if (left is ITraceableMultiplicable<T> multiplicable) return multiplicable.Multiply(left, right);
            throw new InvalidOperationException(
                $"MULTIPLY operation not supported for type {typeof(T).Name}. " +
                $"Type must be int, decimal, double, or implement ITraceableMultiplicable<{typeof(T).Name}>");
        }

        private static T Divide(T left, T right)
        {
            if (IsNumericType()) return NumericOp(left, right, (l, r) => l / r, (l, r) => l / r, (l, r) => l / r);
            if (left is ITraceableDividable<T> dividable) return dividable.Divide(left, right);
            throw new InvalidOperationException(
                $"DIVIDE operation not supported for type {typeof(T).Name}. " +
                $"Type must be int, decimal, double, or implement ITraceableDividable<{typeof(T).Name}>");
        }

        private static int Compare(T left, T right) =>
            left is IComparable comparable ? comparable.CompareTo(right)
                : throw new InvalidOperationException($"Type {typeof(T).Name} does not implement IComparable");

        private static T And(T left, T right)
        {
            if (typeof(T) == typeof(bool)) return (T)(object)((bool)(object)left! && (bool)(object)right!);
            if (left is ITraceableLogical<T> logical) return logical.And(left, right);
            throw new InvalidOperationException(
                $"AND operation not supported for type {typeof(T).Name}. " +
                $"Type must be bool or implement ITraceableLogical<{typeof(T).Name}>");
        }

        private static T Or(T left, T right)
        {
            if (typeof(T) == typeof(bool)) return (T)(object)((bool)(object)left! || (bool)(object)right!);
            if (left is ITraceableLogical<T> logical) return logical.Or(left, right);
            throw new InvalidOperationException(
                $"OR operation not supported for type {typeof(T).Name}. " +
                $"Type must be bool or implement ITraceableLogical<{typeof(T).Name}>");
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current traceable entity.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is Traceable<T> other)
            {
                return EqualityComparer<T>.Default.Equals(Resolve(), other.Resolve());
            }
            return false;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        public override int GetHashCode()
        {
            return Resolve()?.GetHashCode() ?? 0;
        }

        #endregion
    }
}
