# CompactValue currency

`CompactValue` is a signed immutable number: `Mantissa * 1000^Exponent`.
It expands the numeric range, keeping approximately 15–16 significant digits.
It is not an exact integer ledger: adding 1 to an enormous balance may round away,
just as with double. Use long/BigInteger when every unit must remain exact.

## Data, events and saving

```csharp
using SketchEngine.Data;
using SketchEngine.Utilities;

DataValue<CompactValue> gem = new DataValue<CompactValue>(0);
gem.OnValueChanged += OnGemChanged;
gem.Value += 100;
gem.Value *= 1.5;
if (gem.Value >= 50) gem.Value -= 50;

void OnGemChanged(CompactValue balance)
{
    text.text = UtilityNumber.Format(balance);
}
```

The setter continues firing on every assignment, including an identical value,
matching existing DataValue<double> behavior. Keep rewards, prices and event
amounts as CompactValue when using an enormous currency. Do not convert the
balance back to double for arithmetic or animation.

Save your entire DataBlock using the existing DataSerializer/DataFileHandler.
Registered Odin formatters automatically store CompactValue as a double mantissa
and an int exponent, and DataValue<CompactValue> as its value only. Runtime event
listeners are excluded. The existing BinaryFormatter branch is also supported
through [Serializable]; this does not require UtilityNumber.Save per currency.

Existing double fields may remain double and use the same Format method.
Changing an existing saved field's type from DataValue<double> to
DataValue<CompactValue> is a save-schema change, not an automatic migration.
Load the previous schema and construct CompactValue from its double value before
saving the new schema. Keep the old save until migration succeeds.

The formatter and concrete serializer constructors are retained for IL2CPP.
Game-specific wallet/dictionary types still follow your existing Odin AOT setup.
Verify a save/restart/load on your target IL2CPP build before releasing a migration.

## Numbers and display

```csharp
CompactValue small = 100;
CompactValue enormous = new CompactValue(1.5, 2000); // 1.5 * 1000^2000
CompactValue reward = CompactValue.Parse("1e6000");

UtilityNumber.Format(12.9);       // "13"
UtilityNumber.Format(1234);       // "1.23K"
UtilityNumber.Format(999999);     // "1M"
UtilityNumber.Format(enormous);   // Compact suffix, without conversion to double
```

Suffixes: K, M, B, T, a...z, aa, ab... . Uppercase K/M/B/T and
lowercase k/m/b/t are distinct. Decimal digits are clamped to 0–15;
small values round using Math.Round's default midpoint-to-even rule.
Rounded boundaries promote to the next suffix (999.999K becomes 1M).
Strings allocate; TryFormat writes into a caller-owned buffer without allocating
after initialization. ToSaveString/TryParseSave are only for optional standalone
text storage; rounded display text must not be used as a save representation.

Exponent overflow and divide-by-zero throw; NaN/Infinity inputs are rejected.
TryToDouble reports when a conversion overflows or underflows.

## Currency bar

Add CompactValueText to the currency bar, assign its TMP_Text in the inspector,
and bind the actual wallet object once:

```csharp
[SerializeField] CompactValueText bar;

void Start() => bar.Bind(Data_Currency.Get(type));
```

The component subscribes/unsubscribes as it becomes enabled/disabled. Rebind
when switching currency/wallet/minigame. It animates from the currently visible
value and reads the current balance when re-enabled. It reuses character buffers
and cached callbacks; TMP receives an update only when formatted characters change.
DOTween still allocates when creating a tween, and TMP may grow its buffers on first
use; this is not a promise of zero allocation for the complete Unity UI pipeline.

For spread rewards, pause automatic display updates BEFORE adding the reward:

```csharp
bar.SetAutoRefresh(false);
currency.Value += amount; // Gameplay balance changes immediately.
// Spawn visual coins. When the outstanding spread animations finish:
bar.SetAutoRefresh(true); // Tween to the latest real balance.
```

Track outstanding spreads in the game and resume when all complete. Coin count
is a visual setting, independent of the reward amount. Filter spread events by
currency type. Do not derive the previous balance by subtracting the reward from
the current balance when concurrent rewards or spending are possible.

For an existing Currency_Bar implementation, replace double locals with
CompactValue and use this in its float-progress tween:

```csharp
displayed = CompactValue.Lerp(start, target, progress);
text.text = UtilityNumber.Format(displayed);
```

## Validation

The source compiled against the project's real Unity/TMP/DOTween/Odin assemblies.
A standalone harness on Unity's bundled Mono passed 50,000 randomized arithmetic
and comparison checks, plus parsing, formatting, boundary/error, event, Odin nested
DataValue/dictionary roundtrips and the existing BinaryFormatter fallback.
After warm-up, 100,000 arithmetic + span-format iterations allocated 0 managed bytes
in that harness. Unity play-mode visuals and a target IL2CPP build remain separate
integration checks; the harness does not simulate TMP or DOTween native behavior.
