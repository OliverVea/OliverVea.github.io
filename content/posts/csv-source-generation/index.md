---
title: "Static Code Generation for Game Data in .NET"
date: "2024-10-26"
author: "Oliver Vea"
tags:
    - .NET
    - Source Generation
    - CSV
---

## Use of Tabular Data in Game Development

Recently, I've been developing a game with RPG elements. Naturally, this means that there are hundreds of stat values for abilities, characters, items, enemies, and so on.

For example, for a game designer, the following table in a spreadsheet might be a good way to work with spell values:

| Spell        | Damage | Cost |
| ------------ | ------ | ---- |
| Fireball     | 15     | 15   |
| Frostbolt    | 12     | 20   |

The table can be easily analysed for e.g. Damage/Cost, Damage/Second, grouping spells into power tiers for analysing level progression, and so on.

To use the values in the game systems, the spreadsheet will have to be deserialized into `Spell` objects.
Without code generation, the csv parsing would have to be done at runtime, which can be very problematic in several ways.

## The Problem with Runtime CSV Parsing

The naïve approach to integrating the tabular data into the game code would be to have e.g. csv data included in the shipped game data. The data can be parsed at runtime either during game startup or when loading a scene where the data is needed. This comes with various issues:

Firstly, as runtime parsing of csv files requires disk access from the operating system, as well as all the overhead of parsing the csv data, there are performance concerns, both in terms of memory and CPU. This will increase startup and/or loading times of the game, especially on slower devices, such as consoles and mobile devices.

Secondly, the files will increase the size of the shipped game data. If the files are compressed they will have to be decompressed before being read by the game.

Thirdly, and perhaps most importantly, files might not be found for any of the following reasons:

   1. The file has been renamed without updating the code.
   2. The game code does not properly translate the file to other operating systems.
   3. The file is simply missing.

If a required file cannot be found at runtime, the game will certainly throw an exception, which, in the worst case, might crash the application.

## Introducing: Source Generation

Source generation is an important part of modern .NET development. It allows a developer to write a generator or use existing, optimized generators for various tasks, such as performant json serialization, DI registration, or many other tasks. In many cases it can be an improvement on existing reflection-based solutions, also allowing for trimming of assemblies, greatly reducing the size of published binary files. See the official Microsoft documentation, [[1]](#references), for a more comprehensive description of what code generation is and how to use it.

Source generation is a complex topic and has seen recent additions. This article will not dive into details but will merely implement a source generator for a practical application.

## Solving the Problem with a Source Generator

For the use case stated earlier, source generation can alleviate all of these issues and brings with it additional huge benefits to the quality and cohesiveness of game code.

During builds, a source generator will take csv files and generate corresponding C# source files which can be accessed directly in the source code of the game. When the csv files are updated, the source generator will automatically update the generated source code.

This means that the data from the spreadsheets are directly hardcoded into the game, meaning that after a build, the original spreadsheet files are no longer required. The data can simply be statically accessed from anywhere in the game, bypassing the performance requirements and negating any chance at runtime exceptions from missing files.

<!-- Unit tests -->

## Implementation

The complete source code for the implementation, examples and tests can be found in the Github repository, [[2]](#references).
The code is based on an article by Luca Bolognese, [[3]](#references).

The [**`CsvGenerator`**](https://github.com/OliverVea/CsvSourceGeneration/blob/master/src/CsvGenerator.cs) class is the entry point of the source generation. It enumerates csv files, parsing each into source code and writes the source code as additional files.

The [**`CsvParser`**](https://github.com/OliverVea/CsvSourceGeneration/blob/master/src/CsvParser.cs) class is used to parse csv text into a `CsvDocument`.

The [**`CsvDocument`**](https://github.com/OliverVea/CsvSourceGeneration/blob/master/src/CsvDocument.cs) class is a dynamic representation of a csv file. It contains the list of columns, named [`CsvProperty`](https://github.com/OliverVea/CsvSourceGeneration/blob/master/src/CsvProperty.cs), in the csv files, which will be accessed as properties in the resulting class.

The [**`CsvTemplate.tt`**](https://github.com/OliverVea/CsvSourceGeneration/blob/master/src/CsvTemplate.tt) t4 template takes the `CsvDocument` and converts it into source code, creating a class from the columns of the csv file and creating a `static readonly` instance of the class for each data row.

T4 templates are nifty for generating text based on dynamic data as they are easy to understand and edit. You can read more about them in the official Microsoft documentation, [[4]](#references).

## Code Examples

The final result of the source generation is the following static class generated from the earlier tabular data:

{{< code language="csharp" source="posts/csv-source-generation/Csv_Example.g.cs" >}}

It can then be statically accessed and used in game logic, as in the following snippet of a simple spell execution method:

{{< code language="csharp" source="posts/csv-source-generation/ExecuteSpell.cs" >}}

The source generator also supports multiple different data types such as `int`, `float`, `bool`, `string` and `TimeSpan`, and can easily be modifier and expanded to support additional data types.

For example, when a value ends with `s`, the resulting column type is `TimeSpan`, allowing the user to specify a cooldown of `2.5s` which will be converted into `TimeSpan.FromSeconds(2.5)`. This has obvious benefits as a simple `float` value from a csv file could be minutes, seconds or even milliseconds, but a `TimeSpan` has an implicit time scale, ensuring that calculations are always in the correct unit.

### What about your project?

The source generator from this project is available as a nuget package [[5]](#references). To use it, simply add a package reference to the project the files should be generated in:

{{< code language="xml" source="posts/csv-source-generation/PackageReference.csproj" >}}

Files included in the `.csproj` file will be converted into static classes:

{{< code language="xml" source="posts/csv-source-generation/CsvInclude.csproj" >}}

The above snippet includes `CsvFiles/MyFile.csv` and will generate a `public static class MyFile` based on the content of the file.

Please note that this source generator has been developed for demonstration purposes. The author does not guarantee quality other than very simple use cases.

### Additional benefits

* Static analysis of invalid data, missing values, and so on
* Unit testing + coverage

### Conclusion

## References

1. [The .NET Compiler Platform SDK - Source generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/#source-generators)
2. [Github repository](https://github.com/OliverVea/CsvSourceGeneration)
3. [Luca Bolognese - New C# Source Generator Samples](https://devblogs.microsoft.com/dotnet/new-c-source-generator-samples/)
4. [Code Generation and T4 Text Templates](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022#run-time-t4-text-templates)
5. [CsvSourceGeneration nuget package](https://www.nuget.org/packages/CsvSourceGeneration)