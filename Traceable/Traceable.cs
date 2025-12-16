using System;
using System.Collections.Generic;
using System.Linq;

namespace Traceable;

/// <summary>Wraps a value of type T and tracks its computation history and dependencies.</summary>
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
    private readonly ITraceableBase[]? _scopeConditions;

    private static readonly IReadOnlyDictionary<string, object?> EmptyArbitraryState = new Dictionary<string, object?>();
    private static readonly IReadOnlyDictionary<string, T> EmptyValueState = new Dictionary<string, T>();

    /// <summary>Creates a base traceable entity with a mutable value.</summary>
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
        _scopeConditions = TraceableScope.GetCurrentScope();
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

    /// <summary>The identifier for this entity.</summary>
    public string Name => _name;

    /// <summary>Optional description for display purposes.</summary>
    public string? Description { get => _description; set => _description = value; }

    public IReadOnlyDictionary<string, object?> ArbitraryState => _arbitraryState;
    public IReadOnlyDictionary<string, T> ValueState => _valueState;
    public bool HasArbitraryState => _arbitraryState.Count > 0;
    public bool HasValueState => _valueState.Count > 0;
    public bool TryGetArbitraryState(string key, out object? value) => _arbitraryState.TryGetValue(key, out value);
    public bool TryGetValueState(string key, out T value) => _valueState.TryGetValue(key, out value!);

    /// <summary>The resolved value of this entity.</summary>
    public T Value => Resolve();

    public object? ValueAsObject => Resolve();

    /// <summary>The dependency expression showing how this value is computed.</summary>
    public string Dependencies => BuildDependencies();

    /// <summary>Evaluates and returns the value of this entity.</summary>
    public T Resolve() => _isBase ? _baseValue : _computeFunc!();

    /// <summary>Updates the value of a base entity.</summary>
    public void Reload(T newValue)
    {
        if (!_isBase)
            throw new InvalidOperationException("Cannot reload a composite entity. Only base entities can be reloaded.");
        _baseValue = newValue!;
    }

    public IEnumerable<string> GetDependencyNames()
    {
        var names = _isBase
            ? new[] { _name }
            : _operands!.SelectMany(op => op is ITraceable<T> t ? t.GetDependencyNames() : new[] { op.Name });

        if (_scopeConditions != null && _scopeConditions.Length > 0)
            names = names.Concat(_scopeConditions.Select(c => c.Name));

        return names.Distinct();
    }

    public GraphNode BuildGraph()
    {
        var children = _isBase || _operands == null
            ? new List<GraphNode>()
            : _operands.Select(op => op.BuildGraph()).ToList();

        if (_scopeConditions != null && _scopeConditions.Length > 0)
            children.AddRange(_scopeConditions.Select(c => c.BuildGraph()));

        return new GraphNode(
            Name, Description, Resolve(), _isBase, _operation, children,
            HasArbitraryState ? _arbitraryState : null,
            HasValueState ? _valueState.ToDictionary(k => k.Key, v => (object?)v.Value) : null);
    }

    /// <summary>Renders the computation graph to the console as an ASCII tree.</summary>
    public void PrintConsole() => Console.WriteLine(TraceableGraphRenderer.Render(BuildGraph(), "", true, true));

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
        if (_isBase)
        {
            if (_scopeConditions == null || _scopeConditions.Length == 0)
                return _name;
            var conditions = string.Join(" & ", _scopeConditions.Select(c => c.Name));
            return $"{_name} (when {conditions})";
        }

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

    private static bool IsBinaryOperator(string op) =>
        op is "+" or "-" or "*" or "/" or ">" or "<" or ">=" or "<=" or "==" or "!=" or "&" or "|";

    private static Traceable<T> BinaryOp(Traceable<T> left, Traceable<T> right, string op,
        Func<bool> isSupported, Func<T, T, T> compute, string supportedTypes)
    {
        if (left is null) throw new ArgumentNullException(nameof(left));
        if (right is null) throw new ArgumentNullException(nameof(right));
        if (!isSupported())
            throw new InvalidOperationException($"Operator {op} not supported for type {typeof(T).Name}. Supported: {supportedTypes}");

        return new Traceable<T>(op, new ITraceableBase[] { left, right }, () => compute(left.Resolve(), right.Resolve()));
    }

    public static Traceable<T> operator +(Traceable<T> left, Traceable<T> right) =>
        BinaryOp(left, right, "+", TraceableOperations<T>.IsAdditionSupported, TraceableOperations<T>.Add,
            $"int, decimal, double, string, ITraceableAddable<{typeof(T).Name}>");

    public static Traceable<T> operator -(Traceable<T> left, Traceable<T> right) =>
        BinaryOp(left, right, "-", TraceableOperations<T>.IsSubtractionSupported, TraceableOperations<T>.Subtract,
            $"int, decimal, double, ITraceableSubtractable<{typeof(T).Name}>");

    public static Traceable<T> operator *(Traceable<T> left, Traceable<T> right) =>
        BinaryOp(left, right, "*", TraceableOperations<T>.IsMultiplicationSupported, TraceableOperations<T>.Multiply,
            $"int, decimal, double, ITraceableMultiplicable<{typeof(T).Name}>");

    public static Traceable<T> operator /(Traceable<T> left, Traceable<T> right) =>
        BinaryOp(left, right, "/", TraceableOperations<T>.IsDivisionSupported, TraceableOperations<T>.Divide,
            $"int, decimal, double, ITraceableDividable<{typeof(T).Name}>");

    public static Traceable<T> operator &(Traceable<T> left, Traceable<T> right) =>
        BinaryOp(left, right, "&", TraceableOperations<T>.IsLogicalSupported, TraceableOperations<T>.And,
            $"bool, ITraceableLogical<{typeof(T).Name}>");

    public static Traceable<T> operator |(Traceable<T> left, Traceable<T> right) =>
        BinaryOp(left, right, "|", TraceableOperations<T>.IsLogicalSupported, TraceableOperations<T>.Or,
            $"bool, ITraceableLogical<{typeof(T).Name}>");

    private static Traceable<bool> CompareOp(Traceable<T> left, Traceable<T> right, string op, Func<int, bool> predicate)
    {
        if (left is null) throw new ArgumentNullException(nameof(left));
        if (right is null) throw new ArgumentNullException(nameof(right));
        if (!typeof(IComparable).IsAssignableFrom(typeof(T)))
            throw new InvalidOperationException($"Type {typeof(T).Name} does not implement IComparable");

        return new Traceable<bool>(op, new ITraceableBase[] { left, right },
            () => predicate(TraceableOperations<T>.Compare(left.Resolve(), right.Resolve())));
    }

    public static Traceable<bool> operator >(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, ">", c => c > 0);
    public static Traceable<bool> operator <(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, "<", c => c < 0);
    public static Traceable<bool> operator >=(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, ">=", c => c >= 0);
    public static Traceable<bool> operator <=(Traceable<T> left, Traceable<T> right) => CompareOp(left, right, "<=", c => c <= 0);

    public static Traceable<bool> operator ==(Traceable<T> left, Traceable<T> right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return new Traceable<bool>(true, "true");
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return new Traceable<bool>(false, "false");
        return new Traceable<bool>("==", new ITraceableBase[] { left, right },
            () => EqualityComparer<T>.Default.Equals(left.Resolve(), right.Resolve()));
    }

    public static Traceable<bool> operator !=(Traceable<T> left, Traceable<T> right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return new Traceable<bool>(false, "false");
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return new Traceable<bool>(true, "true");
        return new Traceable<bool>("!=", new ITraceableBase[] { left, right },
            () => !EqualityComparer<T>.Default.Equals(left.Resolve(), right.Resolve()));
    }

    public override bool Equals(object? obj) => obj is Traceable<T> other && EqualityComparer<T>.Default.Equals(Resolve(), other.Resolve());
    public override int GetHashCode() => Resolve()?.GetHashCode() ?? 0;
}
