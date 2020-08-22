using System;
using FCSCommon.Enums;
using FCSCommon.Extensions;
using FCSCommon.Objects;
using FCSCommon.Utilities;

namespace FCS_DeepDriller.Model.Upgrades
{
    internal class SilkTouchUpgrade : UpgradeFunction
    {
        public float Amount { get; set; }
        public override float PowerUsage => 0.5f;
        public override float Damage => 1f;
        public override UpgradeFunctions UpgradeType => UpgradeFunctions.SilkTouch;
        public override string FriendlyName => "Silk Touch";
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

        public override string Format()
        {
            return $"{FriendlyName} at {Amount}";
        }
    }
}