using CustomPress.Models;

namespace CustomPress.Compression;

static class Compressor
{
    public static CompressionResult Compress(List<double> inputs, InputMode mode = InputMode.Numeric)
    {
        var discoveredRules  = RuleFinder.FindRules(inputs);
        var repeatingBlocks  = BlockFinder.FindRepeatingBlocks(inputs);
        var rules            = discoveredRules.Select(r => r.Transform).ToList();

        var repeatingCovered = new HashSet<int>();
        foreach (var rb in repeatingBlocks)
            for (int i = rb.StartIndex; i < rb.StartIndex + rb.BlockLength * rb.RepeatCount; i++)
                repeatingCovered.Add(i);

        var ruleMatches = new Dictionary<int, int>();
        var compressed  = new List<CompressedItem>();
        var ruleCovered = new HashSet<int>();

        for (int ruleIdx = 0; ruleIdx < rules.Count; ruleIdx++)
        {
            int j = 0;
            while (j < inputs.Count)
            {
                if (repeatingCovered.Contains(j) || ruleCovered.Contains(j)) { j++; continue; }

                int start = j;
                int count = 1;

                while (start + count < inputs.Count
                    && !repeatingCovered.Contains(start + count)
                    && !ruleCovered.Contains(start + count)
                    && Math.Abs(rules[ruleIdx](inputs[start + count - 1]) - inputs[start + count]) < 1e-10)
                    count++;

                if (count >= 3)
                {
                    compressed.Add(new CompressedItem
                    {
                        StartIndex = start,
                        StartValue = inputs[start],
                        Count      = count,
                        RuleIndex  = ruleIdx
                    });
                    for (int k = start; k < start + count; k++)
                    {
                        ruleCovered.Add(k);
                        ruleMatches[k] = ruleIdx;
                    }
                    j = start + count;
                }
                else
                    j++;
            }
        }

        var coveredIndices = new HashSet<int>();
        foreach (var item in compressed)
            for (int i = 0; i < item.Count; i++)
                coveredIndices.Add(item.StartIndex + i);
        foreach (var i in repeatingCovered)
            coveredIndices.Add(i);

        var uncompressedValues = new List<double>();
        for (int i = 0; i < inputs.Count; i++)
            if (!coveredIndices.Contains(i))
                uncompressedValues.Add(inputs[i]);

        return new CompressionResult(
            discoveredRules,
            compressed,
            repeatingBlocks,
            uncompressedValues,
            ruleMatches,
            coveredIndices,
            mode
        );
    }
}
