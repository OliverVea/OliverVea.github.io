---
title: "Static Code Generation for Game Data in .NET"
date: "2024-11-02"
author: "Oliver Vea"
tags:
    - .NET
    - Source Generation
    - CSV
---

## Use of Tabular Data in Game Development

Recently, I've been developing a game with RPG elements. Naturally, this means that there are hundreds of stat values for abilities, characters, items, enemies, and so on.

When doing game development, the roles of developers and game designers have distinct areas of responsibility. The developer will write the systems required to make the game run, while the game designer will design and tweak the systems and the various values of the game. 

For example, a game designer might want an overview of statistics for the various spells in the game. The following table in a spreadsheet might be a good way for the game designer to work with spell values:

| Spell        | Damage | Cost |
|:------------:|:------:|:----:|
| Fireball     | 15     | 15   |
| Frostbolt    | 12     | 20   |

The table can be easily analysed for e.g. Damage/Cost, Damage/Second, grouping spells into power tiers for analysing level progression, and so on.

To use the values in the game systems, the spreadsheet will have to be deserialized into `Spell` objects.
Without code generation, the CSV parsing would have to be done at runtime, which can be very problematic in several ways.

## The Problem with Runtime CSV Parsing

The naïve approach to integrating the tabular data into the game code would be to have e.g. CSV data included in the shipped game data. The data can be parsed at runtime either during game startup or when loading a scene where the data is needed. This comes with various issues:

**1. Runtime parsing of CSV files requires slow disk access from the operating system**

As well as the overhead of parsing CSV files, relying on slow disk access are a cause of performance concerns, both in terms of memory and CPU.

This might increase startup and/or loading times of the game, especially on slower devices, such as consoles and mobile devices. In the worst case, unnecessary heap allocations could cause micro stutters in the game from garbage allocation.

**2. The files will increase the size of the shipped game data**

If compressed, the files must be decompressed first before being read by the game.

It is not possible to automatically trim unused data from the game as the compiler does not have any knowledge of the data contained in spreadsheet files.

**3. Files might not be found**

Files might not be found for any of the following reasons:

   1. The file has been renamed without updating the code.
   2. The game code does not properly translate the file to other operating systems.
   3. The file is simply missing.

If a required file cannot be found at runtime, the game will certainly throw an exception, which, in the worst case, might crash the application.

## Introducing: Source Generation

Source generation is an important part of modern .NET development. It allows a developer to write a generator or use existing, optimized generators for various tasks, such as:

- [Performant json serialization](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-8-0)
- [DI registration](https://github.com/pakrym/jab)
- [Many other tasks](https://github.com/topics/source-generators)

In many cases it can be an improvement on existing reflection-based solutions, also allowing for trimming of assemblies, greatly reducing the size of published binary files. See [the official Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/#source-generators) for a more comprehensive description of what code generation is and how to use it.

Source generation is a complex topic and has seen recent additions. This article will not dive into details but will merely implement a source generator for a practical application.

## Solving the Problem with a Source Generator

For the use case stated earlier, source generation can alleviate all of these issues and brings with it additional huge benefits to the quality and cohesiveness of game code.

During builds, a source generator will take      files and generate corresponding C# source files which can be accessed directly in the source code of the game. When the CSV files are updated, the source generator will automatically update the generated source code.

This means that the data from the spreadsheets are directly hardcoded into the game, meaning that after a build, the original spreadsheet files are no longer required. The data can simply be statically accessed from anywhere in the game, bypassing the performance requirements and negating any chance at runtime exceptions from missing files.

<!-- Unit tests -->

## Implementation

The complete source code for the implementation, examples and tests can be found in [the Github repository](https://github.com/OliverVea/CSVSourceGeneration).
The code is based on [an article](https://devblogs.microsoft.com/dotnet/new-c-source-generator-samples/) by Luca Bolognese.

The [**`CsvGenerator`**](https://github.com/OliverVea/CSVSourceGeneration/blob/master/src/CSVGenerator.cs) class is the entry point of the source generation. It enumerates CSV files, parsing each into source code and writes the source code as additional files.

The [**`CsvParser`**](https://github.com/OliverVea/CSVSourceGeneration/blob/master/src/CSVParser.cs) class is used to parse CSV text into a `CSVDocument`.

The [**`CsvDocument`**](https://github.com/OliverVea/CSVSourceGeneration/blob/master/src/CSVDocument.cs) class is a dynamic representation of a CSV file. It contains the list of columns, named [`CsvProperty`](https://github.com/OliverVea/CSVSourceGeneration/blob/master/src/CSVProperty.cs), in the CSV files, which will be accessed as properties in the resulting class.

The [**`CsvTemplate.tt`**](https://github.com/OliverVea/CSVSourceGeneration/blob/master/src/CSVTemplate.tt) t4 template reads the `CsvDocument`, using the properties and rows. It creates a static parent class, an immutable child class with properties corresponding to each column. Lastly, it creates a `static readonly` instance of the child class for each data row, allowing for direct access to the CSV data.

T4 templates, while slowly becoming and outdated technology, are nifty for generating text based on dynamic data as they are easy to understand and edit. You can read more about them in [the official Microsoft documentation](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022#run-time-t4-text-templates).

Please note that due to a lack of support for T4 and increased performance, among other benefits, lots of modern source generation code instead use `StringBuilder` to compose the source code string.

## Code Examples

The final result of the source generation is the following static class generated from the earlier tabular data:

{{< code language="csharp" source="posts/csv-source-generation/Csv_Example.g.cs" >}}

It can then be statically accessed and used in game logic, as in the following snippet of a simple spell execution method:

{{< code language="csharp" source="posts/csv-source-generation/ExecuteSpell.cs" >}}

The source generator also supports multiple different data types such as `int`, `float`, `bool`, `string` and `TimeSpan`, and can easily be modifier and expanded to support additional data types.

For example, when a value ends with `s`, the resulting column type is `TimeSpan`, allowing the user to specify a cooldown of `2.5s` which will be converted into `TimeSpan.FromSeconds(2.5)`. This has obvious benefits as a simple `float` value from a CSV file could be minutes, seconds or even milliseconds, but a `TimeSpan` has an implicit time scale, ensuring that calculations are always in the correct unit.

## What about your project?

The source generator from this project is available as [a nuget package](https://www.nuget.org/packages/CSVSourceGeneration). To use it, simply add a package reference to the project the files should be generated in:

{{< code language="xml" source="posts/csv-source-generation/PackageReference.csproj" >}}

Files included in the `.csproj` file will be converted into static classes:

{{< code language="xml" source="posts/csv-source-generation/CsvInclude.csproj" >}}

The above snippet includes `CSVFiles/MyFile.csv` and will generate a `public static class MyFile` based on the content of the file.

Please note that this source generator has been developed for demonstration purposes. The author does not guarantee quality other than very simple use cases.

## Additional benefits

Source generation can bring some additional benefits to your project as the data is available as source code at runtime. For example, invalid data and missing values will be immediately obvious upon a build. If a designer inputs an incorrect value, the first build in any CI/CD pipeline will make it obvious that something is wrong and needs to be fixed. In other words, source generation allows your CI/CD pipeline to fail early.

In addition to this, it is easier to integrate your data into unit testing. You can also get a meaningful coverage statistic of your data, not just your code!

## Conclusion

In conclusion, source generation brings lots of benefits to your game:

1. Enforce type safety in your data access
2. Boost your data access performance
3. Increase reliability and reduce game crashes
4. Allowing automatic processes, such as CI/CD, to ensure game quality

For developers looking to alleviate these issues as well as gaining several auxiliary benefits, I encourage you to explore source generation as a potential solution.                       
