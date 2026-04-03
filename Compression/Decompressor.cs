using CustomPress.Models;

namespace CustomPress.Compression;

static class Decompressor
{
    public static List<double> Decompress(int totalCount, CompressionResult result, List<double> inputs)
    {
        var rules      = result.DiscoveredRules.Select(r => r.Transform).ToList();
        var unpacked   = new List<double>();
        int trunkIndex = 0;

        for (int t = 0; t < totalCount; t++)
        {
            var rep  = result.RepeatingBlocks.FirstOrDefault(r => r.StartIndex == t);
            var comp = result.Compressed.FirstOrDefault(c => c.StartIndex == t);

            if (rep != null)
            {
                for (int r = 0; r < rep.RepeatCount; r++)
                    foreach (var v in rep.Block)
                        unpacked.Add(v);
                t += rep.BlockLength * rep.RepeatCount - 1;
            }
            else if (comp != null)
            {
                double value = comp.StartValue;
                for (int i = 0; i < comp.Count; i++)
                {
                    unpacked.Add(i == 0 ? value : value = rules[comp.RuleIndex](value));
                }
                t += comp.Count - 1;
            }
            else
            {
                if (trunkIndex < result.UncompressedValues.Count)
                    unpacked.Add(result.UncompressedValues[trunkIndex++]);
            }
        }

        return unpacked;
    }
}
