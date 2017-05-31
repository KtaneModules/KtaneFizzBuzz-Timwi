using System.Collections.Generic;
using System.Linq;
using FizzBuzz;
using UnityEngine;

public class FizzBuzzModule : MonoBehaviour {

    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable[] SelectionButtons;
    public KMSelectable SubmitButton;
    public TextMesh[] Labels;

    int[] ButtonStates = new int[3];
    int[][] Nums = new int[3][];
    int[] Colors = new int[3];
    int[][] Solutions = new int[2][];
    int moduleId;
    static int moduleIdCounter = 1;
    Color[] ColorMats = new Color[] { Color.red, Color.green, Color.cyan, Color.yellow, Color.white };

    // STATES
    // 0: Number
    // 1: Fizz
    // 2: Buzz
    // 3: FizzBuzz
    static string[] StateNames = new string[] { "NUMBER", "FIZZ", "BUZZ", "FIZZBUZZ" };

    // COLORS
    // 0: Red
    // 1: Green
    // 2: Blue
    // 3: Yellow
    // 4: White
    static string[] ColorNames = new string[] { "red", "green", "blue", "yellow", "white" };

    public int[,] bases = new int[,] {
        {7, 3, 2, 4, 5}, // ≥ 3 battery holders present
		{3, 4, 9, 2, 8}, // Serial & parallel ports present
		{4, 5, 8, 8, 2}, // 3 letters & 3 digits in serial number
		{2, 3, 7, 9, 1}, // DVI & Stereo RCA ports present
		{1, 2, 2, 5, 3}, // ≥ 5 batteries present
		{3, 1, 8, 3, 4}, // None of the above
		{6, 6, 1, 2, 8} // ≥ 2 strikes
	};

    void Start() {
        moduleId = moduleIdCounter++;

        GetComponent<KMBombModule>().OnActivate += OnActivate;

        for (int i = 0; i < 3; i++) {
            var j = i;
            SelectionButtons[i].OnInteract += delegate { HandlePress(j); return false; };
            Nums[i] = GenNum();
            Labels[i].text = "";

            int color = Random.Range(0, 5);
            Colors[i] = color;
            Labels[i].color = ColorMats[color];
        }

        SubmitButton.OnInteract += delegate { Submit(); return false; };
    }

    void OnActivate() {
        for (int i = 0; i < 3; i++) {
            Labels[i].text = MakeString(i);
        }
        FindSolutions();
    }

    bool HandlePress(int buttonNum) {
        KMAudio.PlaySoundAtTransform("tick", this.transform);
        SelectionButtons[buttonNum].AddInteractionPunch();

        int state = (ButtonStates[buttonNum] + 1) % 4;
        ButtonStates[buttonNum] = state;

        switch (state) {
            case 0:
                Labels[buttonNum].text = MakeString(buttonNum);
                break;
            case 1:
                Labels[buttonNum].text = "Fizz";
                break;
            case 2:
                Labels[buttonNum].text = "Buzz";
                break;
            case 3:
                Labels[buttonNum].text = "FizzBuzz";
                break;
        }

        return false;
    }

    int[] GenNum() {
        int[] result = new int[7];
        for (int i = 0; i < 7; i++) {
            result[i] = Random.Range(0, 10);
        }
        return result;
    }

    string MakeString(int button) {
        string result = "";
        for (int i = 0; i < 7; i++) {
            result += "" + Nums[button][i];
        }
        return result;
    }

    void FindSolutions() {
        var conditions = new bool[6];
        if (BombInfo.GetBatteryHolderCount() >= 3) {
            conditions[0] = true;
            Debug.LogFormat("[FizzBuzz #{0}] Condition met: {1}", moduleId, "3+ battery holders");
        }
        if (BombInfo.IsPortPresent(KMBombInfoExtensions.KnownPortType.Serial) && BombInfo.IsPortPresent(KMBombInfoExtensions.KnownPortType.Parallel)) {
            conditions[1] = true;
            Debug.LogFormat("[FizzBuzz #{0}] Condition met: {1}", moduleId, "Serial & parallel ports present");
        }
        if (Enumerable.Count(BombInfo.GetSerialNumberLetters()) == Enumerable.Count(BombInfo.GetSerialNumberNumbers())) {
            conditions[2] = true;
            Debug.LogFormat("[FizzBuzz #{0}] Condition met: {1}", moduleId, "3 letters & 3 digits in serial number");
        }
        if (BombInfo.IsPortPresent(KMBombInfoExtensions.KnownPortType.DVI) && BombInfo.IsPortPresent(KMBombInfoExtensions.KnownPortType.StereoRCA)) {
            conditions[3] = true;
            Debug.LogFormat("[FizzBuzz #{0}] Condition met: {1}", moduleId, "DVI-D & Stereo RCA ports present");
        }
        if (BombInfo.GetBatteryCount() >= 5) {
            conditions[4] = true;
            Debug.LogFormat("[FizzBuzz #{0}] Condition met: {1}", moduleId, "5+ batteries");
        }
        if (conditions.All(b => !b)) {
            conditions[5] = true;
            Debug.LogFormat("[FizzBuzz #{0}] Condition met: {1}", moduleId, "No other conditions met");
        }

        foreach (var hasTwoStrikes in new[] { false, true }) {
            var solution = new int[3];
            for (int n = 0; n < 3; n++) {
                int addNum = 0;

                // Add up the numbers for all conditions
                for (int i = 0; i < 6; i++) {
                    // If two strikes, add the number for “two strikes” _instead of_ the one for “No other condition met”
                    if (i == 5 && hasTwoStrikes)
                        addNum += bases[6, Colors[n]];
                    else if (conditions[i])
                        addNum += bases[i, Colors[n]];
                }

                int num = 0;
                int beforeNum = 0;
                for (int i = 0; i < 7; i++) {
                    beforeNum *= 10;
                    beforeNum += Nums[n][i];

                    num *= 10;
                    num += (Nums[n][i] + addNum) % 10;
                }

                solution[n] = (num % 3 == 0 ? 1 : 0) + (num % 5 == 0 ? 2 : 0);

                Debug.LogFormat("[FizzBuzz #{0}] Button {1} ({2}), {3} 2 strikes:", moduleId, n + 1, ColorNames[Colors[n]], hasTwoStrikes ? "≥" : "<");
                Debug.LogFormat("[FizzBuzz #{0}] — original number is {1}", moduleId, beforeNum);
                Debug.LogFormat("[FizzBuzz #{0}] — adding number is {1}", moduleId, addNum);
                Debug.LogFormat("[FizzBuzz #{0}] — final number is {1}", moduleId, num);
                Debug.LogFormat("[FizzBuzz #{0}] — solution is {1}", moduleId, StateNames[solution[n]]);
            }
            Solutions[hasTwoStrikes ? 1 : 0] = solution;
            Debug.LogFormat("[FizzBuzz #{0}] Solution for {1} 2 strikes is [{2}, {3}, {4}].", moduleId, hasTwoStrikes ? "≥" : "<", StateNames[solution[0]], StateNames[solution[1]], StateNames[solution[2]]);
        }
    }

    void Submit() {
        KMAudio.PlaySoundAtTransform("tick", this.transform);
        GetComponent<KMSelectable>().AddInteractionPunch();

        var twoStrikes = BombInfo.GetStrikes() >= 2;
        Debug.LogFormat("[FizzBuzz #{0}] Submit button pressed at {1} 2 strikes.", moduleId, twoStrikes ? "≥" : "<");
        var solution = Solutions[twoStrikes ? 1 : 0];

        bool valid = true;
        for (int i = 0; i < 3; i++)
            if (solution[i] != ButtonStates[i])
                valid = false;

        if (valid) {
            Debug.LogFormat("[FizzBuzz #{0}] Module solved.", moduleId);
            BombModule.HandlePass();
        }
        else {
            Debug.LogFormat("[FizzBuzz #{0}] Submitted data: [{1}, {2}, {3}]. Incorrect solution.", moduleId, Labels[0].text, Labels[1].text, Labels[2].text);
            BombModule.HandleStrike();
        }
    }

    KMSelectable[] ProcessTwitchCommand(string command) {
        command = command.Trim().ToLowerInvariant();
        var pieces = command.Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (pieces.Length < 2 || pieces[0] != "press")
            return null;

        var list = new List<KMSelectable>();
        for (int i = 1; i < pieces.Length; i++) {
            switch (pieces[i]) {
                case "t":
                case "top":
                case "u":
                case "up":
                case "upper":
                case "1":
                case "first":
                case "1st":
                case "one":
                    list.Add(SelectionButtons[0]);
                    break;

                case "m":
                case "middle":
                case "c":
                case "center":
                case "centre":
                case "2":
                case "second":
                case "2nd":
                case "two":
                    list.Add(SelectionButtons[1]);
                    break;

                case "b":
                case "bottom":
                case "d":
                case "down":
                case "l":
                case "lower":
                case "3":
                case "third":
                case "3rd":
                case "three":
                    list.Add(SelectionButtons[2]);
                    break;

                case "s":
                case "sub":
                case "submit":
                case "done":
                    list.Add(SubmitButton);
                    break;

                default:
                    return null;
            }
        }

        return list.ToArray();
    }
}
