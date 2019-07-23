using SMLHelper.V2.Handlers;
using System;

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
    }
}
