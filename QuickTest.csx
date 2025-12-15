#!/usr/bin/env dotnet-script
#r "EntityTrace/bin/Debug/netstandard2.0/EntityTrace.dll"

using EntityTrace;
using System;

// Your original example: transform 3.145 to 3 by rounding down
var someVar = new Traceable<double>("someVar", 3.145);
var rounded = someVar.Transform<double, int>("Round", a => (int)Math.Floor(a));

Console.WriteLine($"Original value: {someVar.Resolve()}");
Console.WriteLine($"Transformed value: {rounded.Resolve()}");
Console.WriteLine($"Dependencies: {rounded.Dependencies}");
Console.WriteLine($"\nDependency Graph:");
Console.WriteLine(rounded.Graph);

// Verify it updates when base value changes
Console.WriteLine($"\n--- After Reset ---");
someVar.Reset(9.876);
Console.WriteLine($"New original value: {someVar.Resolve()}");
Console.WriteLine($"New transformed value: {rounded.Resolve()}");
Console.WriteLine($"Dependencies: {rounded.Dependencies}");
Console.WriteLine($"\nDependency Graph:");
Console.WriteLine(rounded.Graph);
