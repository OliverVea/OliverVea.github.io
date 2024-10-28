---
title: "Code Generation from csv file with .NET"
date: "2024-10-26"
author: "Oliver Vea"
tags:
    - .NET
    - Source Generation
    - CSV
---

## Setting the stage

Recently, I've been developing a game with RPG elements. Naturally, this means that there are hundreds of stat values for abilities, characters, items, enemies, and so on.

In the world of game development it can be a good idea to store this kind of data in spreadsheets as it allows for versatile access to the values for analysis, tuning and easy extension.

Without source generation, csv files have significant downsides. Firstly, to read a csv file at runtime, it must be available. If the file cannot be found, it will throw an error. This can show up in unfortunate situations as a `dotnet build` doesn't necessarily mean that it is possible to run the game, as vital csv files might be missing.

In addition to this, reading and parsing csv files at runtime also has loading time and performance implications. For most use cases it's probably not noticable, but it's something to keep track of, as it is not free.

For a game designer, depending on how spells are designed, the following table might be a good way to work with spell values:

| Spell        | Damage | Range | Cost |
| ------------ | ------ | ----- | ---- |
| Fireball     | 15     | 5     | 15   |
| Frostbolt    | 12     | 5     | 20   |
| Heavy Strike | 20     | 0.7   | 5    |

As the values of the spells are the domain of the game designer, the programmer does not care about the values themselves and the values do not need to be editable at runtime. Rather, we want type safety, reliability, and performance. Therefore we might prefer the following structure:

```csharp
public static class Spells
{
    public class Spell
    {
        public int Damage { get; }
        public float Range { get; }
        public int Cost { get; }

        public Spell(int damage, float range, int cost) {
            Damage = damage;
            Range = range;
            Cost = cost;
        }
    }    

    public static readonly Spell Fireball = new(15, 5, 15);
    public static readonly Spell Frostbolt = new(12, 5, 20);
    public static readonly Spell HeavyStrike = new(20, 0.7f, 5);
}
```

Now while the structure is different, the content of these representations are the same. It is possible to map the first representation to the second one, which is exactly what we will do in this article.

## Introducing technologies

The idea for this article was derived from an article, [[1](#references)], on source generation by Luca Bolognese. I've made two key modifications to the example:

1. A simple but unoptimized parser for the csv file instead of the nuget package used in the article.
2. A run time t4 template, [[2](#references)], to generate the C# code.

### Source generation


### T4 templates


## References

1. [Luca Bolognese - New C# Source Generator Samples](https://devblogs.microsoft.com/dotnet/new-c-source-generator-samples/)
2. [Code Generation and T4 Text Templates](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022#run-time-t4-text-templates)