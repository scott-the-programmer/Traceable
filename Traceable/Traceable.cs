using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Traceable
{
    /// <summary>
    /// A traceable entity that wraps a value of type <typeparamref name="T"/> and automatically
    /// tracks its computation history and dependencies.
    /// </summary>
    /// <typeparam name="T">The type of value being tracked.</typeparam>
    /// <remarks>
    /// <para>
    /// Traceable entities come in two forms:
    /// </para>
    /// <list type="bullet">
    /// <item><description><b>Base entities</b>: Created via the public constructor with a mutable value that can be updated via <see cref="Reset"/>.</description></item>
    /// <item><description><b>Composite entities</b>: Created automatically via operators or <see cref="TraceableExtensions.Transform{TInput,TOutput}"/>, holding references to their operands.</description></item>
    /// </list>
    /// <para>
    /// Values are computed lazily via <see cref="Resolve"/>. When a base entity is reset, all dependent
    /// composite entities automatically reflect the change on their next resolution.
    /// </para>
    /// </remarks>
    /// <example>
    /// Basic usage:
    /// <code>
    /// var price = new Traceable&lt;decimal&gt;("Price", 100m);
    /// var quantity = new Traceable&lt;decimal&gt;("Quantity", 5m);
    /// var total = price * quantity;  // Composite entity
    ///
    /// Console.WriteLine(total.Value);        // 500
    /// Console.WriteLine(total.Dependencies); // "Price * Quantity"
    ///
    /// price.Reset(120m);
    /// Console.WriteLine(total.Value);        // 600 (automatically updated)
    /// </code>
    /// </example>
    public class Traceable<T> : ITraceable<T>
    {
        private readonly bool _isBase;
        private T _baseValue = default!;
        private readonly string? _operation;
        private readonly ITraceableBase[]? _operands;
        private readonly Func<T>? _computeFunc;
        private readonly string _name;
        private string? _description;
        private readonly IReadOnlyDictionary<string, object?> _arbitraryState;
        private readonly IReadOnlyDictionary<string, T> _valueState;

        private static readonly IReadOnlyDictionary<string, object?> EmptyArbitraryState = new Dictionary<string, object?>();
        private static readonly IReadOnlyDictionary<string, T> EmptyValueState = new Dictionary<string, T>();

        /// <summary>
        /// Creates a new base traceable entity with a mutable value.
        /// </summary>
        /// <param name="value">The initial value of the entity.</param>
        /// <param name="name">
        /// A unique identifier for this entity. Used in dependency expressions and graph visualization.
        /// Cannot be null or whitespace.
        /// </param>
        /// <param name="arbitraryState">
        /// Optional metadata dictionary for storing arbitrary key-value pairs.
        /// Useful for attaching context like units, sources, or validation rules.
        /// </param>
        /// <param name="valueState">
        /// Optional type-safe metadata dictionary where values are of the same type as the entity.
        /// Useful for storing related values like targets, bounds, or thresholds.
        /// </param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
        /// <example>
        /// <code>
        /// // Simple creation
        /// var price = new Traceable&lt;decimal&gt;(99.99m, "Price");
        ///
        /// // With metadata
        /// var temperature = new Traceable&lt;double&gt;(72.5, "Temperature",
        ///     arbitraryState: new Dictionary&lt;string, object?&gt; { ["unit"] = "Fahrenheit" },
        ///     valueState: new Dictionary&lt;string, double&gt; { ["min"] = 60.0, ["max"] = 80.0 });
        /// </code>
        /// </example>
        public Traceable(
            T value,
            string name,
            IReadOnlyDictionary<string, object?>? arbitraryState = null,
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

        internal Traceable(
            string operation,
            ITraceableBase[] operands,
            Func<T> computeFunc,
            IReadOnlyDictionary<string, object?>? arbitraryState = null,
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

        /// <summary>
        /// Gets the identifier for this entity. For base entities, this is the name provided at construction.
        /// For composite entities, this is auto-generated from the operation and operand names.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets or sets an optional description for display purposes.
        /// When set, <see cref="PrintConsole"/> uses this instead of <see cref="Name"/>.
        /// </summary>
        public string? Description { get => _description; set => _description = value; }

        /// <summary>
        /// Gets the arbitrary metadata dictionary containing key-value pairs of any type.
        /// Returns an empty dictionary if no arbitrary state was provided.
        /// </summary>
        public IReadOnlyDictionary<string, object?> ArbitraryState => _arbitraryState;

        /// <summary>
        /// Gets the type-safe metadata dictionary where all values are of type <typeparamref name="T"/>.
        /// Returns an empty dictionary if no value state was provided.
        /// </summary>
        public IReadOnlyDictionary<string, T> ValueState => _valueState;

        /// <summary>
        /// Gets a value indicating whether this entity has any arbitrary state entries.
        /// </summary>
        public bool HasArbitraryState => _arbitraryState.Count > 0;

        /// <summary>
        /// Gets a value indicating whether this entity has any value state entries.
        /// </summary>
        public bool HasValueState => _valueState.Count > 0;

        /// <summary>
        /// Attempts to retrieve a value from the arbitrary state dictionary.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">When this method returns, contains the value if found; otherwise, null.</param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetArbitraryState(string key, out object? value) => _arbitraryState.TryGetValue(key, out value);

        /// <summary>
        /// Attempts to retrieve a value from the type-safe value state dictionary.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">When this method returns, contains the value if found; otherwise, the default value.</param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValueState(string key, out T value) => _valueState.TryGetValue(key, out value!);

        /// <summary>
        /// Gets the resolved value of this entity. Equivalent to calling <see cref="Resolve"/>.
        /// </summary>
        public T Value => Resolve();

        /// <summary>
        /// Gets the resolved value as an object. Useful for cross-type operations and serialization.
        /// </summary>
        public object? ValueAsObject => Resolve();

        /// <summary>
        /// Gets the dependency expression string showing how this value is computed.
        /// Properly handles operator precedence (e.g., "A + B * C" for A + (B * C)).
        /// For base entities, returns the entity name.
        /// </summary>
        public string Dependencies => BuildDependencies();

        /// <summary>
        /// Evaluates and returns the value of this entity.
        /// For base entities, returns the stored value. For composite entities, executes the computation function.
        /// </summary>
        /// <returns>The computed value of type <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// Values are computed lazily each time <see cref="Resolve"/> is called.
        /// This ensures that changes to base entities via <see cref="Reset"/> are automatically reflected.
        /// </remarks>
        public T Resolve() => _isBase ? _baseValue : _computeFunc!();

        /// <summary>
        /// Updates the value of a base entity. Changes automatically propagate to all dependent composite entities.
        /// </summary>
        /// <param name="newValue">The new value to set.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when called on a composite entity. Only base entities can be reset.
        /// </exception>
        /// <example>
        /// <code>
        /// var price = new Traceable&lt;decimal&gt;("Price", 100m);
        /// var taxRate = new Traceable&lt;decimal&gt;("TaxRate", 0.08m);
        /// var total = price * (1 + taxRate);
        ///
        /// Console.WriteLine(total.Value);  // 108
        ///
        /// price.Reset(200m);
        /// Console.WriteLine(total.Value);  // 216 (automatically updated)
        /// </code>
        /// </example>
        public void Reset(T newValue)
        {
            if (!_isBase)
                throw new InvalidOperationException("Cannot reset a composite entity. Only base entities can be reset.");
            _baseValue = newValue!;
        }

        /// <summary>
        /// Returns the distinct names of all base entities that this entity depends on.
        /// </summary>
        /// <returns>
        /// An enumerable of unique base entity names. For a base entity, returns only its own name.
        /// For a composite entity, recursively collects all base entity names from its operands.
        /// </returns>
        public IEnumerable<string> GetDependencyNames()
        {
            if (_isBase) return new[] { _name };

            return _operands!
                .SelectMany(op => op is ITraceable<T> t ? t.GetDependencyNames() : new[] { op.Name })
                .Distinct();
        }

        /// <summary>
        /// Builds a tree representation of the computation graph starting from this entity.
        /// </summary>
        /// <returns>
        /// A <see cref="GraphNode"/> representing this entity and all its dependencies as a tree structure.
        /// Includes resolved values, operation names, and any associated state.
        /// </returns>
        /// <remarks>
        /// The returned graph can be used for visualization, serialization, or analysis of the computation.
        /// Use <see cref="PrintConsole"/> for quick console-based visualization.
        /// </remarks>
        public GraphNode BuildGraph()
        {
            var children = _isBase || _operands == null
                ? new List<GraphNode>()
                : _operands.Select(op => op.BuildGraph()).ToList();

            return new GraphNode(
                Name, Description, Resolve(), _isBase, _operation, children,
                HasArbitraryState ? _arbitraryState : null,
                HasValueState ? _valueState.ToDictionary(k => k.Key, v => (object?)v.Value) : null);
        }

        /// <summary>
        /// Renders the computation graph to the console as an ASCII tree structure.
        /// Shows entity names (or descriptions if set), values, and any associated state.
        /// </summary>
        public void PrintConsole() => Console.WriteLine(RenderGraph(BuildGraph(), "", true, true));

        #region Name/Dependency Building

        private string BuildName()
        {
            if (_isBase || _operands == null || _operands.Length == 0) return _name ?? "Unknown";
            if (_operands.Length == 1) return $"{_operation}({_operands[0].Name})";
            if (_operands.Length == 2 && IsBinaryOperator(_operation!))
                return $"{_operands[0].Name} {_operation} {_operands[1].Name}";

            return $"{_operation}({string.Join(", ", _operands.Select(o => o.Name))})";
        }

        private string BuildDependencies(int parentPrecedence = int.MaxValue)
        {
            if (_isBase) return _name;
            if (_operands!.Length == 1) return $"{_operation}({_operands[0].Dependencies})";

            if (_operands.Length == 2 && IsBinaryOperator(_operation!))
            {
                int prec = GetPrecedence(_operation!);
                string left = _operands[0] is Traceable<T> lt ? lt.BuildDependencies(prec) : _operands[0].Dependencies;
                string right = _operands[1] is Traceable<T> rt ? rt.BuildDependencies(prec) : _operands[1].Dependencies;
                string expr = $"{left} {_operation} {right}";
                return parentPrecedence != int.MaxValue && prec < parentPrecedence ? $"({expr})" : expr;
            }

            return $"{_operation}({string.Join(", ", _operands.Select(o => o.Dependencies))})";
        }

        private static int GetPrecedence(string op) => op switch
        {
            "*" or "/" => 5,
            "+" or "-" => 4,
            ">" or "<" or ">=" or "<=" => 3,
            "==" or "!=" => 2,
            "&" => 1,
            "|" => 0,
            _ => int.MaxValue
        };

        private static bool IsBinaryOperator(string op) => op is "+" or "-" or "*" or "/" or ">" or "<" or ">=" or "<=" or "==" or "!=" or "&" or "|";

        #endregion

        #region Graph Rendering

        private static string RenderGraph(GraphNode node, string prefix, bool isLast, bool isRoot)
        {
            var sb = new StringBuilder();
            string connector = isRoot ? "" : (isLast ? "└── " : "├── ");
            sb.AppendLine($"{prefix}{connector}{node.Description ?? node.Name} = {node.Value?.ToString() ?? "null"}");

            if (node.ArbitraryState != null || node.ValueState != null)
            {
                string statePrefix = prefix + (isRoot ? "" : (isLast ? "    " : "│   "));
                if (node.ArbitraryState != null)
                    foreach (var kvp in node.ArbitraryState)
                        sb.AppendLine($"{statePrefix}  [arbitrary] {kvp.Key}: {kvp.Value?.ToString() ?? "null"}");
                if (node.ValueState != null)
                    foreach (var kvp in node.ValueState)
                        sb.AppendLine($"{statePrefix}  [value] {kvp.Key}: {kvp.Value?.ToString() ?? "null"}");
            }

            if (node.Children.Count > 0)
            {
                string childPrefix = prefix + (isRoot ? "" : (isLast ? "    " : "│   "));
                for (int i = 0; i < node.Children.Count; i++)
                    sb.Append(RenderGraph(node.Children[i], childPrefix, i == node.Children.Count - 1, false));
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        #endregion

        #region Operators

        private static Traceable<T> BinaryOp(Traceable<T> left, Traceable<T> right, string op,
            Func<bool> isSupported, Func<T, T, T> compute, string supportedTypes)
        {
            if (left is null) throw new ArgumentNullException(nameof(left));
            if (right is null) throw new ArgumentNullException(nameof(right));
            if (!isSupported())
                throw new InvalidOperationException($"Operator {op} not supported for type {typeof(T).Name}. Supported: {supportedTypes}");

            return new Traceable<T>(op, new ITraceableBase[] { left, right }, () => compute(left.Resolve(), right.Resolve()));
        }

        /// <summary>
        /// Adds two traceable entities.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A new composite entity representing the sum.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when type <typeparamref name="T"/> does not support addition.
        /// Supported types: int, decimal, double, string, or types implementing <see cref="ITraceableAddable{T}"/>.
        /// </exception>
        public static Traceable<T> operator +(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "+", IsAdditionSupported, Add, $"int, decimal, double, string, ITraceableAddable<{typeof(T).Name}>");

        /// <summary>
        /// Subtracts the right traceable entity from the left.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A new composite entity representing the difference.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when type <typeparamref name="T"/> does not support subtraction.
        /// Supported types: int, decimal, double, or types implementing <see cref="ITraceableSubtractable{T}"/>.
        /// </exception>
        public static Traceable<T> operator -(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "-", IsSubtractionSupported, Subtract, $"int, decimal, double, ITraceableSubtractable<{typeof(T).Name}>");

        /// <summary>
        /// Multiplies two traceable entities.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A new composite entity representing the product.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when type <typeparamref name="T"/> does not support multiplication.
        /// Supported types: int, decimal, double, or types implementing <see cref="ITraceableMultiplicable{T}"/>.
        /// </exception>
        public static Traceable<T> operator *(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "*", IsMultiplicationSupported, Multiply, $"int, decimal, double, ITraceableMultiplicable<{typeof(T).Name}>");

        /// <summary>
        /// Divides the left traceable entity by the right.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>A new composite entity representing the quotient.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when type <typeparamref name="T"/> does not support division.
        /// Supported types: int, decimal, double, or types implementing <see cref="ITraceableDividable{T}"/>.
        /// </exception>
        public static Traceable<T> operator /(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "/", IsDivisionSupported, Divide, $"int, decimal, double, ITraceableDividable<{typeof(T).Name}>");

        /// <summary>
        /// Performs a logical AND operation on two traceable entities.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A new composite entity representing the logical AND result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when type <typeparamref name="T"/> does not support logical operations.
        /// Supported types: bool, or types implementing <see cref="ITraceableLogical{T}"/>.
        /// </exception>
        public static Traceable<T> operator &(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "&", IsLogicalSupported, And, $"bool, ITraceableLogical<{typeof(T).Name}>");

        /// <summary>
        /// Performs a logical OR operation on two traceable entities.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A new composite entity representing the logical OR result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when type <typeparamref name="T"/> does not support logical operations.
        /// Supported types: bool, or types implementing <see cref="ITraceableLogical{T}"/>.
        /// </exception>
        public static Traceable<T> operator |(Traceable<T> left, Traceable<T> right) =>
            BinaryOp(left, right, "|", IsLogicalSupported, Or, $"bool, ITraceableLogical<{typeof(T).Name}>");

        private static Traceable<bool> CompareOp(Traceable<T> left, Traceable<T> right, string op, Func<int, bool> predicate)
        {
            if (left is null) throw new ArgumentNullException(nameof(left));
            if (right is null) throw new ArgumentNullException(nameof(right));
            if (!typeof(IComparable).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException($"Type {typeof(T).Name} does not implement IComparable");

            return new Traceable<bool>(op, new ITraceableBase[] { left, right },
                () => predicate(Compare(left.Resolve(), right.Resolve())));
        }

        /// <summary>
        /// Determines whether the left entity is greater than the right entity.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A <see cref="Traceable{T}"/> of bool representing the comparison result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="T"/> does not implement <see cref="IComparable"/>.</exception>
        public static Traceable<bool> operator >(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, ">", c => c > 0);

        /// <summary>
        /// Determines whether the left entity is less than the right entity.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A <see cref="Traceable{T}"/> of bool representing the comparison result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="T"/> does not implement <see cref="IComparable"/>.</exception>
        public static Traceable<bool> operator <(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, "<", c => c < 0);

        /// <summary>
        /// Determines whether the left entity is greater than or equal to the right entity.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A <see cref="Traceable{T}"/> of bool representing the comparison result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="T"/> does not implement <see cref="IComparable"/>.</exception>
        public static Traceable<bool> operator >=(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, ">=", c => c >= 0);

        /// <summary>
        /// Determines whether the left entity is less than or equal to the right entity.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A <see cref="Traceable{T}"/> of bool representing the comparison result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="T"/> does not implement <see cref="IComparable"/>.</exception>
        public static Traceable<bool> operator <=(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, "<=", c => c <= 0);

        /// <summary>
        /// Determines whether two traceable entities have equal values.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A <see cref="Traceable{T}"/> of bool representing the equality result.</returns>
        /// <remarks>
        /// Handles null operands gracefully: both null returns true, one null returns false.
        /// Uses <see cref="EqualityComparer{T}.Default"/> for value comparison.
        /// </remarks>
        public static Traceable<bool> operator ==(Traceable<T> left, Traceable<T> right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return new Traceable<bool>(true, "true");
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return new Traceable<bool>(false, "false");
            return new Traceable<bool>("==", new ITraceableBase[] { left, right },
                () => EqualityComparer<T>.Default.Equals(left.Resolve(), right.Resolve()));
        }

        /// <summary>
        /// Determines whether two traceable entities have unequal values.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A <see cref="Traceable{T}"/> of bool representing the inequality result.</returns>
        /// <remarks>
        /// Handles null operands gracefully: both null returns false, one null returns true.
        /// Uses <see cref="EqualityComparer{T}.Default"/> for value comparison.
        /// </remarks>
        public static Traceable<bool> operator !=(Traceable<T> left, Traceable<T> right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return new Traceable<bool>(false, "false");
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return new Traceable<bool>(true, "true");
            return new Traceable<bool>("!=", new ITraceableBase[] { left, right },
                () => !EqualityComparer<T>.Default.Equals(left.Resolve(), right.Resolve()));
        }

        /// <summary>
        /// Determines whether this entity's resolved value equals another entity's resolved value.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="Traceable{T}"/> with an equal resolved value; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is Traceable<T> other && EqualityComparer<T>.Default.Equals(Resolve(), other.Resolve());

        /// <summary>
        /// Returns the hash code for this entity's resolved value.
        /// </summary>
        /// <returns>The hash code of the resolved value, or 0 if the value is null.</returns>
        public override int GetHashCode() => Resolve()?.GetHashCode() ?? 0;

        #endregion

        #region Type Dispatch

        private static bool IsNumericType() => typeof(T) == typeof(int) || typeof(T) == typeof(decimal) || typeof(T) == typeof(double);
        private static bool IsAdditionSupported() => IsNumericType() || typeof(T) == typeof(string) || typeof(ITraceableAddable<T>).IsAssignableFrom(typeof(T));
        private static bool IsSubtractionSupported() => IsNumericType() || typeof(ITraceableSubtractable<T>).IsAssignableFrom(typeof(T));
        private static bool IsMultiplicationSupported() => IsNumericType() || typeof(ITraceableMultiplicable<T>).IsAssignableFrom(typeof(T));
        private static bool IsDivisionSupported() => IsNumericType() || typeof(ITraceableDividable<T>).IsAssignableFrom(typeof(T));
        private static bool IsLogicalSupported() => typeof(T) == typeof(bool) || typeof(ITraceableLogical<T>).IsAssignableFrom(typeof(T));

        private static T NumericOp(T left, T right, Func<int, int, int> intOp, Func<decimal, decimal, decimal> decOp, Func<double, double, double> dblOp)
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
            if (left is ITraceableAddable<T> a) return a.Add(left, right);
            throw new InvalidOperationException($"Add not supported for {typeof(T).Name}");
        }

        private static T Subtract(T left, T right)
        {
            if (IsNumericType()) return NumericOp(left, right, (l, r) => l - r, (l, r) => l - r, (l, r) => l - r);
            if (left is ITraceableSubtractable<T> s) return s.Subtract(left, right);
            throw new InvalidOperationException($"Subtract not supported for {typeof(T).Name}");
        }

        private static T Multiply(T left, T right)
        {
            if (IsNumericType()) return NumericOp(left, right, (l, r) => l * r, (l, r) => l * r, (l, r) => l * r);
            if (left is ITraceableMultiplicable<T> m) return m.Multiply(left, right);
            throw new InvalidOperationException($"Multiply not supported for {typeof(T).Name}");
        }

        private static T Divide(T left, T right)
        {
            if (IsNumericType()) return NumericOp(left, right, (l, r) => l / r, (l, r) => l / r, (l, r) => l / r);
            if (left is ITraceableDividable<T> d) return d.Divide(left, right);
            throw new InvalidOperationException($"Divide not supported for {typeof(T).Name}");
        }

        private static int Compare(T left, T right) =>
            left is IComparable c ? c.CompareTo(right) : throw new InvalidOperationException($"{typeof(T).Name} is not IComparable");

        private static T And(T left, T right)
        {
            if (typeof(T) == typeof(bool)) return (T)(object)((bool)(object)left! && (bool)(object)right!);
            if (left is ITraceableLogical<T> l) return l.And(left, right);
            throw new InvalidOperationException($"And not supported for {typeof(T).Name}");
        }

        private static T Or(T left, T right)
        {
            if (typeof(T) == typeof(bool)) return (T)(object)((bool)(object)left! || (bool)(object)right!);
            if (left is ITraceableLogical<T> l) return l.Or(left, right);
            throw new InvalidOperationException($"Or not supported for {typeof(T).Name}");
        }

        #endregion
    }
}
