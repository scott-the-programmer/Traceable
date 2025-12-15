# EntityTrace

A .NET library for creating traceable computations where every value maintains its origin and computation history.

## Overview

EntityTrace enables developers to wrap primitive values in traceable entities that support natural arithmetic syntax while automatically tracking dependencies, providing audit trails, and generating visual dependency graphs. Every operation on a traceable entity creates a new entity that remembers how it was computed, allowing you to understand the complete lineage of any calculated value.

## Features

- **Natural Syntax**: Use standard operators (+, -, \*, /, <, >, ==, &, |) with full IntelliSense support
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

## Real-World Examples

### 1. RPG Damage System

Build dynamic game systems where stats automatically propagate to derived values like damage or health.

```csharp
using EntityTrace;

// 1. Define base character stats
var strength = new Traceable<int>("Strength", 18);
var weaponDamage = new Traceable<int>("WeaponBase", 5);
var buff = new Traceable<int>("ActiveBuff", 0);

// 2. Define the damage mechanics
// Damage = (Strength / 2) + Weapon + Buffs
var strengthBonus = strength / new Traceable<int>("StrScale", 2);
var totalDamage = strengthBonus + weaponDamage + buff;

totalDamage.Description = "Total Hit Damage";

// 3. Initial Combat State
Console.WriteLine($"Hit: {totalDamage.Resolve()}");
// Result: (18/2) + 5 + 0 = 14

// 4. Gameplay Event: Player activates "Rage" (Buff +5)
buff.Reset(5);

// The damage calculation automatically updates
Console.WriteLine($"Rage Hit: {totalDamage.Resolve()}");
// Result: (18/2) + 5 + 5 = 19

// 5. Gameplay Event: Player equips "Greatsword" (Weapon 12)
weaponDamage.Reset(12);

Console.WriteLine($"Greatsword Hit: {totalDamage.Resolve()}");
// Result: (18/2) + 12 + 5 = 26
```

### 2. Profit Analysis & Troubleshooting

Use `Reset` to perform "what-if" analysis on business logic. Perfect for troubleshooting unexpected results by isolating variables.

```csharp
// 1. Define the Financial Model
var revenue = new Traceable<decimal>("Revenue", 10000m);
var cogs = new Traceable<decimal>("COGS", 8500m); // Cost of Goods Sold
var taxRate = new Traceable<decimal>("TaxRate", 0.20m);
var fixedCosts = new Traceable<decimal>("Overhead", 1000m);

var grossProfit = revenue - cogs;
var tax = grossProfit * taxRate;
var netIncome = grossProfit - tax - fixedCosts;

// 2. Analyze Initial Performance
Console.WriteLine($"Net Income: {netIncome.Resolve()}");
// Calculation: (10000 - 8500) = 1500 Gross
// Tax: 1500 * 0.20 = 300
// Net: 1500 - 300 - 1000 = 200
// Result: 200 (Too low!)

// 3. Troubleshooting: "What if we negotiate better material costs?"
// Reducing COGS to 6000
cogs.Reset(6000m);

Console.WriteLine($"Scenario A (Cheaper Materials): {netIncome.Resolve()}");
// New Gross: 4000
// New Tax: 800
// New Net: 4000 - 800 - 1000 = 2200

// 4. Analyze the dependencies for the new scenario
netIncome.PrintConsole();
/* Output:
Revenue - COGS - (Revenue - COGS) * TaxRate - Overhead = 2200.00
├── Revenue - COGS - (Revenue - COGS) * TaxRate = 3200.00
│   ├── Revenue - COGS = 4000
│   │   ├── Revenue = 10000
│   │   └── COGS = 6000
│   └── (Revenue - COGS) * TaxRate = 800.00
│       ├── Revenue - COGS = 4000 ...
│       └── TaxRate = 0.20
└── Overhead = 1000
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
```

### Operations and Dependencies

All operations create new traceable entities that remember their computation:

```csharp
var basePrice = new Traceable<decimal>("BasePrice", 100.00m);
var taxRate = new Traceable<decimal>("TaxRate", 0.08m);
var discount = new Traceable<decimal>("Discount", 10.00m);

var tax = basePrice * taxRate;
var total = basePrice + tax - discount;

Console.WriteLine(total.Dependencies);
// Output: BasePrice + BasePrice * TaxRate - Discount
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
var isGreater = a > b;  // Returns Traceable<bool>
```

### Boolean Type

- **Logical**: `&` (AND), `|` (OR)
- **Comparison**: `==`, `!=`

```csharp
var isActive = new Traceable<bool>("IsActive", true);
var hasLicense = new Traceable<bool>("HasLicense", true);
var canDrive = isActive & hasLicense;
```

### String Type

- **Concatenation**: `+`
- **Comparison**: `==`, `!=`

```csharp
var first = new Traceable<string>("First", "John");
var last = new Traceable<string>("Last", "Doe");
var full = first + last;
```

### Custom Types

EntityTrace supports custom types through optional operator interfaces. Implement the interfaces that match the operations your type supports:

#### Custom Type Example: Money

```csharp
public struct Money : ITraceableArithmetic<Money>, IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }
    // Implementation of Add, Subtract, etc...
}

// Usage
var price = new Traceable<Money>("Price", new Money(100m, "USD"));
var tax = new Traceable<Money>("Tax", new Money(20m, "USD"));
var total = price + tax;
```

## Advanced Usage

### Complex Computation Chains

```csharp
var revenue = new Traceable<decimal>("Revenue", 10000m);
var cogs = new Traceable<decimal>("COGS", 6000m);
var expenses = new Traceable<decimal>("OpEx", 2000m);

var grossProfit = revenue - cogs;
var netIncome = grossProfit - expenses;

netIncome.PrintConsole();
/* Output:
Revenue - COGS - OpEx = 2000.00
├── Revenue - COGS = 4000.00
│   ├── Revenue - COGS = 4000
│   │   ├── Revenue = 10000
│   │   └── COGS = 6000
│   └── OpEx = 2000
*/
```

## API Reference

### Core Interface

```csharp
public interface ITraceable<T>
{
    string Name { get; }
    T Value { get; }
    string Dependencies { get; }
    T Resolve();
    void Reset(T newValue);
}
```

## Requirements

- **.NET Standard 2.0** or higher
- Compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+

## Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
