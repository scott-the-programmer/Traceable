using System;
using EntityTrace;

/// <summary>
/// Demo program showing the Transform feature in action.
/// Run with: dotnet run --project TransformDemo.csproj (after creating a console project)
/// </summary>
class TransformDemo
{
    static void Main()
    {
        Console.WriteLine("=== EntityTrace Transform Feature Demo ===\n");

        // Example 1: Single-input transform with type change
        Console.WriteLine("Example 1: Rounding a decimal to int");
        var price = new Traceable<double>("Price", 19.99);
        var roundedPrice = price.Transform<double, int>("Round", x => (int)Math.Floor(x));

        Console.WriteLine($"Value: {roundedPrice.Resolve()}");
        Console.WriteLine($"Dependencies: {roundedPrice.Dependencies}");
        Console.WriteLine($"Graph:\n{roundedPrice.Graph}\n");

        // Example 2: Multi-input transform
        Console.WriteLine("Example 2: Combining two values");
        var first = new Traceable<string>("FirstName", "John");
        var last = new Traceable<string>("LastName", "Doe");
        var fullName = TraceableExtensions.Transform(first, last, "Combine", (f, l) => $"{f} {l}");

        Console.WriteLine($"Value: {fullName.Resolve()}");
        Console.WriteLine($"Dependencies: {fullName.Dependencies}");
        Console.WriteLine($"Graph:\n{fullName.Graph}\n");

        // Example 3: Complex nested scenario (from user's original question)
        Console.WriteLine("Example 3: Complex computation with transform");
        var a = new Traceable<double>("A", 10.7);
        var b = new Traceable<double>("B", 5.3);
        var sum = a + b;
        var floored = sum.Transform<double, int>("Floor", x => (int)Math.Floor(x));
        var doubled = floored * new Traceable<int>("Two", 2);

        Console.WriteLine($"Value: {doubled.Resolve()}");
        Console.WriteLine($"Dependencies: {doubled.Dependencies}");
        Console.WriteLine($"Graph:\n{doubled.Graph}\n");

        // Example 4: Three-input transform
        Console.WriteLine("Example 4: Weighted average of three values");
        var val1 = new Traceable<int>("Value1", 80);
        var val2 = new Traceable<int>("Value2", 90);
        var val3 = new Traceable<int>("Value3", 70);
        var average = TraceableExtensions.Transform(val1, val2, val3, "WeightedAvg",
            (v1, v2, v3) => (v1 * 0.5 + v2 * 0.3 + v3 * 0.2));

        Console.WriteLine($"Value: {average.Resolve()}");
        Console.WriteLine($"Dependencies: {average.Dependencies}");
        Console.WriteLine($"Graph:\n{average.Graph}\n");

        // Example 5: Reset propagation
        Console.WriteLine("Example 5: Reset propagation through transform chain");
        var temperature = new Traceable<double>("TempCelsius", 25.0);
        var fahrenheit = temperature.Transform<double, double>("ToFahrenheit", c => c * 9 / 5 + 32);

        Console.WriteLine($"Initial: {fahrenheit.Resolve()}°F");

        temperature.Reset(100.0);
        Console.WriteLine($"After reset to 100°C: {fahrenheit.Resolve()}°F");
        Console.WriteLine($"Graph:\n{fahrenheit.Graph}\n");
    }
}
