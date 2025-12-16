# Traceable

Trying to simplify troubleshooting for any sort of calculation

## Overview

_This project is inspired by [Pulumi's](https://www.pulumi.com/) C# SDK._

Ever wonder how that number was calculated?

Or stare at an if statement for far too long?

Leverage `Traceable<T>` and look 

## Installation

```bash
dotnet add package Traceable
```

## Real-World Examples

### 1. RPG Damage System

Build dynamic game systems where stats automatically propagate to derived values like damage or health.

```csharp
using Traceable;

// 1. Define base character stats
var strength = new Traceable<int>(18, "Strength");
var weaponDamage = new Traceable<int>(5, "WeaponBase");
var buff = new Traceable<int>(0, "ActiveBuff");

// 2. Define the damage mechanics
// Damage = (Strength / 2) + Weapon + Buffs
var strengthBonus = strength / new Traceable<int>(2, "StrScale");
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
var revenue = new Traceable<decimal>(10000m, "Revenue");
var cogs = new Traceable<decimal>(8500m, "COGS"); // Cost of Goods Sold
var taxRate = new Traceable<decimal>(0.20m, "TaxRate");
var fixedCosts = new Traceable<decimal>(1000m, "Overhead");

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
var price = new Traceable<decimal>(100.00m, "BasePrice");
price.Description = "Product base price";

Console.WriteLine(price.Name);        // BasePrice
Console.WriteLine(price.Value);       // 100.00
Console.WriteLine(price.Description); // Product base price
```

### Operations and Dependencies

All operations create new traceable entities that remember their computation:

```csharp
var basePrice = new Traceable<decimal>(100.00m, "BasePrice");
var taxRate = new Traceable<decimal>(0.08m, "TaxRate");
var discount = new Traceable<decimal>(10.00m, "Discount");

var tax = basePrice * taxRate;
var total = basePrice + tax - discount;

Console.WriteLine(total.Dependencies);
// Output: BasePrice + BasePrice * TaxRate - Discount
```

### Dynamic Updates with Reset

Update base entity values and see changes propagate through computations:

```csharp
var units = new Traceable<int>(10, "Units");
var pricePerUnit = new Traceable<decimal>(5.00m, "PricePerUnit");

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
var a = new Traceable<int>(10, "A");
var b = new Traceable<int>(5, "B");

var sum = a + b;
var isGreater = a > b;  // Returns Traceable<bool>
```

### Boolean Type

- **Logical**: `&` (AND), `|` (OR)
- **Comparison**: `==`, `!=`

```csharp
var isActive = new Traceable<bool>(true, "IsActive");
var hasLicense = new Traceable<bool>(true, "HasLicense");
var canDrive = isActive & hasLicense;
```

### String Type

- **Concatenation**: `+`
- **Comparison**: `==`, `!=`

```csharp
var first = new Traceable<string>("John", "First");
var last = new Traceable<string>("Doe", "Last");
var full = first + last;
```

### Custom Types

Traceable supports custom types through optional operator interfaces. Implement the interfaces that match the operations your type supports:

#### Custom Type Example: Money

```csharp
public struct Money : ITraceableArithmetic<Money>, IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }
    // Implementation of Add, Subtract, etc...
}

// Usage
var price = new Traceable<Money>(new Money(100m, "USD"), "Price");
var tax = new Traceable<Money>(new Money(20m, "USD"), "Tax");
var total = price + tax;
```

## Advanced Usage

### Complex Computation Chains

```csharp
var revenue = new Traceable<decimal>(10000m, "Revenue");
var cogs = new Traceable<decimal>(6000m, "COGS");
var expenses = new Traceable<decimal>(2000m, "OpEx");

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
