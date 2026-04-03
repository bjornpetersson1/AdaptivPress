using CustomPress.Compression;
using CustomPress.Models;

// List<double> inputs = [2, 45, 63, 42, 43, 44, 45, 74, 901, 300, 150, 75, 2, 4, 8, 16, 32, 2, 2, 2, 2, 2, 2];
string userInput = string.Empty;
var inputs = new List<double>();
do
{
    Console.WriteLine("Type E and enter to continue");
    Console.Write("Add num to list: ");

    userInput = Console.ReadLine() ?? string.Empty;
    if (double.TryParse(userInput, out double result))
        inputs.Add(result);
    Console.Clear();
}
while (userInput != "E");

// Compress
var compressionResult = Compressor.Compress(inputs);
var unpacked          = Decompressor.Decompress(inputs.Count, compressionResult, inputs);

// Output
Console.Write("Lista med datapunkter:\n");
foreach (var inp in inputs)
    Console.Write(inp + "\n");
Console.WriteLine();
Console.WriteLine("Totalt " + inputs.Count + " datapunkter");
Console.WriteLine("Dessa " + compressionResult.CoveredIndices.Count + " datapunkter följer ett känt mönster och kan komprimeras");
foreach(var index in compressionResult.CoveredIndices)
    Console.WriteLine($"  Index {index}: {inputs[index]}");
Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
// Console.Write("Möjliga regler för komprimering [TalIndex, Regel]: ");
// foreach (var index in compressionResult.RuleMatches)
//     Console.Write($"[{index.Key}, {compressionResult.DiscoveredRules[index.Value].Description}], ");
Console.WriteLine();
Console.WriteLine($"Komprimerade tal som komprimeras enligt en matematisk regel sparas i {compressionResult.Compressed.Count} instanser av en regelklass");
Console.WriteLine($"Hittade {compressionResult.DiscoveredRules.Count} möjliga regler:\n"
    + string.Join("\n", compressionResult.DiscoveredRules.Select(r => r.Description)));
Console.WriteLine();
Console.WriteLine($"Komprimerade tal som komprimeras enligt en blockregel sparas i {compressionResult.RepeatingBlocks.Count} instanser av en blockregel \n");
foreach (var rb in compressionResult.RepeatingBlocks)
    Console.WriteLine($"  Index {rb.StartIndex}: [{string.Join(", ", rb.Block)}] × {rb.RepeatCount}");
Console.Write("Dessa " + compressionResult.UncompressedValues.Count + " tal gick inte att komprimera och behöver sparas enskilt:\n");
foreach (var inp in compressionResult.UncompressedValues)
    Console.Write(inp + "\n");
Console.WriteLine();
Console.WriteLine();
Console.Write("Lista med uppackade datapunkter:\n");
foreach (var inp in unpacked)
    Console.Write(inp + "\n");
Console.WriteLine($"Totalt {unpacked.Count} datapunkter efter uppackning");
Console.WriteLine();
