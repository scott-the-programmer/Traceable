namespace Traceable
{
    /// <summary>Implement to support the + operator.</summary>
    public interface ITraceableAddable<T>
    {
        T Add(T left, T right);
    }

    /// <summary>Implement to support the - operator.</summary>
    public interface ITraceableSubtractable<T>
    {
        T Subtract(T left, T right);
    }

    /// <summary>Implement to support the * operator.</summary>
    public interface ITraceableMultiplicable<T>
    {
        T Multiply(T left, T right);
    }

    /// <summary>Implement to support the / operator.</summary>
    public interface ITraceableDividable<T>
    {
        T Divide(T left, T right);
    }

    /// <summary>Implement to support the &amp; and | operators.</summary>
    public interface ITraceableLogical<T>
    {
        T And(T left, T right);
        T Or(T left, T right);
    }

    /// <summary>Combines all arithmetic operations (+, -, *, /).</summary>
    public interface ITraceableArithmetic<T> :
        ITraceableAddable<T>,
        ITraceableSubtractable<T>,
        ITraceableMultiplicable<T>,
        ITraceableDividable<T>
    {
    }
}
