using System;

namespace Traceable.Tests
{
    /// <summary>
    /// Example custom type with partial arithmetic support (addition and subtraction only).
    /// </summary>
    public struct Vector2D : ITraceableAddable<Vector2D>, ITraceableSubtractable<Vector2D>, IEquatable<Vector2D>
    {
        public double X { get; }
        public double Y { get; }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2D Add(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X + right.X, left.Y + right.Y);
        }

        public Vector2D Subtract(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X - right.X, left.Y - right.Y);
        }

        public bool Equals(Vector2D other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2D other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString() => $"({X}, {Y})";

        public static bool operator ==(Vector2D left, Vector2D right) => left.Equals(right);
        public static bool operator !=(Vector2D left, Vector2D right) => !left.Equals(right);
    }

    /// <summary>
    /// Example custom type with full arithmetic support and comparison.
    /// </summary>
    public struct Money : ITraceableArithmetic<Money>, IComparable<Money>, IComparable, IEquatable<Money>
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        }

        public Money Add(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException($"Cannot add money with different currencies: {left.Currency} and {right.Currency}");
            return new Money(left.Amount + right.Amount, left.Currency);
        }

        public Money Subtract(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException($"Cannot subtract money with different currencies: {left.Currency} and {right.Currency}");
            return new Money(left.Amount - right.Amount, left.Currency);
        }

        public Money Multiply(Money left, Money right)
        {
            // For money, multiply by scalar (treat right as multiplier)
            return new Money(left.Amount * right.Amount, left.Currency);
        }

        public Money Divide(Money left, Money right)
        {
            return new Money(left.Amount / right.Amount, left.Currency);
        }

        public int CompareTo(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot compare money with different currencies: {Currency} and {other.Currency}");
            return Amount.CompareTo(other.Amount);
        }

        public int CompareTo(object obj)
        {
            if (obj is Money other)
                return CompareTo(other);
            throw new ArgumentException("Object is not a Money");
        }

        public bool Equals(Money other)
        {
            return Amount == other.Amount && Currency == other.Currency;
        }

        public override bool Equals(object obj)
        {
            return obj is Money other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        public override string ToString() => $"{Amount:F2} {Currency}";

        public static bool operator ==(Money left, Money right) => left.Equals(right);
        public static bool operator !=(Money left, Money right) => !left.Equals(right);
    }

    /// <summary>
    /// Example custom type with logical operations (three-valued logic).
    /// </summary>
    public struct TriState : ITraceableLogical<TriState>, IEquatable<TriState>
    {
        public enum State
        {
            False,
            True,
            Unknown
        }

        public State Value { get; }

        public TriState(State value)
        {
            Value = value;
        }

        public TriState And(TriState left, TriState right)
        {
            // Three-valued logic truth table for AND
            if (left.Value == State.False || right.Value == State.False)
                return new TriState(State.False);
            if (left.Value == State.Unknown || right.Value == State.Unknown)
                return new TriState(State.Unknown);
            return new TriState(State.True);
        }

        public TriState Or(TriState left, TriState right)
        {
            // Three-valued logic truth table for OR
            if (left.Value == State.True || right.Value == State.True)
                return new TriState(State.True);
            if (left.Value == State.Unknown || right.Value == State.Unknown)
                return new TriState(State.Unknown);
            return new TriState(State.False);
        }

        public bool Equals(TriState other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is TriState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }

        public override string ToString() => Value.ToString();

        public static bool operator ==(TriState left, TriState right) => left.Equals(right);
        public static bool operator !=(TriState left, TriState right) => !left.Equals(right);
    }
}
