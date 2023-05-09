using Nautilus.Handlers;

namespace FCS_AlterraHub.Core.Extensions;

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
    /// Turns string into a <see cref="TechType"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static TechType ToTechType(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return TechType.None;
        }

        // Look for a known TechType
        if (TechTypeExtensions.FromString(value, out TechType tType, true))
        {
            return tType;
        }

        //  Not one of the known TechTypes - is it registered with SMLHelper?

        if (EnumHandler.TryGetValue(value, out TechType custom))
        {
            return custom;
        }

        return TechType.None;
    }
}
