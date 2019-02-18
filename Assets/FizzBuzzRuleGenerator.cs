using System.Collections.Generic;
using System.Linq;
using KModkit;

public class FizzBuzzRuleGenerator
{
    private MonoRandom rnd;
    public FizzBuzzModule module;
    private int seed;

    public FizzBuzzRuleGenerator(MonoRandom rnd, FizzBuzzModule module)
    {
        this.rnd = rnd;
        seed = rnd.Seed;
        this.module = module;
    }

    #region Number Table
    private static readonly int[,] DefaultTable = new int[,] {
        {7, 3, 2, 4, 5}, // ≥ 3 battery holders present
		{3, 4, 9, 2, 8}, // Serial & parallel ports present
		{4, 5, 8, 8, 2}, // 3 letters & 3 digits in serial number
		{2, 3, 7, 9, 1}, // DVI & Stereo RCA ports present
		{1, 2, 2, 5, 3}, // ≥ 5 batteries present
		{3, 1, 8, 3, 4}, // None of the above
		{6, 6, 1, 2, 8}  // ≥ 2 strikes
	};

    public int[,] GenerateOffsetTable()
    {
        if (seed == 1)
        {
            return DefaultTable;
        }
        else
        {
            int[,] rules = new int[7, 5];
            for (int i = 0; i < 5; i++)
            {
                List<int> list = generateIntList(1, 10);
                list = rnd.ShuffleFisherYates(list).Skip(3).ToList();
                list.Add(list[rnd.Next(0, 6)]);
                for (int j = 0; j < 7; j++)
                {
                    rules[j, i] = list[j];
                }
            }
            return rules;
        }
    }

    private List<int> generateIntList(int min, int max)
    {
        List<int> result = new List<int>();
        for (int i = min; i < max; i++)
        {
            result.Add(i);
        }
        return result;
    }
    #endregion

    #region Conditions
    private readonly Dictionary<Port, string> ports = new Dictionary<Port, string>() { { Port.PS2, "PS/2" }, { Port.DVI, "DVI-D" }, { Port.StereoRCA, "Stereo RCA" }, { Port.RJ45, "RJ-45" } };

    private List<FizzBuzzRule> ruleSet1 = new List<FizzBuzzRule>();
    private List<FizzBuzzRule> ruleSet2 = new List<FizzBuzzRule>();
    private List<FizzBuzzRule> ruleSet3 = new List<FizzBuzzRule>();

    private void setupRuleSets()
    {
        // Rule Set 1
        ruleSet1.Add(new FizzBuzzRule
        {
            Id = "AtLeast2Strikes",
            RuleText = "2 or more strikes are present on the bomb.",
            CheckRule = info => info.GetStrikes() >= 2
        });
        ruleSet1.Add(new FizzBuzzRule
        {
            Id = "LessThanOneThirdStartingTime",
            RuleText = "Less than one third of the bomb's starting time remains on the timer.",
            CheckRule = info => info.GetTime() <= module.startingTime / 3
        });
        ruleSet1.Add(new FizzBuzzRule
        {
            Id = "MoreThanTwoThirdsModulesSolved",
            RuleText = "More than two thirds of the modules present on the bomb have been solved.",
            CheckRule = info => info.GetSolvedModuleNames().Count > module.startingTime * 2f / 3
        });

        // Rule Set 2
        ruleSet2.Add(new FizzBuzzRule
        {
            Id = "SerialAndParallelPortPresent",
            RuleText = "At least one Serial <u>and</u> Parallel port are present on the bomb.",
            CheckRule = info => info.IsPortPresent(Port.Serial) && info.IsPortPresent(Port.Parallel)
        });
        ruleSet2.Add(new FizzBuzzRule
        {
            Id = "ThreeLettersAndThreeDigitsInSerialNumber",
            RuleText = "3 letters and 3 digits are present in the serial number.",
            CheckRule = info => Enumerable.Count(info.GetSerialNumberLetters()) > 3 && Enumerable.Count(info.GetSerialNumberNumbers()) > 3
        });
        ruleSet2.Add(new FizzBuzzRule
        {
            Id = "SerialNumberHasVowel",
            RuleText = "A vowel is present in the serial number.",
            CheckRule = info => info.GetSerialNumberLetters().Any(letter => "AEIOU".Contains(letter))
        });
        ruleSet2.Add(new FizzBuzzRule
        {
            Id = "GreenNumberPresent",
            RuleText = "A green number is present on the module.",
            CheckRule = _ => module.Colors.Contains(1)
        });

        // Rule Set 3
        ruleSet3.Add(new FizzBuzzRule
        {
            Id = "AtLeastXBatteryHoldersPresent",
            RuleText = "{0} or more battery holders are present on the bomb.",
            GenRule = rnd => new object[] { rnd.Next(2, 4) },
            CheckRuleWithConfigs = (info, configs) => info.GetBatteryHolderCount() > (int) configs[0]
        });
        ruleSet3.Add(new FizzBuzzRule
        {
            Id = "AtLeastXBatteriesPresent",
            RuleText = "{0} or more batteries are present on the bomb.",
            GenRule = rnd => new object[] { rnd.Next(4, 6) },
            CheckRuleWithConfigs = (info, configs) => info.GetBatteryCount() > (int) configs[0]
        });
        ruleSet3.Add(new FizzBuzzRule
        {
            Id = "XAndYPortsPresent",
            RuleText = "At least one {2} <u>and</u> {3} port are present on the bomb.",
            GenRule = rnd =>
            {
                int port1Index = rnd.Next(0, 4);
                int port2Index;
                //port2Index = rnd.Next(0, 4);
                do { port2Index = rnd.Next(0, 4); } while (port2Index == port1Index);
                Port port1, port2;
                var keys = ports.Keys.ToArray();
                port1 = keys[port1Index];
                port2 = keys[port2Index];
                return new object[] {
                    port1, port2,
                    ports[port1], ports[port2]
                };
            },
            CheckRuleWithConfigs = (info, configs) => info.IsPortPresent((Port) configs[0]) && info.IsPortPresent((Port) configs[1])
        });
        ruleSet3.Add(new FizzBuzzRule
        {
            Id = "IndicatorXPresent",
            RuleText = "A{2} {0} present on the bomb.",
            GenRule = rnd =>
            {
                Indicator label = (Indicator) System.Enum.GetValues(typeof(Indicator)).GetValue(rnd.Next(0, 12));
                bool lit = rnd.Next(0, 2) == 0;
                return new object[] {
                    label, lit, lit ? " lit" : "n unlit"
                };
            },
            CheckRuleWithConfigs = (info, configs) => (bool) configs[1] ? info.IsIndicatorOn((Indicator) configs[0]) : info.IsIndicatorOff((Indicator) configs[0])
        });
    }

    // ≥ 3 battery holders present
    // Serial & parallel ports present
    // 3 letters & 3 digits in serial number
    // DVI & Stereo RCA ports present
    // ≥ 5 batteries present
    // None of the above
    // ≥ 2 strikes

    public List<FizzBuzzRule> GenerateRuleSet()
    {
        setupRuleSets();

        if (seed == 1)
        {
            return new List<FizzBuzzRule> {
                ruleSet3[0].SetConfigs(3),
                ruleSet2[0],
                ruleSet2[1],
                ruleSet3[2].SetConfigs(Port.DVI, Port.StereoRCA, "DVI-D", "Stereo RCA"),
                ruleSet3[1].SetConfigs(5),
                ruleSet1[0]
            };
        }
        var result = new List<FizzBuzzRule>();

        int ix1 = rnd.Next(0, ruleSet2.Count);
        int ix2;
        do { ix2 = rnd.Next(0, ruleSet2.Count); } while (ix2 == ix1);
        result.Add(ruleSet2[ix1]);
        result.Add(ruleSet2[ix2]);

        ix1 = rnd.Next(0, ruleSet3.Count);
        int ix3;
        do { ix2 = rnd.Next(0, ruleSet3.Count); } while (ix2 == ix1);
        do { ix3 = rnd.Next(0, ruleSet3.Count); } while (ix3 == ix1 || ix3 == ix2);
        var rs3Rule1 = ruleSet3[ix1];
        var rs3Rule2 = ruleSet3[ix2];
        var rs3Rule3 = ruleSet3[ix3];
        rs3Rule1.SetConfigs(rs3Rule1.GenRule(rnd));
        rs3Rule2.SetConfigs(rs3Rule2.GenRule(rnd));
        rs3Rule3.SetConfigs(rs3Rule3.GenRule(rnd));
        result.Add(rs3Rule1);
        result.Add(rs3Rule2);
        result.Add(rs3Rule3);

        rnd.ShuffleFisherYates(result);

        result.Add(ruleSet1[rnd.Next(0, ruleSet1.Count)]);

        return result;
    }
    #endregion

    #region Divisibility Rules
    private static readonly int[,] divisibilityRules = {
        { 2, 4, 5, 8 },
        { 3, 7, 9, 11 }
    };

    public int[] GenerateDivisibilityRules()
    {
        if (seed == 1)
        {
            return new int[] { 3, 5 };
        }
        return new int[] {
            divisibilityRules[0, rnd.Next(0, 4)],
            divisibilityRules[1, rnd.Next(0, 4)]
        }.OrderBy(x => x).ToArray();
    }
    #endregion
}

public class FizzBuzzRule
{
    public string Id;
    public string RuleText;
    public object[] Configs;

    public GenRuleMethod GenRule;
    public delegate object[] GenRuleMethod(MonoRandom rnd);

    public CheckRuleMethod CheckRule;
    public delegate bool CheckRuleMethod(KMBombInfo info);

    public CheckRuleWithConfigsMethod CheckRuleWithConfigs;
    public delegate bool CheckRuleWithConfigsMethod(KMBombInfo info, object[] configs);

    public string GetRuleString()
    {
        if (Configs != null)
        {
            return string.Format(RuleText, Configs);
        }
        else
        {
            return RuleText;
        }
    }

    public FizzBuzzRule SetConfigs(params object[] configs)
    {
        Configs = configs;
        return this;
    }
}