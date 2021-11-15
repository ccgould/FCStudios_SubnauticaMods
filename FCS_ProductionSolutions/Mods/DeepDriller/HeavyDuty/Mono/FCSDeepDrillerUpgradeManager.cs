using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Models.Upgrades;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Structs;
using FCS_ProductionSolutions.Mods.DeepDriller.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using FCSDeepDrillerBuildable = FCS_ProductionSolutions.Mods.DeepDriller.LightDuty.Buildable.FCSDeepDrillerBuildable;

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono
{
    internal class FCSDeepDrillerUpgradeManager : MonoBehaviour
    {
        internal readonly List<UpgradeClass> Classes = new List<UpgradeClass>
        {
            new UpgradeClass{FunctionName = "OresPerDay", FriendlyName = "Ores Per Day", Template = "os.OresPerDay({Amount});"},
            new UpgradeClass{FunctionName = "SilkTouch", FriendlyName = "Silk Touch"},
            new UpgradeClass{FunctionName = "MinOreCount", FriendlyName = "Minimum Ore Count"},
            new UpgradeClass{FunctionName = "MaxOreCount", FriendlyName = "Maximum Ore Count",Template = "os.MaxOreCount({TechType},{Amount});"},
            new UpgradeClass{FunctionName = "AutoStartUpAt", FriendlyName = "Auto Start Down At"},
            new UpgradeClass{FunctionName = "AutoShutDownAt", FriendlyName = "Auto Shut Down At"},
            new UpgradeClass{FunctionName = "ConnectToBase", FriendlyName = "Connect to base",Template = "os.ConnectToBase({BaseID});"},
        };
        
        public List<UpgradeFunction> Upgrades { get; set; } = new List<UpgradeFunction>();
        public Action<UpgradeFunction> OnUpgradeUpdate { get; set; }
        private string _textHistory = "Enter Function";

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

                if (!functionName) return;

                var parameters = GetMethodParameters(functionString, out var paraResults);

                if (!parameters) return;

                switch (fNameResult)
                {
                    case "OresPerDay":

                        OresPerDayImplementation(functionString, ref upgrade, paraResults);
                        break;

                    case "MaxOreCount":

                        MaxOreCountImplementation(functionString, ref upgrade, paraResults);
                        break;

                    case "MinOreCount":

                        MinOreCountImplementation(functionString, ref upgrade, paraResults);
                        break;

                    case "SilkTouch":

                        SilkTouchImplementation(functionString, ref upgrade, paraResults);
                        break;

                    case "AutoShutDownAt":

                        AutoShutDownImplementation(functionString, ref upgrade, paraResults);
                        break;

                    case "AutoStartUpAt":

                        AutoStartUpImplementation(functionString, ref upgrade, paraResults);
                        break;
                    case "ConnectToBase":

                        ConnectToBaseImplementation(functionString, ref upgrade, paraResults);
                        break;
                }
            }
        }

        private void ConnectToBaseImplementation(string functionString, ref UpgradeFunction upgrade, string[] paraResults)
        {
            var connectToBaseCheck = Upgrades.Any(x =>
                x.UpgradeType == UpgradeFunctions.ConnectToBase);

            if (connectToBaseCheck)
            {
                QuickLogger.Message("Function already implemented.", true);
                return;
            }

            bool connectToBaseValid = ConnectToBaseUpgrade.IsValid(paraResults, out var connectToBase);

            if (connectToBaseValid)
            {
                QuickLogger.Debug($"Function  Valid: {functionString}", true);
                var connectToBaseUpgrade = new ConnectToBaseUpgrade {IsEnabled = true, Mono = _mono, BaseName = connectToBase.BaseFriendlyID};
                Upgrades.Add(connectToBaseUpgrade);
                upgrade = connectToBaseUpgrade;
                connectToBaseUpgrade.TriggerUpdate();
                OnUpgradeUpdate?.Invoke(connectToBaseUpgrade);
            }
            else
            {
                QuickLogger.Debug($"Invalid Function: {functionString}", true);
            }
        }

        private void AutoStartUpImplementation(string functionString, ref UpgradeFunction upgrade, string[] paraResults)
        {
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
                var autoStartUpAtUpgrade = new AutoStartUpAtUpgrade {IsEnabled = true, Mono = _mono, Time = autoStartUpAt};
                Upgrades.Add(autoStartUpAtUpgrade);
                upgrade = autoStartUpAtUpgrade;
                OnUpgradeUpdate?.Invoke(autoStartUpAtUpgrade);
            }
            else
            {
                QuickLogger.Debug($"Invalid Function: {functionString}", true);
            }
        }

        private void AutoShutDownImplementation(string functionString, ref UpgradeFunction upgrade, string[] paraResults)
        {
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
                var autoShutDownAtUpgrade = new AutoShutDownAtUpgrade {IsEnabled = true, Mono = _mono, Time = autoShutdownAt};
                Upgrades.Add(autoShutDownAtUpgrade);
                upgrade = autoShutDownAtUpgrade;
                OnUpgradeUpdate?.Invoke(autoShutDownAtUpgrade);
            }
            else
            {
                QuickLogger.Debug($"Invalid Function: {functionString}", true);
            }
        }

        private void SilkTouchImplementation(string functionString, ref UpgradeFunction upgrade, string[] paraResults)
        {
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
                    TechType = silkTouchCheckTuple.Item1
                };
                Upgrades.Add(silkTouchUpgrade);
                upgrade = silkTouchUpgrade;
                OnUpgradeUpdate?.Invoke(silkTouchUpgrade);
            }
            else
            {
                QuickLogger.Debug($"Invalid Function: {functionString}", true);
            }
        }

        private void MinOreCountImplementation(string functionString, ref UpgradeFunction upgrade, string[] paraResults)
        {
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
                    TechType = minTuple.Item1
                };
                Upgrades.Add(maxOreCountUpgrade);
                upgrade = maxOreCountUpgrade;
                OnUpgradeUpdate?.Invoke(maxOreCountUpgrade);
            }
            else
            {
                QuickLogger.Debug($"Invalid Function: {functionString}", true);
            }
        }

        private void MaxOreCountImplementation(string functionString, ref UpgradeFunction upgrade, string[] paraResults)
        {
            bool maxOreCountValid = MaxOreCountUpgrade.IsValid(paraResults, out var tuple);

            if (maxOreCountValid)
            {
                QuickLogger.Debug($"Function  Valid: {functionString}", true);
                var maxOreCountCheck = Upgrades.Any(x =>
                    x.UpgradeType == UpgradeFunctions.MaxOreCount && ((MaxOreCountUpgrade) x).TechType == tuple.Item1);

                if (maxOreCountCheck)
                {
                    QuickLogger.Message("Function already implemented. Changing value", true);
                    var maxOreCountFunc = Upgrades.Single(x =>
                        x.UpgradeType == UpgradeFunctions.MaxOreCount && ((MaxOreCountUpgrade) x).TechType == tuple.Item1);
                    ((MaxOreCountUpgrade) maxOreCountFunc).Amount = tuple.Item2;
                    return;
                }

                var maxOreCountUpgrade = new MaxOreCountUpgrade
                {
                    IsEnabled = true,
                    Mono = _mono,
                    Amount = tuple.Item2,
                    TechType = tuple.Item1
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
        }

        private void OresPerDayImplementation(string functionString, ref UpgradeFunction upgrade, string[] paraResults)
        {
            var check = Upgrades.Any(x => x.UpgradeType == UpgradeFunctions.OresPerDay);

            if (check)
            {
                QuickLogger.Message("Function already implemented. Updating Value", true);
                var upgradeFunc = Upgrades.Single(x => x.UpgradeType == UpgradeFunctions.OresPerDay);
                bool validFunc = OresPerDayUpgrade.IsValid(paraResults, out var orePerDayFunc);
                if (validFunc)
                {
                    ((OresPerDayUpgrade) upgradeFunc).OreCount = orePerDayFunc;
                }
                else
                {
                    QuickLogger.Debug($"Invalid Function: {functionString}", true);
                }

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
                QuickLogger.Message(FCSDeepDrillerBuildable.InvalidClassFormat(parts.Length > 0 ? parts[0] : "N/A"), true);
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

                    QuickLogger.Debug($"Method Name = {methodName}", true);

                    if (Classes.Any(x => x.FunctionName.Equals(methodName)))
                    {
                        QuickLogger.Debug($"Method Name = {methodName}", true);
                        result = methodName;
                        return true;
                    }
                }
            }

            QuickLogger.Message(FCSDeepDrillerBuildable.InvalidFunctionFormat(methodName), true);

            return false;
        }

        internal void Show()
        {
            uGUI.main.userInput.RequestString("Enter Function", "Execute Function", _textHistory, 100, ImplementCommand);
        }

        private void ImplementCommand(string text)
        {
            _textHistory = text;
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
            function.DeActivateUpdate();
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

        public void SetUpdateDialogText(string function)
        {
            _textHistory = function;
        }
    }
}
