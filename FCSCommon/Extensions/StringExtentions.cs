using System;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCSCommon.Extensions
{
    public static class StringExtentions
    {
        /// <summary>
        /// Removes (Instance) from strings)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string RemoveInstance(this string name)
        {
            return name.Replace("(Instance)", string.Empty).Trim();
        }

        /// <summary>
        /// Removes (Clone) from strings)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string RemoveClone(this string name)
        {
            return name.Replace("(Clone)", string.Empty).Trim();
        }

        /// <summary>
        /// Converts string to a TechType if not found returns none.
        /// </summary>
        /// <param name="techType">The string to be checked against.</param>
        /// <returns></returns>
        public static TechType ToTechType(this string techType)
        {
            var result = TechType.None;

            // Check if the TechType exist
            if (!Enum.IsDefined(typeof(TechType), techType))
            {
                //If the item is an custom item try to find the TechType if not found report an error

                if (TechTypeHandler.TryGetModdedTechType(techType, out TechType customTechType))
                {
                    return customTechType;
                }
            }
            else
            {
                result = (TechType)Enum.Parse(typeof(TechType), techType);
            }

            return result;
        }

        public static Pickupable ToPickupable(this string techtype)
        {
            switch (techtype.ToLower())
            {
                case "titanium":
                    var titanium = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.Titanium));
                    return titanium.GetComponent<Pickupable>().Pickup(false);

                case "gold":
                    var gold = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/gold"));
                    return gold.GetComponent<Pickupable>().Pickup(false);

                case "lead":
                    var lead = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/Lead"));
                    return lead.GetComponent<Pickupable>().Pickup(false);

                case "lithium":
                    var lithium = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/lithium"));
                    return lithium.GetComponent<Pickupable>().Pickup(false);

                case "silver":
                    var silver = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/silver"));
                    return silver.GetComponent<Pickupable>().Pickup(false);
                case "quartz":
                    var quartz = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/quartz"));
                    return quartz.GetComponent<Pickupable>().Pickup(false);

                case "copper":
                    var copper = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/copper"));
                    return copper.GetComponent<Pickupable>().Pickup(false);

                case "diamond":
                    var diamond = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/diamond"));
                    return diamond.GetComponent<Pickupable>().Pickup(false);

                case "aluminumoxide":
                    var aluminumOxide = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/aluminumoxide"));
                    return aluminumOxide.GetComponent<Pickupable>().Pickup(false);

                case "uraninitecrystal":
                    var uraniniteCrystal = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/uraninitecrystal"));
                    return uraniniteCrystal.GetComponent<Pickupable>().Pickup(false);

                case "magnetite":
                    var magnetite = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/magnetite"));
                    return magnetite.GetComponent<Pickupable>().Pickup(false);

                case "nickel":
                    var nickel = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/nickel"));
                    return nickel.GetComponent<Pickupable>().Pickup(false);

                case "sulphur":
                    var sulphurcrystal = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/sulphurcrystal"));
                    return sulphurcrystal.GetComponent<Pickupable>().Pickup(false);

                case "kyanite":
                    var kyanite = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/Natural/kyanite"));
                    return kyanite.GetComponent<Pickupable>().Pickup(false);
                default:
                    return null;
            }
        }
    }
}
