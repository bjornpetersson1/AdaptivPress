# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

- **Run**: `dotnet run` — launches the interactive console app
- **Build**: `dotnet build`
- **Clean**: `dotnet clean`

> Note: `Console.Clear()` fails when run without a real TTY (e.g. via Claude Code's Bash tool). Run the app in a proper terminal.

## Architecture

Single-file console app (`Program.cs`) targeting .NET 10.0. No external dependencies.

**Algorithm (all in `Program.cs`):**
1. **Input** — user enters doubles one at a time; `E` ends input
2. **Pattern discovery** — scans consecutive triplets for four rule types:
   - Additive: `x + n`
   - Multiplicative: `x * n`
   - Squaring: `x * x`
   - Square root: `sqrt(x)`
3. **Compression** — stores `(startIndex, startValue, count, Rule)` instead of each element
4. **Verification** — unpacks compressed data and compares to original

**Key types:**
- `Rule` record: holds a `Func<double, double>` transform + description string
- `CompressedItem` class: `StartIndex`, `StartValue`, `Count`, `Rule`

UI strings are in Swedish.
