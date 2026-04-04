using CustomPress.Compression;
using CustomPress.Models;

// ── Helpers ───────────────────────────────────────────────────────────────────

const int W = 56;
ConsoleColor wallColor = ConsoleColor.DarkCyan;

// Total line width = 1(╔) + W(═…═) + 1(╗) = W+2
// Row inner content must be W-1 chars: "║ " (2) + content (W-1) + "║" (1) = W+2 ✓
void Header(string title, ConsoleColor color)
{
    wallColor = color;
    Console.ForegroundColor = color;
    Console.WriteLine("╔" + new string('═', W) + "╗");
    Console.Write("║ ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write(title.PadRight(W - 1));
    Console.ForegroundColor = color;
    Console.WriteLine("║");
    Console.WriteLine("╠" + new string('═', W) + "╣");
    Console.ResetColor();
}

void Row(string text, ConsoleColor color = ConsoleColor.White)
{
    Console.ForegroundColor = wallColor;
    Console.Write("║ ");
    Console.ForegroundColor = color;
    string clipped = text.Length > W - 1 ? text[..(W - 4)] + "..." : text;
    Console.Write(clipped.PadRight(W - 1));
    Console.ForegroundColor = wallColor;
    Console.WriteLine("║");
    Console.ResetColor();
}

void Divider()
{
    Console.ForegroundColor = wallColor;
    Console.WriteLine("╠" + new string('═', W) + "╣");
    Console.ResetColor();
}

void Footer()
{
    Console.ForegroundColor = wallColor;
    Console.WriteLine("╚" + new string('═', W) + "╝");
    Console.WriteLine();
    Console.ResetColor();
}

void WrapRows(IEnumerable<double> values, ConsoleColor color = ConsoleColor.White)
{
    var buf = new System.Text.StringBuilder();
    foreach (var num in values)
    {
        string token = num.ToString();
        string sep   = buf.Length == 0 ? "" : "  ";
        if (buf.Length + sep.Length + token.Length > W - 1)
        {
            Row(buf.ToString(), color);
            buf.Clear();
            buf.Append(token);
        }
        else
        {
            buf.Append(sep + token);
        }
    }
    if (buf.Length > 0) Row(buf.ToString(), color);
}

do
{
    // ── Input ─────────────────────────────────────────────────────────────────────

    string userInput = string.Empty;
    var inputs = new List<double>();
    var inputMode = InputMode.Numeric;
    var inputType = 0;
    do
    {
        Console.Clear();

        Header($"INDATA  ·  {inputs.Count} tal inmatade", ConsoleColor.Cyan);
        if (inputs.Count == 0)
            Row("  (inga tal ännu)", ConsoleColor.DarkGray);
        else
            WrapRows(inputs);
        Divider();
        Row("  Ange nästa tal, eller E för att fortsätta:", ConsoleColor.DarkCyan);
        Footer();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  > ");
        Console.ResetColor();

        userInput = Console.ReadLine() ?? string.Empty;
        if (inputType == 0)
        {
            if (double.TryParse(userInput, out double result))
            {
                inputs.Add(result);
                inputType = 1;   
            }
            else if (userInput != "E")
            {
                inputMode = InputMode.Text;
                inputType = 2;
                foreach (char c in userInput)
                    inputs.Add((double)c);
            }
        }
        else if (inputType == 1)
        {
            if (double.TryParse(userInput, out double result))
                inputs.Add(result);
        }
        else if (inputType == 2 && userInput != "E")
        {
            foreach (char c in userInput)
                inputs.Add((double)c);
        }
    }
    while (userInput != "E");

    Console.Clear();

    // ── Compress ──────────────────────────────────────────────────────────────────

    var cr       = Compressor.Compress(inputs, inputMode);
    var unpacked = Decompressor.Decompress(inputs.Count, cr, inputs);

    string DoublesToString(IEnumerable<double> vals) =>
        new string(vals.Select(v => (char)(int)Math.Round(v)).ToArray());

    // ── INDATA ────────────────────────────────────────────────────────────────────

    Header($"INDATA  ·  {inputs.Count} datapunkter  [{(inputMode == InputMode.Text ? "text" : "numerisk")}]", ConsoleColor.Cyan);
    if (inputMode == InputMode.Text)
        Row("  " + DoublesToString(inputs));
    else
        WrapRows(inputs);
    Footer();

    // ── KOMPRIMERING ──────────────────────────────────────────────────────────────

    int coveredCount = cr.CoveredIndices.Count;
    Header($"KOMPRIMERING  ·  {coveredCount} av {inputs.Count} datapunkter täckta", ConsoleColor.Green);

    Row($"MATEMATISKA REGLER  ({cr.Compressed.Count} instanser)", ConsoleColor.DarkGreen);
    if (cr.Compressed.Count == 0)
    {
        Row("  Inga regelsekvenser hittade");
    }
    else
    {
        foreach (var item in cr.Compressed)
        {
            string ruleName = cr.DiscoveredRules[item.RuleIndex].Description;
            Row($"  [{item.StartIndex}]  {item.Count} tal  →  regel: {ruleName}  (startvärde: {item.StartValue})", ConsoleColor.Green);
        }
    }

    Divider();
    Row($"UPPREPADE BLOCK  ({cr.RepeatingBlocks.Count} block)", ConsoleColor.DarkGreen);
    if (cr.RepeatingBlocks.Count == 0)
    {
        Row("  Inga upprepade block hittade");
    }
    else
    {
        foreach (var rb in cr.RepeatingBlocks)
            Row($"  [{rb.StartIndex}]  [{string.Join(", ", rb.Block)}]  ×  {rb.RepeatCount}", ConsoleColor.Green);
    }

    Divider();
    Row($"MÖJLIGA REGLER TOTALT  ({cr.DiscoveredRules.Count} st)", ConsoleColor.DarkGreen);
    foreach (var rule in cr.DiscoveredRules)
        Row($"  {rule.Description}", ConsoleColor.Green);

    Footer();

    // ── OKOMPRIMERADE ─────────────────────────────────────────────────────────────

    Header($"OKOMPRIMERADE  ·  {cr.UncompressedValues.Count} tal", ConsoleColor.Yellow);
    if (cr.UncompressedValues.Count == 0)
        Row("  Alla datapunkter komprimerades!", ConsoleColor.Yellow);
    else if (inputMode == InputMode.Text)
        Row("  " + DoublesToString(cr.UncompressedValues), ConsoleColor.Yellow);
    else
        WrapRows(cr.UncompressedValues, ConsoleColor.Yellow);
    Footer();

    // ── VERIFIERING ───────────────────────────────────────────────────────────────

    bool verified = unpacked.Count == inputs.Count
        && unpacked.Zip(inputs).All(p => Math.Abs(p.First - p.Second) < 1e-10);

    Header($"VERIFIERING  ·  {unpacked.Count} datapunkter efter uppackning", ConsoleColor.Magenta);
    Row(verified ? "  ✓  Uppackning stämmer med originaldatan" : "  ✗  FEL: uppackning stämmer INTE",
        verified ? ConsoleColor.Green : ConsoleColor.Red);
    if (inputMode == InputMode.Text && verified)
        Row("  " + DoublesToString(unpacked), ConsoleColor.Green);
    Footer();

    Console.WriteLine("Tryck på valfri tangent för att starta om med nya data...");
    Console.ReadKey();
    Console.Clear();
    } while (true);
