namespace FCS_DeepDriller.Model.Upgrades
{
    internal class UpgradeClass
    {
        internal string FunctionName { get; set; }
        internal string FriendlyName { get; set; }



        public override string ToString()
        {
            return FriendlyName;
        }
    }
}