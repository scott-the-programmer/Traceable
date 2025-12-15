namespace EntityTrace
{
    /// <summary>
    /// Enables addition operations for custom types wrapped in Traceable.
    /// Implement this interface to support the + operator.
    /// </summary>
    /// <typeparam name="T">The type that supports addition.</typeparam>
    public interface ITraceableAddable<T>
    {
        /// <summary>
        /// Adds two values of type T.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The sum of the two values.</returns>
        T Add(T left, T right);
    }

    /// <summary>
    /// Enables subtraction operations for custom types wrapped in Traceable.
    /// Implement this interface to support the - operator.
    /// </summary>
    /// <typeparam name="T">The type that supports subtraction.</typeparam>
    public interface ITraceableSubtractable<T>
    {
        /// <summary>
        /// Subtracts the right value from the left value.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The difference of the two values.</returns>
        T Subtract(T left, T right);
    }

    /// <summary>
    /// Enables multiplication operations for custom types wrapped in Traceable.
    /// Implement this interface to support the * operator.
    /// </summary>
    /// <typeparam name="T">The type that supports multiplication.</typeparam>
    public interface ITraceableMultiplicable<T>
    {
        /// <summary>
        /// Multiplies two values of type T.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The product of the two values.</returns>
        T Multiply(T left, T right);
    }

    /// <summary>
    /// Enables division operations for custom types wrapped in Traceable.
    /// Implement this interface to support the / operator.
    /// </summary>
    /// <typeparam name="T">The type that supports division.</typeparam>
    public interface ITraceableDividable<T>
    {
        /// <summary>
        /// Divides the left value by the right value.
        /// </summary>
        /// <param name="left">The left operand (dividend).</param>
        /// <param name="right">The right operand (divisor).</param>
        /// <returns>The quotient of the two values.</returns>
        T Divide(T left, T right);
    }

    /// <summary>
    /// Enables logical AND and OR operations for custom types wrapped in Traceable.
    /// Implement this interface to support the & and | operators.
    /// </summary>
    /// <typeparam name="T">The type that supports logical operations.</typeparam>
    public interface ITraceableLogical<T>
    {
        /// <summary>
        /// Performs logical AND between two values.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The logical AND result.</returns>
        T And(T left, T right);

        /// <summary>
        /// Performs logical OR between two values.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The logical OR result.</returns>
        T Or(T left, T right);
    }

    /// <summary>
    /// Convenience interface combining all arithmetic operations.
    /// Implement this if your type supports all four basic arithmetic operations (+, -, *, /).
    /// </summary>
    /// <typeparam name="T">The type that supports all arithmetic operations.</typeparam>
    public interface ITraceableArithmetic<T> :
        ITraceableAddable<T>,
        ITraceableSubtractable<T>,
        ITraceableMultiplicable<T>,
        ITraceableDividable<T>
    {
    }
}
