using FCS_Alterra_Refrigeration_Solutions.Models.Components;

namespace FCS_Alterra_Refrigeration_Solutions.Models.Base
{
    public class ItemButton : OnScreenButton
    {
        private TechType type = TechType.None;
        public int Amount { set; get; }

        public TechType Type
        {
            set
            {
                TextLineOne = "Take " + TechTypeExtensions.Get(Language.main, value);
                type = value;
            }

            get => type;
        }
    }
}
