﻿using System.Collections.Generic;
using SMLHelper.Handlers;

namespace FCS_LifeSupportSolutions.Configuration
{
    internal static class AuxPatchers
    {
        private const string ModKey = "ALS";

        private static readonly Dictionary<string, string> LanguageDictionary = new Dictionary<string, string>
        {
            { $"{ModKey}_Clear","CLEAR"},
            { $"{ModKey}_Purchase","PURCHASE"},
            { $"{ModKey}_Cost","COST: {0}"},
            { $"{ModKey}_InvalidItem","INVALID ITEM NUMBER"},
            { $"{ModKey}_NoDebitCard","NO DEBIT CARD DETECTED"},
            { $"{ModKey}_NotEnoughCredit","NOT ENOUGH CREDIT"},
            { $"{ModKey}_NoMedKitsToTake","There are no MedKits please wait for more to generate."},
            { $"{ModKey}_HealPlayer","Heal Player"},
            { $"{ModKey}_TakeOxygen","TAKE OXYGEN"},
            { $"{ModKey}_TakeOxygenDesc","Gives you oxygen directly from the base utility unit"},
            { $"{ModKey}_HasPipeAttachment","Tank has pipe attached please disconnect pipe to deconstruct."},
        };

        internal static void AdditionalPatching()
        {
            foreach (KeyValuePair<string, string> languageEntry in LanguageDictionary)
            {
                LanguageHandler.SetLanguageLine(languageEntry.Key, languageEntry.Value);
            }
        }

        private static string GetLanguage(string key)
        {
            return LanguageDictionary.ContainsKey(key) ? Language.main.Get(key) : "N/A";
        }

        public static string Clear()
        {
            return GetLanguage($"{ModKey}_Clear");
        }

        public static string Purchase()
        {
            return GetLanguage($"{ModKey}_Purchase");
        }

        public static string CostFormat(decimal amount)
        {
            return string.Format(GetLanguage($"{ModKey}_Cost"),amount);
        }

        public static string NoInventorySpace()
        {
            return Language.main.Get("InventoryFull");
        }

        public static string InvalidItem()
        {
            return GetLanguage($"{ModKey}_InvalidItem");
        }

        public static string NoCard()
        {
            return GetLanguage($"{ModKey}_NoDebitCard");
        }

        public static string NotEnoughCredit()
        {
            return GetLanguage($"{ModKey}_NotEnoughCredit");
        }

        public static string TakeMedKit()
        {
            return Language.main.Get("MedicalCabinet_PickupMedKit");
        }

        public static string NoMedKitsToTake()
        {
            return GetLanguage($"{ModKey}_NoMedKitsToTake");
        }

        public static string HealPlayer()
        {
            return GetLanguage($"{ModKey}_HealPlayer");
        }

        public static string TakeOxygen()
        {
            return GetLanguage($"{ModKey}_TakeOxygen");
        }

        public static string TakeOxygenDesc()
        {
            return GetLanguage($"{ModKey}_TakeOxygenDesc");
        }

        public static string OxygenTankHasAttachment()
        {
            return GetLanguage($"{ModKey}_HasPipeAttachment");
        }
    }
}
