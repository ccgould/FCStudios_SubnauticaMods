namespace FCS_AlterraHub.Model.Converters
{
    internal class Converters
    {
        internal static string DecimalToMoney(string moneyUnit, decimal amount)
        {
            return moneyUnit + amount.ToString("N");
        }
    }
}
