using CustomPress.Models;

namespace CustomPress.Compression;

static class Compressor
{
    public static CompressionResult Compress(List<double> inputs)
    {
        var discoveredRules  = RuleFinder.FindRules(inputs);
        var repeatingBlocks  = BlockFinder.FindRepeatingBlocks(inputs);
        var rules            = discoveredRules.Select(r => r.Transform).ToList();

        var repeatingCovered = new HashSet<int>();
        foreach (var rb in repeatingBlocks)
            for (int i = rb.StartIndex; i < rb.StartIndex + rb.BlockLength * rb.RepeatCount; i++)
                repeatingCovered.Add(i);

        var matchesIndex = new HashSet<int>();
        var ruleMatches  = new Dictionary<int, int>();

        for (int i = 0; i < rules.Count; i++)
        {
            for (int j = 0; j < inputs.Count - 1; j++)
            {
                if (repeatingCovered.Contains(j) || repeatingCovered.Contains(j + 1)) continue;
                if (rules[i](inputs[j]) == inputs[j + 1])
                {
                    matchesIndex.Add(j);
                    matchesIndex.Add(j + 1);
                    ruleMatches[j]     = i;
                    ruleMatches[j + 1] = i;
                }
            }
        }

        var compressed       = new List<CompressedItem>();
        var matchesIndexList = matchesIndex.OrderBy(x => x).ToList();
        int pos              = 0;

        while (pos < matchesIndexList.Count)
        {
            int start = matchesIndexList[pos];
            int count = 1;

            while (pos + 1 < matchesIndexList.Count
                && matchesIndexList[pos + 1] == matchesIndexList[pos] + 1
                && ruleMatches.ContainsKey(matchesIndexList[pos])
                && ruleMatches.ContainsKey(matchesIndexList[pos + 1])
                && ruleMatches[matchesIndexList[pos]] == ruleMatches[matchesIndexList[pos + 1]]
                && Math.Abs(rules[ruleMatches[matchesIndexList[pos]]](inputs[matchesIndexList[pos]])
                   - inputs[matchesIndexList[pos] + 1]) < 1e-10)
            {
                count++;
                pos++;
            }

            if (count >= 2)
                compressed.Add(new CompressedItem
                {
                    StartIndex = start,
                    StartValue = inputs[start],
                    Count      = count,
                    RuleIndex  = ruleMatches[matchesIndexList[pos]]
                });

            pos++;
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
            coveredIndices
        );
    }
}
