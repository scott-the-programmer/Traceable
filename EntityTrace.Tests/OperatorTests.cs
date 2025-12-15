using System;
using Xunit;

namespace EntityTrace.Tests
{
    public class OperatorTests
    {
        #region Arithmetic Operators - Integer

        [Fact]
        public void Addition_Int_ShouldCreateCompositeWithCorrectValue()
        {
            var a = new Traceable<int>("A", 5);
            var b = new Traceable<int>("B", 3);

            var sum = a + b;

            Assert.Equal(8, sum.Resolve());
            Assert.Equal("A + B", sum.Dependencies);
        }

        [Fact]
        public void Subtraction_Int_ShouldWork()
        {
            var a = new Traceable<int>("A", 10);
            var b = new Traceable<int>("B", 3);

            var diff = a - b;

            Assert.Equal(7, diff.Resolve());
            Assert.Equal("A - B", diff.Dependencies);
        }

        [Fact]
        public void Multiplication_Int_ShouldWork()
        {
            var a = new Traceable<int>("A", 5);
            var b = new Traceable<int>("B", 3);

            var product = a * b;

            Assert.Equal(15, product.Resolve());
            Assert.Equal("A * B", product.Dependencies);
        }

        [Fact]
        public void Division_Int_ShouldWork()
        {
            var a = new Traceable<int>("A", 15);
            var b = new Traceable<int>("B", 3);

            var quotient = a / b;

            Assert.Equal(5, quotient.Resolve());
            Assert.Equal("A / B", quotient.Dependencies);
        }

        #endregion

        #region Arithmetic Operators - Decimal

        [Fact]
        public void Addition_Decimal_ShouldWork()
        {
            var a = new Traceable<decimal>("A", 5.5m);
            var b = new Traceable<decimal>("B", 3.2m);

            var sum = a + b;

            Assert.Equal(8.7m, sum.Resolve());
        }

        [Fact]
        public void Multiplication_Decimal_ShouldWork()
        {
            var a = new Traceable<decimal>("A", 5.5m);
            var b = new Traceable<decimal>("B", 2.0m);

            var product = a * b;

            Assert.Equal(11.0m, product.Resolve());
        }

        #endregion

        #region Arithmetic Operators - Double

        [Fact]
        public void Addition_Double_ShouldWork()
        {
            var a = new Traceable<double>("A", 5.5);
            var b = new Traceable<double>("B", 3.2);

            var sum = a + b;

            Assert.Equal(8.7, sum.Resolve(), 4); // 4 decimal places precision
        }

        #endregion

        #region String Concatenation

        [Fact]
        public void Addition_String_ShouldConcatenate()
        {
            var first = new Traceable<string>("First", "Hello");
            var last = new Traceable<string>("Last", "World");

            var full = first + last;

            Assert.Equal("HelloWorld", full.Resolve());
            Assert.Equal("First + Last", full.Dependencies);
        }

        #endregion

        #region Comparison Operators

        [Fact]
        public void GreaterThan_Int_ShouldReturnTraceableBool()
        {
            var a = new Traceable<int>("A", 10);
            var b = new Traceable<int>("B", 5);

            var result = a > b;

            Assert.IsType<Traceable<bool>>(result);
            Assert.True(result.Resolve());
            Assert.Equal("A > B", result.Dependencies);
        }

        [Fact]
        public void LessThan_Int_ShouldWork()
        {
            var a = new Traceable<int>("A", 5);
            var b = new Traceable<int>("B", 10);

            var result = a < b;

            Assert.True(result.Resolve());
        }

        [Fact]
        public void GreaterThanOrEqual_Int_ShouldWork()
        {
            var a = new Traceable<int>("A", 10);
            var b = new Traceable<int>("B", 10);

            var result = a >= b;

            Assert.True(result.Resolve());
        }

        [Fact]
        public void LessThanOrEqual_Int_ShouldWork()
        {
            var a = new Traceable<int>("A", 5);
            var b = new Traceable<int>("B", 10);

            var result = a <= b;

            Assert.True(result.Resolve());
        }

        [Fact]
        public void Equality_Int_ShouldWork()
        {
            var a = new Traceable<int>("A", 10);
            var b = new Traceable<int>("B", 10);

            var result = a == b;

            Assert.True(result.Resolve());
        }

        [Fact]
        public void Inequality_Int_ShouldWork()
        {
            var a = new Traceable<int>("A", 10);
            var b = new Traceable<int>("B", 5);

            var result = a != b;

            Assert.True(result.Resolve());
        }

        #endregion

        #region Boolean Operators

        [Fact]
        public void LogicalAnd_Bool_ShouldWork()
        {
            var a = new Traceable<bool>("A", true);
            var b = new Traceable<bool>("B", true);

            var result = a & b;

            Assert.True(result.Resolve());
            Assert.Equal("A & B", result.Dependencies);
        }

        [Fact]
        public void LogicalAnd_Bool_WithFalse_ShouldReturnFalse()
        {
            var a = new Traceable<bool>("A", true);
            var b = new Traceable<bool>("B", false);

            var result = a & b;

            Assert.False(result.Resolve());
        }

        [Fact]
        public void LogicalOr_Bool_ShouldWork()
        {
            var a = new Traceable<bool>("A", false);
            var b = new Traceable<bool>("B", true);

            var result = a | b;

            Assert.True(result.Resolve());
            Assert.Equal("A | B", result.Dependencies);
        }

        #endregion

        #region Complex Expressions

        [Fact]
        public void ComplexExpression_ShouldMaintainPrecedence()
        {
            var a = new Traceable<int>("A", 2);
            var b = new Traceable<int>("B", 3);
            var c = new Traceable<int>("C", 4);

            var result = a + b * c;

            Assert.Equal(14, result.Resolve()); // 2 + (3 * 4) = 14
            Assert.Equal("A + B * C", result.Dependencies);
        }

        [Fact]
        public void ComplexExpressionWithParentheses_ShouldWork()
        {
            var a = new Traceable<int>("A", 2);
            var b = new Traceable<int>("B", 3);
            var c = new Traceable<int>("C", 4);

            var result = (a + b) * c;

            Assert.Equal(20, result.Resolve()); // (2 + 3) * 4 = 20
            Assert.Equal("(A + B) * C", result.Dependencies);
        }

        [Fact]
        public void GetDependencyNames_ForComplexExpression_ShouldReturnAllBaseEntities()
        {
            var a = new Traceable<int>("A", 1);
            var b = new Traceable<int>("B", 2);
            var c = new Traceable<int>("C", 3);

            var result = (a + b) * c;

            Assert.Equal(new[] { "A", "B", "C" }, result.GetDependencyNames());
        }

        [Fact]
        public void Reset_ShouldPropagateToComposites()
        {
            var a = new Traceable<int>("A", 10);
            var b = new Traceable<int>("B", 5);
            var sum = a + b;

            Assert.Equal(15, sum.Resolve());

            a.Reset(20);

            Assert.Equal(25, sum.Resolve());
        }

        [Fact]
        public void Reset_OnComposite_ShouldThrow()
        {
            var a = new Traceable<int>("A", 1);
            var b = new Traceable<int>("B", 2);
            var sum = a + b;

            var ex = Assert.Throws<InvalidOperationException>(() => sum.Reset(10));
            Assert.Contains("Cannot reset a composite entity", ex.Message);
        }

        #endregion

        #region Error Cases

        [Fact]
        public void BooleanAnd_OnNonBool_ShouldThrow()
        {
            var a = new Traceable<int>("A", 1);
            var b = new Traceable<int>("B", 2);

            var ex = Assert.Throws<InvalidOperationException>(() => { var result = a & b; });
            Assert.Contains("Operator & not supported for type Int32", ex.Message);
        }

        [Fact]
        public void Subtraction_OnString_ShouldThrow()
        {
            var a = new Traceable<string>("A", "Hello");
            var b = new Traceable<string>("B", "World");

            var ex = Assert.Throws<InvalidOperationException>(() => { var result = a - b; });
            Assert.Contains("Operator - not supported for type String", ex.Message);
        }

        #endregion

        #region README Examples

        [Fact]
        public void READMEExample_QuickStart_ShouldWork()
        {
            var c = new Traceable<int>("C", 1);
            var d = new Traceable<int>("D", 2);
            var z = new Traceable<int>("Z", 1);

            var a = c + d - z;
            a.Description = "Total calculation";

            Assert.Equal(2, a.Resolve());
            Assert.Equal("C + D - Z", a.Dependencies);
        }

        [Fact]
        public void READMEExample_Reset_ShouldWork()
        {
            var units = new Traceable<decimal>("Units", 10m);
            var price_per_unit = new Traceable<decimal>("PricePerUnit", 5.00m);

            var total = units * price_per_unit;
            Assert.Equal(50.00m, total.Resolve());

            units.Reset(20m);
            Assert.Equal(100.00m, total.Resolve());
        }

        #endregion
    }
}
