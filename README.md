# Traceable

A .NET library that wraps values so you can trace where they came from and how they were calculated.

```bash
dotnet add package Traceable
```

## Usage

### 1. Arithmetic Chains

Track how computed values flow through math operations. See the full dependency tree.

```csharp
var a = new Traceable<int>(10, "A");
var b = new Traceable<int>(5, "B");
var result = (a + b) * new Traceable<int>(2, "C");

Console.WriteLine(result.Dependencies);  // (A + B) * C
```

### 2. Incorperate decisions into your calculations

Values created inside a scope automatically depend on the condition.

```csharp
var ready = new Traceable<bool>(true, "IsReady");
var steady = new Traceable<bool>(true, "IsSteady");

using ((ready && steady).AsScope())
{
    var capacity = new Traceable<int>(100, "Go!!");
    Console.WriteLine(config.Dependencies);  // Go!! (when IsReady & IsSteady)
}
```

### 3. Live Reload

Easily explore "what-if" scenarios by amending values within the chain

```csharp
var damage = new Traceable<int>(10, "BaseDamage");
var hit = damage * new Traceable<int>(2, "Multiplier");

Console.WriteLine(hit.Resolve());  // 20

damage.Reload(50);
Console.WriteLine(hit.Resolve());  // 100
```

### 4. Transform Traceables into different types

```csharp
// Transform a traceable to a different type
var damage = new Traceable<int>(10, "BaseDamage");
var crit = new Traceable<decimal>(1.2m, "CritModifier");
var message = (damage * crit).Transform(d => $"You dealt {d} damage!", "Message");

Console.WriteLine(message.Resolve());       // You dealt 12 damage!
Console.WriteLine(message.Dependencies);    // Message(CritHit(BaseDamage, CritModifier))

// Split a traceable into multiple outputs
var damage = new Traceable<int>(10, "BaseDamage");
var (fireDamage, posionDamage) = damage.Split(d => (d * 0.1m, d * 0.2m), "Fire", "Poison");
```

---

## Example

### RPG Damage System

```csharp
var strength = new Traceable<int>(18, "Strength");
var weaponDamage = new Traceable<int>(5, "WeaponBase");
var buff = new Traceable<int>(0, "ActiveBuff");

var strengthBonus = strength / new Traceable<int>(2, "StrScale");
var totalDamage = strengthBonus + weaponDamage + buff;

Console.WriteLine($"Hit: {totalDamage.Resolve()}");  // 14

// Comparisons return Traceable<bool>
var critThreshold = new Traceable<int>(20, "CritThreshold");
var isEnraged = new Traceable<bool>(false, "IsEnraged");

var overThreshold = totalDamage > critThreshold;
var isCritical = overThreshold & isEnraged;

Console.WriteLine($"Is Critical: {isCritical.Resolve()}");  // False

// Player activates "Rage" and equips "Greatsword"
buff.Reload(5);
weaponDamage.Reload(12);
isEnraged.Reload(true);

Console.WriteLine($"Damage: {totalDamage.Resolve()}");      // 26
Console.WriteLine($"Is Critical: {isCritical.Resolve()}");  // True
Console.WriteLine(isCritical.Dependencies);
// Strength / StrScale + WeaponBase + ActiveBuff > CritThreshold & IsEnraged

// Calculate a battle message string (Traceable<string>) from the damage
var playerName = new Traceable<string>("Hero", "Name");
var message = playerName + new Traceable<string>(" hits for ", "Msg")
            + totalDamage.Transform(d => d.ToString(), "Format");

Console.WriteLine(message.Resolve()); // Hero hits for 26
```

### Scoped Dependencies

Values created inside a scope automatically track the scope condition:

```csharp
var inCombat = new Traceable<bool>(true, "InCombat");

using (inCombat.AsScope())
{
    var adrenaline = new Traceable<int>(50, "Adrenaline");
    var focus = new Traceable<int>(30, "Focus");
    var bonus = adrenaline + focus;

    Console.WriteLine(adrenaline.Dependencies);  // Adrenaline (when InCombat)
    Console.WriteLine(bonus.Dependencies);       // Adrenaline (when InCombat) + Focus (when InCombat)

    // Reload still works on scoped values
    adrenaline.Reload(75);
    Console.WriteLine(bonus.Resolve());  // 105
}
```

## License

MIT
