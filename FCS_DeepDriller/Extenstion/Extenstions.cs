using System.Reflection;

namespace FCS_DeepDriller.Extenstion
{
    internal static class Extenstions
    {
        public static object GetPrivateField<T>(this T instance, string fieldName, BindingFlags bindingFlags = BindingFlags.Default)
        {
            return typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | bindingFlags).GetValue(instance);
        }
    }
}
