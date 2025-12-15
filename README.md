# EntityTrace

A .NET library for creating traceable computations where every value maintains its origin and computation history.

## Overview

EntityTrace enables developers to wrap primitive values in traceable entities that support natural arithmetic syntax while automatically tracking dependencies, providing audit trails, and generating visual dependency graphs. Every operation on a traceable entity creates a new entity that remembers how it was computed, allowing you to understand the complete lineage of any calculated value.

## Features

- **Natural Syntax**: Use standard operators (+, -, *, /, <, >, ==, &, |) with full IntelliSense support
- **Automatic Dependency Tracking**: Every computation automatically records its inputs and operations
- **Visual Dependency Graphs**: Generate tree-structured visualizations of computation flows
- **Multiple Type Support**: Built-in support for integers, decimals, doubles, booleans, and strings
- **Extensible Architecture**: Implement `ITraceable<T>` to add custom types
- **Lazy Evaluation**: Computed values are calculated on-demand with efficient propagation
- **Audit Trail**: Complete history of how values are derived from base entities

## Installation

```bash
dotnet add package EntityTrace
```

## Quick Start

```csharp
using EntityTrace;

// Create base entities
var c = new Traceable<int>("C", 1);
var d = new Traceable<int>("D", 2);
var z = new Traceable<int>("Z", 1);

// Perform computations with natural syntax
var a = c + d - z;
a.Description = "Total calculation";

// Resolve to get the final value
Console.WriteLine(a.Resolve());  // Output: 2

// View the dependency expression
Console.WriteLine(a.Dependencies);  // Output: C + D - Z

// Visualize the computation graph
a.PrintConsole();
/* Output:
Total calculation = 2
├── C = 1
├── D = 2
└── Z = 1
*/
```

## Core Concepts

### Traceable Entities

A traceable entity wraps a value and maintains metadata about its origin:

```csharp
var price = new Traceable<decimal>("BasePrice", 100.00m);
price.Description = "Product base price";

Console.WriteLine(price.Name);        // BasePrice
Console.WriteLine(price.Value);       // 100.00
Console.WriteLine(price.Description); // Product base price
Console.WriteLine(price.Resolve());   // 100.00
```

### Operations and Dependencies

All operations create new traceable entities that remember their computation:

```csharp
var basePrice = new Traceable<decimal>("BasePrice", 100.00m);
var taxRate = new Traceable<decimal>("TaxRate", 0.08m);
var discount = new Traceable<decimal>("Discount", 10.00m);

var tax = basePrice * taxRate;
var total = basePrice + tax - discount;

Console.WriteLine(total.Resolve());      // 98.00
Console.WriteLine(total.Dependencies);   // BasePrice + BasePrice * TaxRate - Discount
Console.WriteLine(string.Join(", ", total.GetDependencyNames()));
// Output: BasePrice, TaxRate, Discount
```

### Dependency Graphs

Visualize complex computations as tree structures:

```csharp
var hours = new Traceable<int>("Hours", 40);
var rate = new Traceable<decimal>("HourlyRate", 25.00m);
var bonus = new Traceable<decimal>("Bonus", 100.00m);

var basePay = hours * rate;
var totalPay = basePay + bonus;
totalPay.Description = "Weekly Compensation";

totalPay.PrintConsole();
/* Output:
Weekly Compensation = 1100.00
├── Hours * HourlyRate = 1000.00
│   ├── Hours = 40
│   └── HourlyRate = 25.00
└── Bonus = 100.00
*/
```

### Dynamic Updates with Reset

Update base entity values and see changes propagate through computations:

```csharp
var units = new Traceable<int>("Units", 10);
var pricePerUnit = new Traceable<decimal>("PricePerUnit", 5.00m);

var total = units * pricePerUnit;
Console.WriteLine(total.Resolve());  // 50.00

// Update the base value
units.Reset(20);
Console.WriteLine(total.Resolve());  // 100.00 (automatically recalculated)
```

## Supported Types and Operations

### Numeric Types (int, decimal, double)

- **Arithmetic**: `+`, `-`, `*`, `/`
- **Comparison**: `>`, `<`, `>=`, `<=`, `==`, `!=`

```csharp
var a = new Traceable<int>("A", 10);
var b = new Traceable<int>("B", 5);

var sum = a + b;
var difference = a - b;
var product = a * b;
var quotient = a / b;
var isGreater = a > b;  // Returns Traceable<bool>
```

### Boolean Type

- **Logical**: `&` (AND), `|` (OR)
- **Comparison**: `==`, `!=`

```csharp
var isActive = new Traceable<bool>("IsActive", true);
var hasPermission = new Traceable<bool>("HasPermission", true);

var canProceed = isActive & hasPermission;
Console.WriteLine(canProceed.Resolve());  // true
```

### String Type

- **Concatenation**: `+`
- **Comparison**: `==`, `!=`

```csharp
var first = new Traceable<string>("FirstName", "John");
var last = new Traceable<string>("LastName", "Doe");

var fullName = first + last;
Console.WriteLine(fullName.Resolve());  // JohnDoe
```

### Custom Types

EntityTrace supports custom types through optional operator interfaces. Implement the interfaces that match the operations your type supports:

#### Available Operator Interfaces

- **`ITraceableAddable<T>`** - Enables `+` operator
- **`ITraceableSubtractable<T>`** - Enables `-` operator
- **`ITraceableMultiplicable<T>`** - Enables `*` operator
- **`ITraceableDividable<T>`** - Enables `/` operator
- **`ITraceableLogical<T>`** - Enables `&` and `|` operators
- **`ITraceableArithmetic<T>`** - Convenience interface combining all arithmetic operations
- **`System.IComparable<T>`** - Enables `>`, `<`, `>=`, `<=` operators (standard .NET interface)

#### Example: Vector2D with Partial Arithmetic Support

```csharp
using EntityTrace;

public struct Vector2D : ITraceableAddable<Vector2D>, ITraceableSubtractable<Vector2D>
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

    public override string ToString() => $"({X}, {Y})";
}

// Usage with Traceable
var v1 = new Traceable<Vector2D>("V1", new Vector2D(1, 2));
var v2 = new Traceable<Vector2D>("V2", new Vector2D(3, 4));
var sum = v1 + v2;  // Works! Natural operator syntax

Console.WriteLine(sum.Resolve());      // (4, 6)
Console.WriteLine(sum.Dependencies);   // V1 + V2
sum.PrintConsole();
/* Output:
V1 + V2 = (4, 6)
├── V1 = (1, 2)
└── V2 = (3, 4)
*/
```

#### Example: Money with Full Arithmetic Support

```csharp
public struct Money : ITraceableArithmetic<Money>, IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public Money Subtract(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public Money Multiply(Money left, Money right)
    {
        return new Money(left.Amount * right.Amount, left.Currency);
    }

    public Money Divide(Money left, Money right)
    {
        return new Money(left.Amount / right.Amount, left.Currency);
    }

    public int CompareTo(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare different currencies");
        return Amount.CompareTo(other.Amount);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}

// Usage with Traceable
var price = new Traceable<Money>("Price", new Money(100m, "USD"));
var quantity = new Traceable<Money>("Quantity", new Money(5m, "USD"));
var discount = new Traceable<Money>("Discount", new Money(50m, "USD"));

var total = price * quantity - discount;

Console.WriteLine(total.Resolve());      // 450.00 USD
Console.WriteLine(total.Dependencies);   // Price * Quantity - Discount
total.PrintConsole();
/* Output:
Price * Quantity - Discount = 450.00 USD
├── Price * Quantity = 500.00 USD
│   ├── Price = 100.00 USD
│   └── Quantity = 5.00 USD
└── Discount = 50.00 USD
*/

// Comparison operators also work
var budget = new Traceable<Money>("Budget", new Money(400m, "USD"));
var isOverBudget = total > budget;
Console.WriteLine(isOverBudget.Resolve());  // true
```

#### Benefits of Custom Types

- **Type Safety**: Custom types enforce domain rules (e.g., currency validation)
- **Natural Syntax**: Use standard operators with your domain types
- **Flexible Implementation**: Implement only the operations that make sense for your type
- **Seamless Integration**: Works with all Traceable features (graphs, dependencies, reset)

## Advanced Usage

### Complex Computation Chains

```csharp
var revenue = new Traceable<decimal>("Revenue", 10000m);
var cogs = new Traceable<decimal>("COGS", 6000m);
var operatingExpenses = new Traceable<decimal>("OpEx", 2000m);
var taxRate = new Traceable<decimal>("TaxRate", 0.21m);

var grossProfit = revenue - cogs;
var ebit = grossProfit - operatingExpenses;
var tax = ebit * taxRate;
var netIncome = ebit - tax;

netIncome.Description = "Net Income";

netIncome.PrintConsole();
/* Output:
Net Income = 1580.00
├── Revenue - COGS - OpEx = 2000.00
│   ├── Revenue - COGS = 4000.00
│   │   ├── Revenue = 10000
│   │   └── COGS = 6000
│   └── OpEx = 2000
└── (Revenue - COGS - OpEx) * TaxRate = 420.00
    ├── Revenue - COGS - OpEx = 2000.00
    │   └── [subtree...]
    └── TaxRate = 0.21
*/
```

### Conditional Logic with Booleans

```csharp
var age = new Traceable<int>("Age", 25);
var hasLicense = new Traceable<bool>("HasLicense", true);

var isAdult = age >= 18;
var canDrive = isAdult & hasLicense;

Console.WriteLine(canDrive.Resolve());      // true
Console.WriteLine(canDrive.Dependencies);   // Age >= 18 & HasLicense
```

### Custom Type Extension

Implement `ITraceable<T>` for custom domain types:

```csharp
public class CustomTraceable<T> : ITraceable<T>
{
    public string Name { get; }
    public string Description { get; set; }
    public T Value { get; }

    public T Resolve() => Value;
    public string Dependencies => Name;
    public string Graph => $"{Description ?? Name} = {Value}";
    public IEnumerable<string> GetDependencyNames() => new[] { Name };
    public void Reset(T newValue) { /* implementation */ }
}
```

## Use Cases

### Financial Modeling
Track how financial metrics are calculated from base assumptions, providing transparency for audits and regulatory compliance.

### Scientific Computation
Document how experimental results are derived from raw measurements, creating reproducible calculation chains.

### Business Rules Engine
Visualize complex business logic as dependency graphs, making it easier to understand and debug conditional workflows.

### Configuration Management
Track how derived configuration values depend on base settings, simplifying troubleshooting of complex systems.

### Data Validation
Create traceable validation rules that show exactly which conditions failed and why.

## API Reference

### Core Interface

```csharp
public interface ITraceable<T>
{
    string Name { get; }
    string Description { get; set; }
    T Value { get; }
    string Dependencies { get; }
    string Graph { get; }

    T Resolve();
    IEnumerable<string> GetDependencyNames();
    void Reset(T newValue);
}
```

### Main Class

```csharp
public class Traceable<T> : ITraceable<T>
{
    // Constructor for base entities
    public Traceable(string name, T value);

    // Properties
    public string Name { get; }
    public string Description { get; set; }
    public T Value { get; }
    public string Dependencies { get; }
    public string Graph { get; }

    // Methods
    public T Resolve();
    public IEnumerable<string> GetDependencyNames();
    public void Reset(T newValue);

    // Operators (availability depends on type constraints)
    public static Traceable<T> operator +(Traceable<T> left, Traceable<T> right);
    public static Traceable<T> operator -(Traceable<T> left, Traceable<T> right);
    public static Traceable<T> operator *(Traceable<T> left, Traceable<T> right);
    public static Traceable<T> operator /(Traceable<T> left, Traceable<T> right);
    public static Traceable<bool> operator >(Traceable<T> left, Traceable<T> right);
    public static Traceable<bool> operator <(Traceable<T> left, Traceable<T> right);
    public static Traceable<bool> operator >=(Traceable<T> left, Traceable<T> right);
    public static Traceable<bool> operator <=(Traceable<T> left, Traceable<T> right);
    public static Traceable<bool> operator ==(Traceable<T> left, Traceable<T> right);
    public static Traceable<bool> operator !=(Traceable<T> left, Traceable<T> right);
    public static Traceable<T> operator &(Traceable<T> left, Traceable<T> right);  // bool only
    public static Traceable<T> operator |(Traceable<T> left, Traceable<T> right);  // bool only
}
```

## Requirements

- **.NET Standard 2.0** or higher
- Compatible with:
  - .NET Framework 4.6.1+
  - .NET Core 2.0+
  - .NET 5.0+
  - Mono 5.4+
  - Xamarin

## Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: Report bugs or request features via [GitHub Issues](https://github.com/yourusername/EntityTrace/issues)
- **Documentation**: Full documentation available at [docs link]
- **Examples**: See the [examples](examples/) directory for more use cases

## Acknowledgments

EntityTrace was designed to bring transparency and traceability to computational workflows, enabling developers to build more maintainable and debuggable systems.
