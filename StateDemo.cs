using System;
using System.Collections.Generic;
using EntityTrace;

namespace EntityTraceDemo
{
    class StateDemo
    {
        static void Main()
        {
            Console.WriteLine("=== EntityTrace State Feature Demo ===\n");

            // Example 1: Arbitrary State
            Console.WriteLine("1. Arbitrary State Example:");
            var revenue = new Traceable<decimal>(
                "Revenue",
                10000m,
                arbitraryState: new Dictionary<string, object>
                {
                    ["source"] = "Q4 2024",
                    ["confidence"] = 0.95,
                    ["currency"] = "USD"
                }
            );

            Console.WriteLine($"Revenue Value: {revenue.Value}");
            Console.WriteLine($"Has Arbitrary State: {revenue.HasArbitraryState}");
            Console.WriteLine($"Source: {revenue.ArbitraryState["source"]}");
            Console.WriteLine($"Confidence: {revenue.ArbitraryState["confidence"]}");
            Console.WriteLine();

            // Example 2: Value State
            Console.WriteLine("2. Value State Example:");
            var sales = new Traceable<decimal>(
                "Sales",
                8000m,
                valueState: new Dictionary<string, decimal>
                {
                    ["target"] = 10000m,
                    ["min"] = 5000m,
                    ["max"] = 15000m
                }
            );

            Console.WriteLine($"Sales Value: {sales.Value}");
            Console.WriteLine($"Has Value State: {sales.HasValueState}");
            Console.WriteLine($"Target: {sales.ValueState["target"]}");
            Console.WriteLine($"Min: {sales.ValueState["min"]}");
            Console.WriteLine($"Max: {sales.ValueState["max"]}");
            Console.WriteLine();

            // Example 3: Both States Combined
            Console.WriteLine("3. Both States Example:");
            var profit = new Traceable<decimal>(
                "Profit",
                5000m,
                arbitraryState: new Dictionary<string, object>
                {
                    ["department"] = "Engineering",
                    ["quarter"] = "Q4"
                },
                valueState: new Dictionary<string, decimal>
                {
                    ["threshold"] = 3000m,
                    ["bonus_multiplier"] = 1.5m
                }
            );

            Console.WriteLine($"Profit Value: {profit.Value}");
            Console.WriteLine($"Department: {profit.ArbitraryState["department"]}");
            Console.WriteLine($"Threshold: {profit.ValueState["threshold"]}");
            Console.WriteLine();

            // Example 4: Graph Visualization with State
            Console.WriteLine("4. Graph Visualization with State:");
            var base1 = new Traceable<decimal>(
                "Base",
                100m,
                arbitraryState: new Dictionary<string, object> { ["source"] = "database" }
            );
            var tax = new Traceable<decimal>(
                "Tax",
                50m,
                valueState: new Dictionary<string, decimal> { ["rate"] = 0.5m }
            );

            var total = base1 + tax;
            total.Description = "Total";

            Console.WriteLine(total.Graph);
            Console.WriteLine();

            // Example 5: TryGet Methods
            Console.WriteLine("5. Safe State Retrieval:");
            if (revenue.TryGetArbitraryState("source", out var source))
            {
                Console.WriteLine($"Found source: {source}");
            }

            if (sales.TryGetValueState("target", out var target))
            {
                Console.WriteLine($"Found target: {target}");
            }

            if (revenue.TryGetArbitraryState("nonexistent", out var missing))
            {
                Console.WriteLine("This won't print");
            }
            else
            {
                Console.WriteLine("Key 'nonexistent' not found (as expected)");
            }

            Console.WriteLine("\n=== Demo Complete ===");
        }
    }
}
