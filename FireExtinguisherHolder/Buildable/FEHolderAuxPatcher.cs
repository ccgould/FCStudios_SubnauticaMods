using SMLHelper.V2.Handlers;

namespace MAC.FireExtinguisherHolder.Buildable
{
    internal partial class FEHolderBuildable
    {
        #region Private Members
        private const string OnHolderNotEmptyKey = "MAC_HolderNotEmpty";
        #endregion

        #region Internal Properties
        internal static string BuildableName { get; private set; }
        internal static TechType TechTypeID { get; private set; }
        #endregion

        #region Private Methods
        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            LanguageHandler.SetLanguageLine(OnHolderNotEmptyKey, "Cannot deconstruct please remove extinguisher first.");
        }
        #endregion

        #region Internal Methods
        internal static string OnHandOverEmpty()
        {
            return Language.main.Get("ReplaceFireExtinguisher");
        }

        internal static string HolderNotEmptyMessage()
        {
            return Language.main.Get(OnHolderNotEmptyKey);
        }
        #endregion

        internal static string OnHandOverNotEmpty()
        {
            return Language.main.Get("TakeFireExtinguisher");
        }
    }
}
