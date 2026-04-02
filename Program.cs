
// List<double> inputs = [2, 45, 63, 42, 43, 44, 45, 74, 901, 300, 150, 75, 2, 4, 8, 16, 32, 2, 2, 2, 2, 2, 2];
string userInput = string.Empty;
List<double> inputs = new List<double>();
do
{
   Console.WriteLine("Type E and enter to continue");
   Console.Write("Add num to list: ");

   userInput = Console.ReadLine();
   if(double.TryParse(userInput, out double result))
   {
       inputs.Add(result);
   }
   Console.Clear();
}
while (userInput != "E");

var trunkInputs = new List<double>();

var discoveredRules = FindRules(inputs);
var rules = discoveredRules.Select(r => r.Transform).ToList();

var removeIndices = new HashSet<int>();
int truncabiliy = 0;
int matchesInARow = 0;
HashSet<int> matchesIndex = new HashSet<int>();
Dictionary<int, int> ruleMatches = new Dictionary<int, int>();

for (int i = 0; i < rules.Count; i++)
{
    matchesInARow = 0;
    for (int j = 0; j < inputs.Count - 1; j++)
    {
        if (rules[i](inputs[j]) == inputs[j+1])
        {
            matchesInARow++;
            matchesIndex.Add(j);
            matchesIndex.Add(j+1);
            ruleMatches[j] = i;
            ruleMatches[j + 1] = i;
            removeIndices.Add(j);
            removeIndices.Add(j+1);
        }
    }
    if (matchesInARow >= 2)
    {
        truncabiliy += matchesInARow+1;
    }
}
var compressed = new List<CompressedItem>();

var matchesIndexList = matchesIndex.OrderBy(x => x).ToList();
int matches = 0;
int ruleMatchPos = 0;

while (matches < matchesIndexList.Count)
{
    int start = matchesIndexList[matches];
    int count = 1;
    while (matches + 1 < matchesIndexList.Count
        && matchesIndexList[matches + 1] == matchesIndexList[matches] + 1
        && ruleMatches.ContainsKey(matchesIndexList[matches])
        && ruleMatches.ContainsKey(matchesIndexList[matches + 1])
        && ruleMatches[matchesIndexList[matches]] ==
           ruleMatches[matchesIndexList[matches + 1]]
        && Math.Abs(rules[ruleMatches[matchesIndexList[matches]]](inputs[matchesIndexList[matches]])
           - inputs[matchesIndexList[matches] + 1]) < 1e-10)
    {
        count++;
        matches++;
    }

    if (count >= 2)
    {
        int ruleIndex = ruleMatches[matchesIndexList[matches]];
        compressed.Add(new CompressedItem
        {
            StartIndex = start,
            StartValue = inputs[start],
            Count = count,
            Rule = ruleIndex
        });

        ruleMatchPos += (count - 1);
    }

    matches++;
}

var coveredIndices = new HashSet<int>();
foreach (var item in compressed)
    for (int i = 0; i < item.Count; i++)
        coveredIndices.Add(item.StartIndex + i);

for (int i = 0; i < inputs.Count; i++)
    if (!coveredIndices.Contains(i))
        trunkInputs.Add(inputs[i]);

int inputsCount = inputs.Count;
var unpackedList = new List<double>();
double valueToBeAdded = 0;
int trunkIndex = 0;

for (int t = 0; t < inputsCount; t++)
{
    var comp = compressed.FirstOrDefault(c => c.StartIndex == t);

    if (comp != null)
    {
        for (int i = 0; i < comp.Count; i++)
        {
            if (i == 0)
            {
                valueToBeAdded = comp.StartValue;
            }
            else
            {
                valueToBeAdded = rules[comp.Rule](valueToBeAdded);
            }

            unpackedList.Add(valueToBeAdded);
        }

        t += comp.Count - 1;
    }
    else
    {
        if(trunkInputs.Count > trunkIndex)
        {
            unpackedList.Add(trunkInputs[trunkIndex]);
            trunkIndex++;
        }
    }
}

Console.Write("Lista med datapunkter: ");
foreach (var inp in inputs)
{
    Console.Write(inp + ", ");
}
Console.WriteLine();
Console.WriteLine();
Console.WriteLine($"Hittade {discoveredRules.Count} regler: " + string.Join(", ", discoveredRules.Select(r => r.Description)));
Console.WriteLine();
Console.WriteLine(truncabiliy + " datapunkter följer ett känt mönster och kan komprimeras");
Console.Write("Komprimeras enligt följande [TalIndex, Regel]: ");
foreach (var index in ruleMatches)
{
    Console.Write($"[{index.Key}, {discoveredRules[index.Value].Description}], ");
}
Console.WriteLine();
Console.WriteLine($"och sparas till {compressed.Count} instanser av en klass");
Console.Write("Följande tal gick inte att komprimera och behöver sparas enskilt: ");
foreach (var inp in trunkInputs)
{
    Console.Write(inp + ", ");
}
Console.WriteLine();
Console.WriteLine();
Console.Write("Lista med uppackade datapunkter: ");
foreach (var inp in unpackedList)
{
    Console.Write(inp + ", ");
}
Console.WriteLine();


// Only discovers rules that appear in at least 2 consecutive pairs (a triplet),
// which guarantees they can form compressible sequences.
static List<Rule> FindRules(List<double> inputs)
{
    var rules = new List<Rule>();
    var addedDiffs  = new HashSet<double>();
    var addedRatios = new HashSet<double>();
    var addedAffine = new HashSet<(double, double)>();
    bool hasSquare     = false;
    bool hasCube       = false;
    bool hasSqrt       = false;
    bool hasReciprocal = false;
    bool hasLog        = false;
    bool hasExp        = false;

    for (int i = 0; i < inputs.Count - 2; i++)
    {
        double a = inputs[i], b = inputs[i + 1], c = inputs[i + 2];

        // Additive: same constant diff across two consecutive pairs
        double diffAB = b - a;
        double diffBC = c - b;
        if (Math.Abs(diffAB - diffBC) < 1e-10 && addedDiffs.Add(diffAB))
        {
            double captured = diffAB;
            rules.Add(new Rule(x => x + captured, $"x + {captured}"));
        }

        // Multiplicative: same constant ratio across two consecutive pairs
        if (a != 0 && b != 0)
        {
            double ratioAB = b / a;
            double ratioBC = c / b;
            if (Math.Abs(ratioAB - ratioBC) < 1e-10 && addedRatios.Add(ratioAB))
            {
                double captured = ratioAB;
                rules.Add(new Rule(x => x * captured, $"x * {captured}"));
            }
        }

        // Squaring: b = a² and c = b²
        if (!hasSquare && b == a * a && c == b * b)
        {
            hasSquare = true;
            rules.Add(new Rule(x => x * x, "x^2"));
        }

        // Cubing: b = a³ and c = b³
        if (!hasCube && b == a * a * a && c == b * b * b)
        {
            hasCube = true;
            rules.Add(new Rule(x => x * x * x, "x^3"));
        }

        // Square root: b = √a and c = √b
        if (!hasSqrt && a >= 0 && b >= 0 && Math.Abs(b - Math.Sqrt(a)) < 1e-10 && Math.Abs(c - Math.Sqrt(b)) < 1e-10)
        {
            hasSqrt = true;
            rules.Add(new Rule(x => Math.Sqrt(x), "sqrt(x)"));
        }

        // Reciprocal: b = 1/a and c = 1/b
        if (!hasReciprocal && a != 0 && b != 0 && c != 0
            && Math.Abs(b - 1.0 / a) < 1e-10
            && Math.Abs(c - 1.0 / b) < 1e-10)
        {
            hasReciprocal = true;
            rules.Add(new Rule(x => 1.0 / x, "1/x"));
        }

        // Natural log: b = ln(a) and c = ln(b)
        if (!hasLog && a > 0 && b > 0
            && Math.Abs(b - Math.Log(a)) < 1e-10
            && Math.Abs(c - Math.Log(b)) < 1e-10)
        {
            hasLog = true;
            rules.Add(new Rule(x => Math.Log(x), "ln(x)"));
        }

        // Exponential: b = e^a and c = e^b
        if (!hasExp
            && Math.Abs(b - Math.Exp(a)) < 1e-10
            && Math.Abs(c - Math.Exp(b)) < 1e-10)
        {
            hasExp = true;
            rules.Add(new Rule(x => Math.Exp(x), "e^x"));
        }

        // Affine: b = m*a + k and c = m*b + k (excludes pure additive m=1 and pure multiplicative k=0)
        if (Math.Abs(b - a) > 1e-10)
        {
            double m = (c - b) / (b - a);
            double k = b - m * a;
            if (!double.IsNaN(m) && !double.IsInfinity(m)
                && Math.Abs(k) > 1e-10
                && Math.Abs(m - 1.0) > 1e-10)
            {
                var key = (Math.Round(m, 9), Math.Round(k, 9));
                if (addedAffine.Add(key))
                {
                    double cm = m, ck = k;
                    rules.Add(new Rule(x => cm * x + ck, $"{cm}*x + {ck}"));
                }
            }
        }
    }

    return rules;
}


record Rule(Func<double, double> Transform, string Description);

public class CompressedItem
{
    public int StartIndex { get; set; }
    public double StartValue { get; set; }
    public int Count { get; set; }
    public int Rule { get; set; }
}
