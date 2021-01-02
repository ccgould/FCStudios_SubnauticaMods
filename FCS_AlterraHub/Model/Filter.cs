using System.Collections.Generic;
using System.Globalization;

namespace FCS_AlterraHub.Model
{
    public class Filter
    {
        public string Category;
        public List<TechType> Types = new List<TechType>();
        public bool IsUnknown { get; set; }
        public bool IsCategory() => !string.IsNullOrEmpty(Category);

        public Filter()
        {

        }

        public Filter(Filter filter)
        {
            Types = filter.Types;
            Category = filter.Category;
        }


        public string GetString()
        {
            if (IsCategory())
            {
                return Category;
            }

            var textInfo = (new CultureInfo("en-US", false)).TextInfo;
            return textInfo.ToTitleCase(Language.main.Get(Types[0]));
        }

        public bool IsTechTypeAllowed(TechType techType)
        {
            return Types.Contains(techType);
        }

        public bool IsSame(Filter other)
        {
            if (other.IsCategory() && other.Category.Equals(Category)) return true;

            return Category == other.Category && Types.Count > 0 && Types.Count == other.Types.Count && Types[0] == other.Types[0];
        }

        public bool HasTechType(TechType techType)
        {
            return Types.Contains(techType);
        }
    }
}