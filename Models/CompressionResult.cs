namespace CustomPress.Models;

record CompressionResult(
    List<Rule> DiscoveredRules,
    List<CompressedItem> Compressed,
    List<RepeatingBlock> RepeatingBlocks,
    List<double> UncompressedValues,
    Dictionary<int, int> RuleMatches,
    HashSet<int> CoveredIndices,
    InputMode Mode
);
