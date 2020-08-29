using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FCS_DeepDriller.Buildable.MK2;
using FCS_DeepDriller.Helpers;
using FCS_DeepDriller.Model.Upgrades;
using FCS_DeepDriller.Mono.MK2;
using FCS_DeepDriller.Structs;
using FCSCommon.Enums;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using UnityEngine;


namespace FCS_DeepDriller.Managers
{
    internal class UpgradeManager : MonoBehaviour
    {
        private readonly List<UpgradeClass> _classes = new List<UpgradeClass>
        {
            new UpgradeClass{FunctionName = "OresPerDay", FriendlyName = "Ores Per Day"},
            new UpgradeClass{FunctionName = "SilkTouch", FriendlyName = "Silk Touch"},
            new UpgradeClass{FunctionName = "MinOreCount", FriendlyName = "Minimum Ore Count"},
            new UpgradeClass{FunctionName = "MaxOreCount", FriendlyName = "Maximum Ore Count"},
            new UpgradeClass{FunctionName = "AutoStartUpAt", FriendlyName = "Auto Start Down At"},
            new UpgradeClass{FunctionName = "AutoShutDownAt", FriendlyName = "Auto Shut Down At"}
        };
        
        public List<UpgradeFunction> Upgrades { get; set; } = new List<UpgradeFunction>();
        public Action<UpgradeFunction> OnUpgradeUpdate { get; set; }

        private FCSDeepDrillerController _mono;

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;

        }

        private void ImplementCommand(string functionString, out UpgradeFunction upgrade)
        {
            upgrade = null;

            // check if we have a string to process
            if (string.IsNullOrEmpty(functionString)) return;
            
            //Check if the function name is valid
            var validClass = ValidateClassType(functionString);

            if (validClass)
            {
                //Check if the function name is valid
                var functionName = GetFunctionName( functionString, out var fNameResult);

                if (functionName)
                {
                    var parameters = GetMethodParameters(functionString, out var paraResults);

                    if (parameters)
                    {
                        switch (fNameResult)
                        {
                            case "OresPerDay":

                                var check = Upgrades.Any(x =>
                                    x.UpgradeType == UpgradeFunctions.OresPerDay);

                                if (check)
                                {
                                    QuickLogger.Message("Function already implemented.", true);
                                    return;
                                }

                                bool valid = OresPerDayUpgrade.IsValid(paraResults, out var orePerDay);

                                if (valid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}", true);
                                    var orePerDayUpgrade = new OresPerDayUpgrade
                                    {
                                        IsEnabled = true,
                                        Mono = _mono,
                                        OreCount = orePerDay,
                                        Function = functionString
                                    };
                                    Upgrades.Add(orePerDayUpgrade);
                                    OnUpgradeUpdate?.Invoke(orePerDayUpgrade);
                                    _mono.OreGenerator.ApplyUpgrade(orePerDayUpgrade);
                                    upgrade = orePerDayUpgrade;
                                }
                                else
                                {
                                    QuickLogger.Debug($"Invalid Function: {functionString}", true);
                                }
                                break;

                            case "MaxOreCount":

                                var maxOreCountCheck = Upgrades.Any(x =>
                                    x.UpgradeType == UpgradeFunctions.MaxOreCount);

                                if (maxOreCountCheck)
                                {
                                    QuickLogger.Message("Function already implemented.", true);
                                    return;
                                }

                                bool maxOreCountValid = MaxOreCountUpgrade.IsValid(paraResults, out var tuple);

                                if (maxOreCountValid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}", true);
                                    var maxOreCountUpgrade = new MaxOreCountUpgrade
                                    {
                                        IsEnabled = true,
                                        Mono = _mono,
                                        Amount = tuple.Item2,
                                        TechType = tuple.Item1,
                                        Function = functionString

                                    };
                                    Upgrades.Add(maxOreCountUpgrade);
                                    OnUpgradeUpdate?.Invoke(maxOreCountUpgrade);
                                    _mono.OreGenerator.ApplyUpgrade(maxOreCountUpgrade);
                                    upgrade = maxOreCountUpgrade;
                                }
                                else
                                {
                                    QuickLogger.Debug($"Invalid Function: {functionString}", true);
                                }
                                break;

                            case "MinOreCount":

                                var minOreCountCheck = Upgrades.Any(x =>
                                    x.UpgradeType == UpgradeFunctions.MinOreCount);

                                if (minOreCountCheck)
                                {
                                    QuickLogger.Message("Function already implemented.", true);
                                    return;
                                }

                                bool minOreCountValid = MinOreCountUpgrade.IsValid(paraResults, out var minTuple);

                                if (minOreCountValid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}", true);
                                    var maxOreCountUpgrade = new MaxOreCountUpgrade
                                    {
                                        IsEnabled = true,
                                        Mono = _mono,
                                        Amount = minTuple.Item2,
                                        TechType = minTuple.Item1,
                                        Function = functionString

                                    };
                                    Upgrades.Add(maxOreCountUpgrade);
                                    upgrade = maxOreCountUpgrade;
                                    OnUpgradeUpdate?.Invoke(maxOreCountUpgrade);
                                }
                                else
                                {
                                    QuickLogger.Debug($"Invalid Function: {functionString}", true);
                                }
                                break;

                            case "SilkTouch":

                                var silkTouchCheck = Upgrades.Any(x =>
                                    x.UpgradeType == UpgradeFunctions.SilkTouch);

                                if (silkTouchCheck)
                                {
                                    QuickLogger.Message("Function already implemented.", true);
                                    return;
                                }

                                bool silkTouchCheckValid = SilkTouchUpgrade.IsValid(paraResults, out var silkTouchCheckTuple);

                                if (silkTouchCheckValid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}", true);
                                    var silkTouchUpgrade = new SilkTouchUpgrade
                                    {
                                        IsEnabled = true,
                                        Mono = _mono,
                                        Amount = silkTouchCheckTuple.Item2,
                                        TechType = silkTouchCheckTuple.Item1,
                                        Function = functionString

                                    };
                                    Upgrades.Add(silkTouchUpgrade);
                                    upgrade = silkTouchUpgrade;
                                    OnUpgradeUpdate?.Invoke(silkTouchUpgrade);
                                }
                                else
                                {
                                    QuickLogger.Debug($"Invalid Function: {functionString}", true);
                                }
                                break;

                            case "AutoShutDownAt":

                                var autoShutdownAtCheck = Upgrades.Any(x =>
                                    x.UpgradeType == UpgradeFunctions.AutoShutdownAt);

                                if (autoShutdownAtCheck)
                                {
                                    QuickLogger.Message("Function already implemented.", true);
                                    return;
                                }

                                bool autoShutdownAtValid = AutoShutDownAtUpgrade.IsValid(paraResults, out var autoShutdownAt);

                                if (autoShutdownAtValid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}", true);
                                    var autoShutDownAtUpgrade = new AutoShutDownAtUpgrade { IsEnabled = true, Mono = _mono, Time = autoShutdownAt,Function = functionString};
                                    Upgrades.Add(autoShutDownAtUpgrade);
                                    upgrade = autoShutDownAtUpgrade;
                                    OnUpgradeUpdate?.Invoke(autoShutDownAtUpgrade);
                                }
                                else
                                {
                                    QuickLogger.Debug($"Invalid Function: {functionString}", true);
                                }
                                break;

                            case "AutoStartUpAt":

                                var autoStartUpAtCheck = Upgrades.Any(x =>
                                    x.UpgradeType == UpgradeFunctions.AutoStartUpAt);

                                if (autoStartUpAtCheck)
                                {
                                    QuickLogger.Message("Function already implemented.", true);
                                    return;
                                }

                                bool autoStartUpAtValid = AutoStartUpAtUpgrade.IsValid(paraResults, out var autoStartUpAt);

                                if (autoStartUpAtValid)
                                {
                                    QuickLogger.Debug($"Function  Valid: {functionString}", true);
                                    var autoStartUpAtUpgrade = new AutoStartUpAtUpgrade { IsEnabled = true, Mono = _mono, Time = autoStartUpAt,Function = functionString};
                                    Upgrades.Add(autoStartUpAtUpgrade);
                                    upgrade = autoStartUpAtUpgrade;
                                    OnUpgradeUpdate?.Invoke(autoStartUpAtUpgrade);
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
            Regex reg = new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            bool matchFound = reg.IsMatch(functionString);
            QuickLogger.Debug($"Parameters Found: {matchFound}");
            if (matchFound)
            {
                // Find matches.
                MatchCollection matches = reg.Matches(functionString);

                // Report the number of matches found.
                QuickLogger.Debug($"{matches.Count} matches found in:\n   {functionString}", true);

                // Report on each match.
                foreach (Match match in matches)
                {
                    QuickLogger.Debug($"Match {match.Value}", true);
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
                    QuickLogger.Debug($"Found Parameter: {parameter}", true);
                }
                para = parameters.ToArray();
                return true;
            }

            return false;
        }

        private bool ValidateClassType(string functionString)
        {
            //Spit by "."
            var parts = functionString.Split('.');

            //If the command has more than zero parts and contains os as the first item the class is valid
            var result =  parts.Length > 0 && parts.Select(t => t.Trim()).Any(item => item.ToLower().Equals("os"));

            if (!result)
            {
                QuickLogger.Message(string.Format(FCSDeepDrillerBuildable.InvalidClassFormat(), parts.Length > 0 ? parts[0] : "N/A"), true);
            }

            return result;
        }

        private bool GetFunctionName(string functionString, out string result)
        {
           var methodName = string.Empty;
            result = string.Empty;
            //Spit by "."
            var parts = functionString.Split('.');

            if (parts.Length <= 0) return false;

            for (int i = 0; i < parts.Length; i++)
            {
                var item = parts[i].Trim();
                if (item.EndsWith(";") && item.Contains("(") && item.Contains(")"))
                {
                    methodName = item.GetUntilOrEmpty();

                    if (_classes.Any(x => x.FunctionName == methodName))
                    {
                        QuickLogger.Debug($"Method Name = {methodName}", true);
                        result = methodName;
                        return true;
                    }
                }
            }

            QuickLogger.Message(string.Format(FCSDeepDrillerBuildable.InvalidFunctionFormat(),methodName), true);

            return false;
        }

        internal void Show()
        {
            uGUI.main.userInput.RequestString("Enter Function", "Execute Function", "Enter Function", 100, ImplementCommand);
        }

        private void ImplementCommand(string text)
        {
            ImplementCommand(text,out var function);
        }

        public bool HasUpgradeFunction(UpgradeFunctions upgradeEnum, out UpgradeFunction function)
        {
            function = null;

            if (Upgrades.Any(x => x.UpgradeType == upgradeEnum))
            {
                function = Upgrades.FirstOrDefault(x => x.UpgradeType == upgradeEnum);
                return true;
            }


            return false;
        }

        public void DeleteFunction(UpgradeFunction function)
        {
            QuickLogger.Debug("Deleting Function",true);
            Upgrades.Remove(function);
            OnUpgradeUpdate?.Invoke(null);
        }

        internal void Load(IEnumerable<UpgradeSave> functions)
        {
            if (functions == null) return;
            foreach (UpgradeSave function in functions)
            {
                if (string.IsNullOrEmpty(function.Function)) continue;
                ImplementCommand(function.Function, out var upgrade);

                if (upgrade != null)
                {
                    upgrade.IsEnabled = function.IsEnabled;
                }
            }

            OnUpgradeUpdate?.Invoke(null);
        }


        internal IEnumerable<UpgradeSave> Save()
        {
            foreach (UpgradeFunction upgrade in Upgrades)
            {
                yield return new UpgradeSave{Function = upgrade.Function , IsEnabled = upgrade.IsEnabled};
            }
        }
    }
}
