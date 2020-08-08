using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FCS_DeepDriller.Enumerators;
using FCS_DeepDriller.Helpers;
using FCS_DeepDriller.Mono.MK2;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_DeepDriller.Managers
{
    internal class UpgradeManager : MonoBehaviour
    {
        private Dictionary<string,string[]> _classes = new Dictionary<string, string[]>
        {
            {"OreSystem",new []{ "OresPerDay", "SilkTouch", "MinOreCount", "MaxOreCount" } },
            {"System",new [] { "AutoStartUpAt", "AutoShutDownAt"}}
        };

        public List<UpgradeFunction> UpgradeFunctions { get; set; } = new List<UpgradeFunction>();
        private FCSDeepDrillerController _mono;
        
        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;

        }

        private void ImplementCommand(string functionString)
        {
            // check if we have a string to process
            if (string.IsNullOrEmpty(functionString)) return;

            //Check if the function name is valid
            var className = GetClassType(functionString, out var cNameResult);

            if (className)
            {
                //Check if the function name is valid
                var functionName = GetFunctionName(cNameResult, functionString, out var fNameResult);

                if (functionName)
                {
                    var parameters = GetMethodParameters(functionString, out var paraResults);

                    if (parameters)
                    {
                        switch (fNameResult)
                        {
                            case "OresPerDay":

                                var check = UpgradeFunctions.Any(x =>
                                    x.UpgradeType == Enumerators.UpgradeFunctions.OresPerDay);

                                if (check)
                                {
                                    QuickLogger.Message("Function already implemented.",true);
                                    return;
                                }

                                bool valid = OresPerDayUpgrade.IsValid(paraResults, out var orePerDay);
                                
                                if (valid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}",true);
                                    var upgrade = new OresPerDayUpgrade
                                    {
                                        IsEnabled = true,
                                        Mono = _mono,
                                        OreCount = orePerDay
                                    };
                                    UpgradeFunctions.Add(upgrade);
                                    _mono.OreGenerator.ApplyUpgrade(upgrade);
                                }
                                else
                                {
                                    QuickLogger.Debug($"Invalid Function: {functionString}",true);
                                }
                                break;

                            case "MaxOreCount":

                                var maxOreCountCheck = UpgradeFunctions.Any(x =>
                                    x.UpgradeType == Enumerators.UpgradeFunctions.MaxOreCount);

                                if (maxOreCountCheck)
                                {
                                    QuickLogger.Message("Function already implemented.", true);
                                    return;
                                }

                                bool maxOreCountValid = MaxOreCount.IsValid(paraResults, out var tuple);

                                if (maxOreCountValid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}", true);
                                    UpgradeFunctions.Add(new MaxOreCount()
                                    {
                                        IsEnabled = true,
                                        Mono = _mono,
                                        Amount = tuple.Item2,
                                        TechType = tuple.Item1
                                    });
                                }
                                else
                                {
                                    QuickLogger.Debug($"Invalid Function: {functionString}", true);
                                }
                                break;

                            case "MinOreCount":

                                var minOreCountCheck = UpgradeFunctions.Any(x =>
                                    x.UpgradeType == Enumerators.UpgradeFunctions.MinOreCount);

                                if (minOreCountCheck)
                                {
                                    QuickLogger.Message("Function already implemented.", true);
                                    return;
                                }

                                bool minOreCountValid = MinOreCount.IsValid(paraResults, out var minTuple);

                                if (minOreCountValid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}", true);
                                    UpgradeFunctions.Add(new MaxOreCount()
                                    {
                                        IsEnabled = true,
                                        Mono = _mono,
                                        Amount = minTuple.Item2,
                                        TechType = minTuple.Item1
                                    });
                                }
                                else
                                {
                                    QuickLogger.Debug($"Invalid Function: {functionString}", true);
                                }
                                break;

                            case "SilkTouch":

                                var silkTouchCheck = UpgradeFunctions.Any(x =>
                                    x.UpgradeType == Enumerators.UpgradeFunctions.SilkTouch);

                                if (silkTouchCheck)
                                {
                                    QuickLogger.Message("Function already implemented.", true);
                                    return;
                                }

                                bool silkTouchCheckValid = SilkTouch.IsValid(paraResults, out var silkTouchCheckTuple);

                                if (silkTouchCheckValid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}", true);
                                    UpgradeFunctions.Add(new SilkTouch()
                                    {
                                        IsEnabled = true,
                                        Mono = _mono,
                                        Amount = silkTouchCheckTuple.Item2,
                                        TechType = silkTouchCheckTuple.Item1
                                    });
                                }
                               else
                                {
                                    QuickLogger.Debug($"Invalid Function: {functionString}", true);
                                }
                                break;

                            case "AutoShutDownAt":

                                var autoShutdownAtCheck = UpgradeFunctions.Any(x =>
                                    x.UpgradeType == Enumerators.UpgradeFunctions.AutoShutdownAt);

                                if (autoShutdownAtCheck)
                                {
                                    QuickLogger.Message("Function already implemented.", true);
                                    return;
                                }

                                bool autoShutdownAtValid = AutoShutDownAt.IsValid(paraResults, out var autoShutdownAt);

                                if (autoShutdownAtValid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}", true);
                                    UpgradeFunctions.Add(new AutoShutDownAt
                                    {
                                        IsEnabled = true,
                                        Mono = _mono,
                                        Time = autoShutdownAt
                                    });
                                }
                                else
                                {
                                    QuickLogger.Debug($"Invalid Function: {functionString}", true);
                                }
                                break;

                            case "AutoStartUpAt":

                                var autoStartUpAtCheck = UpgradeFunctions.Any(x =>
                                    x.UpgradeType == Enumerators.UpgradeFunctions.AutoStartUpAt);

                                if (autoStartUpAtCheck)
                                {
                                    QuickLogger.Message("Function already implemented.", true);
                                    return;
                                }

                                bool autoStartUpAtValid = AutoStartUpAt.IsValid(paraResults, out var autoStartUpAt);

                                if (autoStartUpAtValid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}", true);
                                    UpgradeFunctions.Add(new AutoStartUpAt
                                    {
                                        IsEnabled = true,
                                        Mono = _mono,
                                        Time = autoStartUpAt
                                    });
                                }
                                else
                                {
                                    QuickLogger.Debug($"Invalid Function: {functionString}", true);
                                }
                                break;
                        }
                    }
                }
            }
        }

        private bool GetMethodParameters(string functionString, out string[] para)
        {
            para = null;
            List<string> parameters = new List<string>();
            var regexPattern = @"(\((?:\(??[^\(]*?\)))";
            Regex reg = new Regex(regexPattern,RegexOptions.Compiled | RegexOptions.IgnoreCase);
            bool matchFound = reg.IsMatch(functionString);
            QuickLogger.Debug( $"Parameters Found: {matchFound}");
            if (matchFound)
            {
                // Find matches.
                MatchCollection matches = reg.Matches(functionString);

                // Report the number of matches found.
                QuickLogger.Debug($"{matches.Count} matches found in:\n   {functionString}",true);

                // Report on each match.
                foreach (Match match in matches)
                {
                    QuickLogger.Debug($"Match {match.Value}",true);
                    var trimmed = match.Value.TrimStart('(').TrimEnd(')');
                    if (match.Value.Contains(","))
                    {
                        var split = trimmed.Split(',');
                        parameters = split.ToList();
                    }
                    else
                    {
                        parameters.Add(trimmed);
                    }
                    
                    GroupCollection groups = match.Groups;
                    QuickLogger.Debug($"'{ groups["word"].Value}' repeated at positions {groups[0].Index} and {groups[1].Index}");
                }

                foreach (string parameter in parameters)
                {
                    QuickLogger.Debug($"Found Parameter: {parameter}",true);
                }
                para = parameters.ToArray();
                return true;
            }

            return false;
        }

        private bool GetClassType(string functionString, out string result)
        {
            result = string.Empty;
            //Spit by "."
            var parts = functionString.Split('.');

            if (parts.Length <= 0) return false;

            for (int i = 0; i < parts.Length; i++)
            {
                var item = parts[i].Trim();
                if (_classes.ContainsKey(item))
                {
                    result = item;
                    QuickLogger.Debug($"Class Name = {result}", true);
                    return true;
                }
            }

            return false;
        }

        private bool GetFunctionName(string className, string functionString, out string result)
        {
            result = string.Empty;
            //Spit by "."
            var parts = functionString.Split('.');

            if (parts.Length <= 0) return false;

            for (int i = 0; i < parts.Length; i++)
            {
                var item = parts[i].Trim();
                if (item.EndsWith(";") && item.Contains("(") && item.Contains(")"))
                {
                    var methodName = item.GetUntilOrEmpty();

                    if (_classes[className].Any(x => x == methodName))
                    {
                        QuickLogger.Debug($"Method Name = {methodName}", true);
                        result = methodName;
                        return true;

                    }
                }
            }

            return false;
        }

        internal void Show()
        {
            uGUI.main.userInput.RequestString("Enter Function", "Execute Function", "Enter Function",100, ImplementCommand);
        }

        public bool HasUpgradeFunction(UpgradeFunctions upgradeEnum, out UpgradeFunction function)
        {
            function = null;

            if (UpgradeFunctions.Any(x => x.UpgradeType == upgradeEnum))
            {
                function = UpgradeFunctions.FirstOrDefault(x => x.UpgradeType == upgradeEnum);
                return true;
            }

            
            return false;
        }
    }

    internal abstract class UpgradeFunction : MonoBehaviour
    {
        private bool _isEnabled;

        public abstract float PowerUsage { get;}
        public abstract float Damage { get;}
        public abstract UpgradeFunctions UpgradeType { get;}
        public FCSDeepDrillerController Mono { get; set; }
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (value)
                {
                    ActivateUpdate();
                }
                else
                {
                    DeActivateUpdate();
                }
            }
        }
        public abstract void ActivateUpdate();
        public abstract void DeActivateUpdate();
        public abstract void TriggerUpdate();
    }

    internal class OresPerDayUpgrade : UpgradeFunction
    {
        public int OreCount { get; set; }

        public override float PowerUsage => 1.0f;
        public override float Damage => 0.5f;
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.OresPerDay;

        public override void ActivateUpdate()
        {
            if (Mono != null)
            {
                Mono.OreGenerator.SetOresPerDay(OreCount);
            }
        }

        public override void DeActivateUpdate()
        {
            if (Mono != null)
            {
                Mono.OreGenerator.SetOresPerDay(12);
            }
        }

        public override void TriggerUpdate()
        {
            if (Mono != null)
            {
                Mono.OreGenerator.SetOresPerDay(OreCount);
            }
        }

        internal static bool IsValid(string[] paraResults, out int amountPerDay)
        {
            amountPerDay = 0;
            try
            {
                if (paraResults.Length != 1)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Debug($"Incorrect amount of parameters expected 1 got {paraResults.Length}", true);
                    return false;
                }
                
                QuickLogger.Debug(
                    int.TryParse(paraResults[0], out var result)
                        ? $"Converted to number {result}"
                        : "Incorrect type in parameter expected: INT ex: OreSystem.OresPerDay(10); .", true);
                amountPerDay = Convert.ToInt32(result);
            }
            catch (Exception e)
            {
                //TODO Show Message Box with error of incorrect parameters
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }
    }

    internal class MaxOreCount : UpgradeFunction
    {
        public int Amount { get; set; }
        public override float PowerUsage => 0.2f;
        public override float Damage { get; }
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.MaxOreCount;
        public TechType TechType { get; set; }

        public override void ActivateUpdate()
        {
            
        }

        public override void DeActivateUpdate()
        {

        }

        public override void TriggerUpdate()
        {

        }

        internal static bool IsValid(string[] paraResults, out Tuple<TechType,int> data)
        {
            data = null;
           try
            {
                if (paraResults.Length != 2)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Debug($"Incorrect amount of parameters expected 2 got {paraResults.Length}", true);
                    return false;
                }

                if (paraResults[0].ToTechType() == TechType.None)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Debug($"Incorrect TechType Value", true);
                    return false;
                }

                QuickLogger.Debug(
                    int.TryParse(paraResults[1], out var result)
                        ? $"Converted to number {result}"
                        : "Incorrect type in parameter expected: INT ex: OreSystem.MaxOreCount(Silver,10); .", true);

                var techType = paraResults[0].ToTechType();
                var amount = Convert.ToInt32(result);
                data = new Tuple<TechType, int>(techType,amount);
            }
            catch (Exception e)
            {
                //TODO Show Message Box with error of incorrect parameters
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }
    }

    internal class MinOreCount : UpgradeFunction
    {
        public int Amount { get; set; }
        public override float PowerUsage => 0.2f;
        public override float Damage { get; }
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.MaxOreCount;
        public TechType TechType { get; set; }

        public override void ActivateUpdate()
        {

        }

        public override void DeActivateUpdate()
        {

        }

        public override void TriggerUpdate()
        {

        }

        internal static bool IsValid(string[] paraResults, out Tuple<TechType, int> data)
        {
            data = null;
            try
            {
                if (paraResults.Length != 2)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Debug($"Incorrect amount of parameters expected 2 got {paraResults.Length}", true);
                    return false;
                }

                if (paraResults[0].ToTechType() == TechType.None)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Debug($"Incorrect TechType Value", true);
                    return false;
                }

                QuickLogger.Debug(
                    int.TryParse(paraResults[1], out var result)
                        ? $"Converted to number {result}"
                        : "Incorrect type in parameter expected: INT ex: OreSystem.MinOreCount(Silver,10); .", true);

                var techType = paraResults[0].ToTechType();
                var amount = Convert.ToInt32(result);
                data = new Tuple<TechType, int>(techType, amount);
            }
            catch (Exception e)
            {
                //TODO Show Message Box with error of incorrect parameters
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }
    }

    internal class SilkTouch : UpgradeFunction
    {
        public float Amount { get; set; }
        public override float PowerUsage => 0.5f;
        public override float Damage => 1f;
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.SilkTouch;
        public TechType TechType { get; set; }

        public override void ActivateUpdate()
        {

        }

        public override void DeActivateUpdate()
        {

        }

        public override void TriggerUpdate()
        {

        }

        internal static bool IsValid(string[] paraResults, out Tuple<TechType, float> data)
        {
            data = null;
            try
            {
                if (paraResults.Length != 2)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Debug($"Incorrect amount of parameters expected 2 got {paraResults.Length}", true);
                    return false;
                }

                if (paraResults[0].ToTechType() == TechType.None)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Debug($"Incorrect TechType Value", true);
                    return false;
                }

                var tryResult = float.TryParse(paraResults[1], out var result);

                QuickLogger.Debug(tryResult
                        ? $"Converted to number {result}"
                        : "Incorrect type in parameter expected: INT ex: OreSystem.SilkTouch(Silver,1.0); .", true);

                if (!tryResult) return false;

                var techType = paraResults[0].ToTechType();
                data = new Tuple<TechType, float>(techType, result);
            }
            catch (Exception e)
            {
                //TODO Show Message Box with error of incorrect parameters
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }
    }

    internal class AutoShutDownAt : UpgradeFunction
    {
        public double Time { get; set; }
        public override float PowerUsage => 0.1f;
        public override float Damage { get; }
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.AutoShutdownAt;

        public override void ActivateUpdate()
        {

        }

        public override void DeActivateUpdate()
        {

        }

        public override void TriggerUpdate()
        {

        }

        internal static bool IsValid(string[] paraResults, out double data)
        {
            data = 0.0;
            try
            {
                if (paraResults.Length != 1)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Debug($"Incorrect amount of parameters expected 1 got {paraResults.Length}", true);
                    return false;
                }

                data = GetTime(paraResults[0]);
            }
            catch (Exception e)
            {
                //TODO Show Message Box with error of incorrect parameters
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }

        private static double GetTime(string time)
        {
            switch (time)
            {
                case "night":
                    return DayNightCycle.main.timePassedAsDouble += 1200.0 - DayNightCycle.main.timePassed % 1200.0;

                case "day":
                    return DayNightCycle.main.timePassedAsDouble += 1200.0 - DayNightCycle.main.timePassed % 1200.0 + 600.0;

            }

            return 0.0;
        }
    }

    internal class AutoStartUpAt : UpgradeFunction
    {
        public double Time { get; set; }
        public override float PowerUsage => 0.1f;
        public override float Damage { get; }
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.AutoStartUpAt;

        public override void ActivateUpdate()
        {

        }

        public override void DeActivateUpdate()
        {

        }

        public override void TriggerUpdate()
        {

        }

        internal static bool IsValid(string[] paraResults, out double data)
        {
            data = 0.0;
            try
            {
                if (paraResults.Length != 1)
                {
                    //TODO Show Message Box with error of incorrect parameters
                    QuickLogger.Debug($"Incorrect amount of parameters expected 1 got {paraResults.Length}", true);
                    return false;
                }

                data = GetTime(paraResults[0]);
            }
            catch (Exception e)
            {
                //TODO Show Message Box with error of incorrect parameters
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }

        private static double GetTime(string time)
        {
            switch (time)
            {
                case "night":
                    return DayNightCycle.main.timePassedAsDouble += 1200.0 - DayNightCycle.main.timePassed % 1200.0;

                case "day":
                    return DayNightCycle.main.timePassedAsDouble += 1200.0 - DayNightCycle.main.timePassed % 1200.0 + 600.0;

            }

            return 0.0;
        }
    }
}
