using CustomPress.Models;

namespace CustomPress.Compression;

static class BlockFinder
{
    public static List<RepeatingBlock> FindRepeatingBlocks(List<double> inputs)
    {
        int n = inputs.Count;
        var candidates = new List<RepeatingBlock>();

        for (int start = 0; start < n; start++)
        {
            for (int len = 2; start + len * 2 <= n; len++)
            {
                int reps = 1;
                while (start + len * (reps + 1) <= n)
                {
                    bool match = true;
                    for (int k = 0; k < len; k++)
                    {
                        if (Math.Abs(inputs[start + k] - inputs[start + len * reps + k]) > 1e-10)
                        { match = false; break; }
                    }
                    if (!match) break;
                    reps++;
                }

                if (reps >= 2)
                    candidates.Add(new RepeatingBlock
                    {
                        StartIndex  = start,
                        BlockLength = len,
                        RepeatCount = reps,
                        Block       = inputs.GetRange(start, len)
                    });
            }
        }

        candidates.Sort((a, b) =>
            (b.BlockLength * (b.RepeatCount - 1))
            .CompareTo(a.BlockLength * (a.RepeatCount - 1)));

        var chosen = new List<RepeatingBlock>();
        var used   = new HashSet<int>();

        foreach (var c in candidates)
        {
            int end = c.StartIndex + c.BlockLength * c.RepeatCount;
            bool overlap = false;
            for (int i = c.StartIndex; i < end; i++)
                if (used.Contains(i)) { overlap = true; break; }
            if (overlap) continue;

            chosen.Add(c);
            for (int i = c.StartIndex; i < end; i++)
                used.Add(i);
        }

        return chosen;
    }
}
