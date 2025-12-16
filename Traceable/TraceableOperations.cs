using System;

namespace Traceable;

internal static class TraceableOperations<T>
{
    public static bool IsNumericType() =>
        typeof(T) == typeof(int) || typeof(T) == typeof(decimal) || typeof(T) == typeof(double);

    public static bool IsAdditionSupported() =>
        IsNumericType() || typeof(T) == typeof(string) || typeof(ITraceableAddable<T>).IsAssignableFrom(typeof(T));

    public static bool IsSubtractionSupported() =>
        IsNumericType() || typeof(ITraceableSubtractable<T>).IsAssignableFrom(typeof(T));

    public static bool IsMultiplicationSupported() =>
        IsNumericType() || typeof(ITraceableMultiplicable<T>).IsAssignableFrom(typeof(T));

    public static bool IsDivisionSupported() =>
        IsNumericType() || typeof(ITraceableDividable<T>).IsAssignableFrom(typeof(T));

    public static bool IsLogicalSupported() =>
        typeof(T) == typeof(bool) || typeof(ITraceableLogical<T>).IsAssignableFrom(typeof(T));

    private static T NumericOp(T left, T right, Func<int, int, int> intOp, Func<decimal, decimal, decimal> decOp, Func<double, double, double> dblOp)
    {
        if (typeof(T) == typeof(int)) return (T)(object)intOp((int)(object)left!, (int)(object)right!);
        if (typeof(T) == typeof(decimal)) return (T)(object)decOp((decimal)(object)left!, (decimal)(object)right!);
        if (typeof(T) == typeof(double)) return (T)(object)dblOp((double)(object)left!, (double)(object)right!);
        return default!;
    }

    public static T Add(T left, T right)
    {
        if (IsNumericType()) return NumericOp(left, right, (l, r) => l + r, (l, r) => l + r, (l, r) => l + r);
        if (typeof(T) == typeof(string)) return (T)(object)((string)(object)left! + (string)(object)right!);
        if (left is ITraceableAddable<T> a) return a.Add(left, right);
        throw new InvalidOperationException($"Add not supported for {typeof(T).Name}");
    }

    public static T Subtract(T left, T right)
    {
        if (IsNumericType()) return NumericOp(left, right, (l, r) => l - r, (l, r) => l - r, (l, r) => l - r);
        if (left is ITraceableSubtractable<T> s) return s.Subtract(left, right);
        throw new InvalidOperationException($"Subtract not supported for {typeof(T).Name}");
    }

    public static T Multiply(T left, T right)
    {
        if (IsNumericType()) return NumericOp(left, right, (l, r) => l * r, (l, r) => l * r, (l, r) => l * r);
        if (left is ITraceableMultiplicable<T> m) return m.Multiply(left, right);
        throw new InvalidOperationException($"Multiply not supported for {typeof(T).Name}");
    }

    public static T Divide(T left, T right)
    {
        if (IsNumericType()) return NumericOp(left, right, (l, r) => l / r, (l, r) => l / r, (l, r) => l / r);
        if (left is ITraceableDividable<T> d) return d.Divide(left, right);
        throw new InvalidOperationException($"Divide not supported for {typeof(T).Name}");
    }

    public static int Compare(T left, T right) =>
        left is IComparable c ? c.CompareTo(right) : throw new InvalidOperationException($"{typeof(T).Name} is not IComparable");

    public static T And(T left, T right)
    {
        if (typeof(T) == typeof(bool)) return (T)(object)((bool)(object)left! && (bool)(object)right!);
        if (left is ITraceableLogical<T> l) return l.And(left, right);
        throw new InvalidOperationException($"And not supported for {typeof(T).Name}");
    }

    public static T Or(T left, T right)
    {
        if (typeof(T) == typeof(bool)) return (T)(object)((bool)(object)left! || (bool)(object)right!);
        if (left is ITraceableLogical<T> l) return l.Or(left, right);
        throw new InvalidOperationException($"Or not supported for {typeof(T).Name}");
    }
}
