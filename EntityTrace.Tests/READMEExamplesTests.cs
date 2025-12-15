using System;
using System.Linq;
using Xunit;

namespace EntityTrace.Tests
{
    /// <summary>
    /// Tests that verify all examples from the README work correctly.
    /// </summary>
    public class READMEExamplesTests
    {
        [Fact]
        public void QuickStart_Example_ShouldWork()
        {
            // From README Quick Start section
            var c = new Traceable<int>("C", 1);
            var d = new Traceable<int>("D", 2);
            var z = new Traceable<int>("Z", 1);

            var a = c + d - z;
            a.Description = "Total calculation";

            // Verify value
            Assert.Equal(2, a.Resolve());

            // Verify dependencies
            Assert.Equal("C + D - Z", a.Dependencies);

            // Verify graph structure
            var graph = a.BuildGraph();
            Assert.Equal("Total calculation", graph.Description);
            Assert.Equal(2, graph.Value);
            Assert.Equal(2, graph.Children.Count);

            // First child is C + D
            var sumChild = graph.Children[0];
            Assert.Equal("+", sumChild.Operation);
            Assert.Equal("C", sumChild.Children[0].Name);
            Assert.Equal(1, sumChild.Children[0].Value);
            Assert.Equal("D", sumChild.Children[1].Name);
            Assert.Equal(2, sumChild.Children[1].Value);

            // Second child is Z
            Assert.Equal("Z", graph.Children[1].Name);
            Assert.Equal(1, graph.Children[1].Value);
        }

        [Fact]
        public void DependencyGraph_Example_ShouldWork()
        {
            // From README "Dependency Graphs" section
            var hours = new Traceable<decimal>("Hours", 40m);
            var rate = new Traceable<decimal>("HourlyRate", 25.00m);
            var bonus = new Traceable<decimal>("Bonus", 100.00m);

            var base_pay = hours * rate;
            var total_pay = base_pay + bonus;
            total_pay.Description = "Weekly Compensation";

            // Verify value
            Assert.Equal(1100.00m, total_pay.Resolve());

            // Verify graph structure
            var graph = total_pay.BuildGraph();
            Assert.Equal("Weekly Compensation", graph.Description);
            Assert.Equal(1100.00m, graph.Value);
            Assert.Equal(2, graph.Children.Count);

            // First child is base_pay (hours * rate)
            var basePay = graph.Children[0];
            Assert.Equal("*", basePay.Operation);
            Assert.Equal(1000.00m, basePay.Value);
            Assert.Equal("Hours", basePay.Children[0].Name);
            Assert.Equal(40m, basePay.Children[0].Value);
            Assert.Equal("HourlyRate", basePay.Children[1].Name);
            Assert.Equal(25.00m, basePay.Children[1].Value);

            // Second child is bonus
            Assert.Equal("Bonus", graph.Children[1].Name);
            Assert.Equal(100.00m, graph.Children[1].Value);
        }

        [Fact]
        public void DynamicUpdates_Example_ShouldWork()
        {
            // From README "Dynamic Updates with Reset" section
            var units = new Traceable<decimal>("Units", 10m);
            var price_per_unit = new Traceable<decimal>("PricePerUnit", 5.00m);

            var total = units * price_per_unit;
            Assert.Equal(50.00m, total.Resolve());

            // Update the base value
            units.Reset(20m);
            Assert.Equal(100.00m, total.Resolve()); // Automatically recalculated
        }

        [Fact]
        public void NumericOperations_Example_ShouldWork()
        {
            // From README "Supported Types and Operations" section
            var a = new Traceable<int>("A", 10);
            var b = new Traceable<int>("B", 5);

            var sum = a + b;
            var difference = a - b;
            var product = a * b;
            var quotient = a / b;
            var is_greater = a > b;

            Assert.Equal(15, sum.Resolve());
            Assert.Equal(5, difference.Resolve());
            Assert.Equal(50, product.Resolve());
            Assert.Equal(2, quotient.Resolve());
            Assert.True(is_greater.Resolve());
        }

        [Fact]
        public void BooleanOperations_Example_ShouldWork()
        {
            // From README "Boolean Type" section
            var is_active = new Traceable<bool>("IsActive", true);
            var has_permission = new Traceable<bool>("HasPermission", true);

            var can_proceed = is_active & has_permission;
            Assert.True(can_proceed.Resolve());
        }

        [Fact]
        public void StringConcatenation_Example_ShouldWork()
        {
            // From README "String Type" section
            var first = new Traceable<string>("FirstName", "John");
            var last = new Traceable<string>("LastName", "Doe");

            var full_name = first + last;
            Assert.Equal("JohnDoe", full_name.Resolve());
        }

        [Fact]
        public void ConditionalLogic_Example_ShouldWork()
        {
            // From README "Conditional Logic with Booleans" section
            var age = new Traceable<int>("Age", 25);
            var minimum_age = new Traceable<int>("MinAge", 18);
            var has_license = new Traceable<bool>("HasLicense", true);

            var is_adult = age >= minimum_age;
            var can_drive = is_adult & has_license;

            Assert.True(can_drive.Resolve());
            Assert.Equal("Age >= MinAge & HasLicense", can_drive.Dependencies);
        }

        [Fact]
        public void GetDependencyNames_Example_ShouldWork()
        {
            // From README "Operations and Dependencies" section
            var base_price = new Traceable<decimal>("BasePrice", 100.00m);
            var tax_rate = new Traceable<decimal>("TaxRate", 0.08m);
            var discount = new Traceable<decimal>("Discount", 10.00m);

            var tax = base_price * tax_rate;
            var total = base_price + tax - discount;

            Assert.Equal(98.00m, total.Resolve());
            Assert.Equal("BasePrice + BasePrice * TaxRate - Discount", total.Dependencies);

            var dependencies = total.GetDependencyNames().ToList();
            Assert.Contains("BasePrice", dependencies);
            Assert.Contains("TaxRate", dependencies);
            Assert.Contains("Discount", dependencies);
            Assert.Equal(3, dependencies.Count);
        }

        [Fact]
        public void ComplexComputation_Example_ShouldWork()
        {
            // Test a complex financial computation
            var revenue = new Traceable<decimal>("Revenue", 10000m);
            var cogs = new Traceable<decimal>("COGS", 6000m);
            var operating_expenses = new Traceable<decimal>("OpEx", 2000m);
            var tax_rate = new Traceable<decimal>("TaxRate", 0.21m);

            var gross_profit = revenue - cogs;
            var ebit = gross_profit - operating_expenses;
            var tax = ebit * tax_rate;
            var net_income = ebit - tax;

            net_income.Description = "Net Income";

            Assert.Equal(1580.00m, net_income.Resolve());

            // Verify all dependencies are tracked
            var deps = net_income.GetDependencyNames().ToList();
            Assert.Contains("Revenue", deps);
            Assert.Contains("COGS", deps);
            Assert.Contains("OpEx", deps);
            Assert.Contains("TaxRate", deps);
        }
    }
}
