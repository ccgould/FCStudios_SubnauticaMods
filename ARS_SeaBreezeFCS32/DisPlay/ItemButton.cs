namespace ARS_SeaBreezeFCS32.Display
{
    internal class ItemButton : OnScreenButton
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
