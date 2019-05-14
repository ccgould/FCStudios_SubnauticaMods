namespace FCSPowerStorage.Utilities.Enums
{
    /// <summary>
    /// The toggle states for the FCS Power Storage
    /// </summary>
    public enum PowerToggleStates
    {
        /// <summary>
        /// Allows the battery to charge and release power
        /// </summary>
        TrickleMode,

        /// <summary>
        /// Allows the battery to charge only
        /// </summary>
        ChargeMode,

        /// <summary>
        /// Default State on load
        /// </summary>
        None,
    }
}
