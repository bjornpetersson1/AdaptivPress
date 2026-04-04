# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

- **Run**: `dotnet run` — launches the interactive console app
- **Build**: `dotnet build`
- **Clean**: `dotnet clean`

> Note: `Console.Clear()` fails when run without a real TTY (e.g. via Claude Code's Bash tool). Run the app in a proper terminal.

## Architecture

Multi-file console app targeting .NET 10.0. No external dependencies. Namespace: `CustomPress`.

### Project structure

```
Program.cs                  — UI loop (input, display, verification)
Models/
  Rule.cs                   — record Rule(Func<double,double> Transform, string Description)
  CompressedItem.cs         — StartIndex, StartValue, Count, RuleIndex
  RepeatingBlock.cs         — StartIndex, BlockLength, RepeatCount, Block
  CompressionResult.cs      — aggregates all compression output
Compression/
  Compressor.cs             — orchestrates rule + block compression
  RuleFinder.cs             — discovers rules from triplet scanning
  BlockFinder.cs            — finds repeating blocks
  Decompressor.cs           — reconstructs original sequence
```

### Algorithm

1. **Input** — user enters doubles one at a time; `E` ends input
2. **Rule discovery** (`RuleFinder`) — scans consecutive triplets for:
   - Additive: `x + n`
   - Multiplicative: `x * n`
   - Squaring: `x^2`, Cubing: `x^3`
   - Square root: `sqrt(x)`, Reciprocal: `1/x`
   - Natural log: `ln(x)`, Exponential: `e^x`
   - Affine: `m*x + k`
3. **Block discovery** (`BlockFinder`) — finds repeating sub-sequences (min length 2, min 2 repetitions); non-overlapping, greedy by savings
4. **Compression** (`Compressor`) — applies rules (min run of 3) to indices not already covered by blocks; outputs `CompressionResult`
5. **Verification** (`Decompressor`) — reconstructs original and checks against input

### Key types

- `Rule` record: `Func<double, double>` transform + description string
- `CompressedItem`: `StartIndex`, `StartValue`, `Count`, `RuleIndex` (index into `DiscoveredRules`)
- `RepeatingBlock`: `StartIndex`, `BlockLength`, `RepeatCount`, `Block` (the repeated values)
- `CompressionResult`: `DiscoveredRules`, `Compressed`, `RepeatingBlocks`, `UncompressedValues`, `RuleMatches`, `CoveredIndices`

UI strings are in Swedish.
