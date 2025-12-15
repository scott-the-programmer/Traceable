using System;
using Xunit;

namespace EntityTrace.Tests
{
    public class CustomTypeTests
    {
        #region Vector2D Tests

        [Fact]
        public void Vector2D_Addition_WithInterface_Works()
        {
            var v1 = new Traceable<Vector2D>("V1", new Vector2D(1, 2));
            var v2 = new Traceable<Vector2D>("V2", new Vector2D(3, 4));

            var sum = v1 + v2;

            Assert.Equal(new Vector2D(4, 6), sum.Resolve());
            Assert.Equal("V1 + V2", sum.Dependencies);
        }

        [Fact]
        public void Vector2D_Subtraction_WithInterface_Works()
        {
            var v1 = new Traceable<Vector2D>("V1", new Vector2D(5, 7));
            var v2 = new Traceable<Vector2D>("V2", new Vector2D(2, 3));

            var diff = v1 - v2;

            Assert.Equal(new Vector2D(3, 4), diff.Resolve());
            Assert.Equal("V1 - V2", diff.Dependencies);
        }

        [Fact]
        public void Vector2D_ComplexExpression_TracksCorrectly()
        {
            var a = new Traceable<Vector2D>("A", new Vector2D(1, 1));
            var b = new Traceable<Vector2D>("B", new Vector2D(2, 2));
            var c = new Traceable<Vector2D>("C", new Vector2D(3, 3));

            var result = a + b - c;

            Assert.Equal(new Vector2D(0, 0), result.Resolve());
            Assert.Equal("A + B - C", result.Dependencies);
        }

        [Fact]
        public void Vector2D_Multiplication_WithoutInterface_Throws()
        {
            var v1 = new Traceable<Vector2D>("V1", new Vector2D(1, 2));
            var v2 = new Traceable<Vector2D>("V2", new Vector2D(3, 4));

            var ex = Assert.Throws<InvalidOperationException>(() => { var result = v1 * v2; });
            Assert.Contains("Operator * not supported for type Vector2D", ex.Message);
            Assert.Contains("ITraceableMultiplicable", ex.Message);
        }

        [Fact]
        public void Vector2D_Division_WithoutInterface_Throws()
        {
            var v1 = new Traceable<Vector2D>("V1", new Vector2D(1, 2));
            var v2 = new Traceable<Vector2D>("V2", new Vector2D(3, 4));

            var ex = Assert.Throws<InvalidOperationException>(() => { var result = v1 / v2; });
            Assert.Contains("Operator / not supported for type Vector2D", ex.Message);
            Assert.Contains("ITraceableDividable", ex.Message);
        }

        [Fact]
        public void Vector2D_BuildGraph_DisplaysCorrectly()
        {
            var v1 = new Traceable<Vector2D>("V1", new Vector2D(1, 2));
            var v2 = new Traceable<Vector2D>("V2", new Vector2D(3, 4));
            var sum = v1 + v2;

            var graph = sum.BuildGraph();

            Assert.Equal("+", graph.Operation);
            Assert.Equal(new Vector2D(4, 6), graph.Value);
            Assert.Equal(2, graph.Children.Count);
            Assert.Equal("V1", graph.Children[0].Name);
            Assert.Equal("V2", graph.Children[1].Name);
        }

        [Fact]
        public void Vector2D_Reset_PropagatesChanges()
        {
            var v1 = new Traceable<Vector2D>("V1", new Vector2D(1, 2));
            var v2 = new Traceable<Vector2D>("V2", new Vector2D(3, 4));
            var sum = v1 + v2;

            Assert.Equal(new Vector2D(4, 6), sum.Resolve());

            v1.Reset(new Vector2D(10, 20));

            Assert.Equal(new Vector2D(13, 24), sum.Resolve());
        }

        #endregion

        #region Money Tests

        [Fact]
        public void Money_Addition_SameCurrency_Works()
        {
            var price = new Traceable<Money>("Price", new Money(100m, "USD"));
            var tax = new Traceable<Money>("Tax", new Money(10m, "USD"));

            var total = price + tax;

            Assert.Equal(new Money(110m, "USD"), total.Resolve());
            Assert.Equal("Price + Tax", total.Dependencies);
        }

        [Fact]
        public void Money_Subtraction_SameCurrency_Works()
        {
            var total = new Traceable<Money>("Total", new Money(100m, "USD"));
            var discount = new Traceable<Money>("Discount", new Money(15m, "USD"));

            var final = total - discount;

            Assert.Equal(new Money(85m, "USD"), final.Resolve());
            Assert.Equal("Total - Discount", final.Dependencies);
        }

        [Fact]
        public void Money_Multiplication_Works()
        {
            var price = new Traceable<Money>("Price", new Money(25m, "USD"));
            var quantity = new Traceable<Money>("Quantity", new Money(4m, "USD"));

            var total = price * quantity;

            Assert.Equal(new Money(100m, "USD"), total.Resolve());
            Assert.Equal("Price * Quantity", total.Dependencies);
        }

        [Fact]
        public void Money_Division_Works()
        {
            var total = new Traceable<Money>("Total", new Money(100m, "USD"));
            var divisor = new Traceable<Money>("Divisor", new Money(4m, "USD"));

            var result = total / divisor;

            Assert.Equal(new Money(25m, "USD"), result.Resolve());
            Assert.Equal("Total / Divisor", result.Dependencies);
        }

        [Fact]
        public void Money_ComplexExpression_Works()
        {
            var price = new Traceable<Money>("Price", new Money(100m, "USD"));
            var quantity = new Traceable<Money>("Quantity", new Money(5m, "USD"));
            var discount = new Traceable<Money>("Discount", new Money(50m, "USD"));

            var final = price * quantity - discount;

            Assert.Equal(new Money(450m, "USD"), final.Resolve());
            Assert.Equal("Price * Quantity - Discount", final.Dependencies);
        }

        [Fact]
        public void Money_Addition_DifferentCurrency_Throws()
        {
            var usd = new Traceable<Money>("USD", new Money(100m, "USD"));
            var eur = new Traceable<Money>("EUR", new Money(100m, "EUR"));

            var ex = Assert.Throws<InvalidOperationException>(() => { var result = (usd + eur).Resolve(); });
            Assert.Contains("different currencies", ex.Message);
        }

        [Fact]
        public void Money_Comparison_SameCurrency_Works()
        {
            var a = new Traceable<Money>("A", new Money(100m, "USD"));
            var b = new Traceable<Money>("B", new Money(50m, "USD"));

            var isGreater = a > b;
            var isLess = a < b;

            Assert.True(isGreater.Resolve());
            Assert.False(isLess.Resolve());
            Assert.Equal("A > B", isGreater.Dependencies);
            Assert.Equal("A < B", isLess.Dependencies);
        }

        [Fact]
        public void Money_Comparison_DifferentCurrency_Throws()
        {
            var usd = new Traceable<Money>("USD", new Money(100m, "USD"));
            var eur = new Traceable<Money>("EUR", new Money(50m, "EUR"));

            var comparison = usd > eur;

            var ex = Assert.Throws<InvalidOperationException>(() => comparison.Resolve());
            Assert.Contains("different currencies", ex.Message);
        }

        #endregion

        #region TriState Tests

        [Fact]
        public void TriState_And_TrueAndTrue_ReturnsTrue()
        {
            var a = new Traceable<TriState>("A", new TriState(TriState.State.True));
            var b = new Traceable<TriState>("B", new TriState(TriState.State.True));

            var result = a & b;

            Assert.Equal(new TriState(TriState.State.True), result.Resolve());
            Assert.Equal("A & B", result.Dependencies);
        }

        [Fact]
        public void TriState_And_TrueAndFalse_ReturnsFalse()
        {
            var a = new Traceable<TriState>("A", new TriState(TriState.State.True));
            var b = new Traceable<TriState>("B", new TriState(TriState.State.False));

            var result = a & b;

            Assert.Equal(new TriState(TriState.State.False), result.Resolve());
        }

        [Fact]
        public void TriState_And_TrueAndUnknown_ReturnsUnknown()
        {
            var a = new Traceable<TriState>("A", new TriState(TriState.State.True));
            var b = new Traceable<TriState>("B", new TriState(TriState.State.Unknown));

            var result = a & b;

            Assert.Equal(new TriState(TriState.State.Unknown), result.Resolve());
        }

        [Fact]
        public void TriState_Or_FalseOrFalse_ReturnsFalse()
        {
            var a = new Traceable<TriState>("A", new TriState(TriState.State.False));
            var b = new Traceable<TriState>("B", new TriState(TriState.State.False));

            var result = a | b;

            Assert.Equal(new TriState(TriState.State.False), result.Resolve());
            Assert.Equal("A | B", result.Dependencies);
        }

        [Fact]
        public void TriState_Or_TrueOrFalse_ReturnsTrue()
        {
            var a = new Traceable<TriState>("A", new TriState(TriState.State.True));
            var b = new Traceable<TriState>("B", new TriState(TriState.State.False));

            var result = a | b;

            Assert.Equal(new TriState(TriState.State.True), result.Resolve());
        }

        [Fact]
        public void TriState_Or_FalseOrUnknown_ReturnsUnknown()
        {
            var a = new Traceable<TriState>("A", new TriState(TriState.State.False));
            var b = new Traceable<TriState>("B", new TriState(TriState.State.Unknown));

            var result = a | b;

            Assert.Equal(new TriState(TriState.State.Unknown), result.Resolve());
        }

        [Fact]
        public void TriState_ComplexExpression_TracksCorrectly()
        {
            var a = new Traceable<TriState>("A", new TriState(TriState.State.True));
            var b = new Traceable<TriState>("B", new TriState(TriState.State.False));
            var c = new Traceable<TriState>("C", new TriState(TriState.State.Unknown));

            var result = a & b | c;

            Assert.Equal(new TriState(TriState.State.Unknown), result.Resolve());
            Assert.Equal("A & B | C", result.Dependencies);
        }

        #endregion

        #region Backward Compatibility Tests

        [Fact]
        public void PrimitiveTypes_Int_StillWorkIdentically()
        {
            var a = new Traceable<int>("A", 5);
            var b = new Traceable<int>("B", 3);

            var sum = a + b;
            var diff = a - b;
            var product = a * b;
            var quotient = a / b;

            Assert.Equal(8, sum.Resolve());
            Assert.Equal(2, diff.Resolve());
            Assert.Equal(15, product.Resolve());
            Assert.Equal(1, quotient.Resolve());
        }

        [Fact]
        public void PrimitiveTypes_Decimal_StillWorkIdentically()
        {
            var a = new Traceable<decimal>("A", 10.5m);
            var b = new Traceable<decimal>("B", 2.5m);

            var sum = a + b;
            var diff = a - b;
            var product = a * b;
            var quotient = a / b;

            Assert.Equal(13.0m, sum.Resolve());
            Assert.Equal(8.0m, diff.Resolve());
            Assert.Equal(26.25m, product.Resolve());
            Assert.Equal(4.2m, quotient.Resolve());
        }

        [Fact]
        public void PrimitiveTypes_Double_StillWorkIdentically()
        {
            var a = new Traceable<double>("A", 10.5);
            var b = new Traceable<double>("B", 2.5);

            var sum = a + b;

            Assert.Equal(13.0, sum.Resolve());
        }

        [Fact]
        public void PrimitiveTypes_String_StillWorkIdentically()
        {
            var a = new Traceable<string>("A", "Hello");
            var b = new Traceable<string>("B", " World");

            var concat = a + b;

            Assert.Equal("Hello World", concat.Resolve());
        }

        [Fact]
        public void PrimitiveTypes_Bool_StillWorkIdentically()
        {
            var a = new Traceable<bool>("A", true);
            var b = new Traceable<bool>("B", false);

            var and = a & b;
            var or = a | b;

            Assert.False(and.Resolve());
            Assert.True(or.Resolve());
        }

        [Fact]
        public void PrimitiveTypes_Comparison_StillWorkIdentically()
        {
            var a = new Traceable<int>("A", 10);
            var b = new Traceable<int>("B", 5);

            var greater = a > b;
            var less = a < b;
            var greaterOrEqual = a >= b;
            var lessOrEqual = a <= b;

            Assert.True(greater.Resolve());
            Assert.False(less.Resolve());
            Assert.True(greaterOrEqual.Resolve());
            Assert.False(lessOrEqual.Resolve());
        }

        #endregion

        #region Error Message Tests

        [Fact]
        public void UnsupportedType_Addition_ShowsHelpfulErrorMessage()
        {
            var a = new Traceable<DateTime>("A", DateTime.Now);
            var b = new Traceable<DateTime>("B", DateTime.Now);

            var ex = Assert.Throws<InvalidOperationException>(() => { var result = a + b; });
            Assert.Contains("Operator + not supported for type DateTime", ex.Message);
            Assert.Contains("ITraceableAddable<DateTime>", ex.Message);
        }

        [Fact]
        public void UnsupportedType_Subtraction_ShowsHelpfulErrorMessage()
        {
            var a = new Traceable<DateTime>("A", DateTime.Now);
            var b = new Traceable<DateTime>("B", DateTime.Now);

            var ex = Assert.Throws<InvalidOperationException>(() => { var result = a - b; });
            Assert.Contains("Operator - not supported for type DateTime", ex.Message);
            Assert.Contains("ITraceableSubtractable<DateTime>", ex.Message);
        }

        [Fact]
        public void UnsupportedType_Multiplication_ShowsHelpfulErrorMessage()
        {
            var a = new Traceable<DateTime>("A", DateTime.Now);
            var b = new Traceable<DateTime>("B", DateTime.Now);

            var ex = Assert.Throws<InvalidOperationException>(() => { var result = a * b; });
            Assert.Contains("Operator * not supported for type DateTime", ex.Message);
            Assert.Contains("ITraceableMultiplicable<DateTime>", ex.Message);
        }

        [Fact]
        public void UnsupportedType_Division_ShowsHelpfulErrorMessage()
        {
            var a = new Traceable<DateTime>("A", DateTime.Now);
            var b = new Traceable<DateTime>("B", DateTime.Now);

            var ex = Assert.Throws<InvalidOperationException>(() => { var result = a / b; });
            Assert.Contains("Operator / not supported for type DateTime", ex.Message);
            Assert.Contains("ITraceableDividable<DateTime>", ex.Message);
        }

        [Fact]
        public void UnsupportedType_LogicalAnd_ShowsHelpfulErrorMessage()
        {
            var a = new Traceable<int>("A", 1);
            var b = new Traceable<int>("B", 2);

            var ex = Assert.Throws<InvalidOperationException>(() => { var result = a & b; });
            Assert.Contains("Operator & not supported for type Int32", ex.Message);
            Assert.Contains("ITraceableLogical<Int32>", ex.Message);
        }

        [Fact]
        public void UnsupportedType_LogicalOr_ShowsHelpfulErrorMessage()
        {
            var a = new Traceable<int>("A", 1);
            var b = new Traceable<int>("B", 2);

            var ex = Assert.Throws<InvalidOperationException>(() => { var result = a | b; });
            Assert.Contains("Operator | not supported for type Int32", ex.Message);
            Assert.Contains("ITraceableLogical<Int32>", ex.Message);
        }

        #endregion
    }
}
