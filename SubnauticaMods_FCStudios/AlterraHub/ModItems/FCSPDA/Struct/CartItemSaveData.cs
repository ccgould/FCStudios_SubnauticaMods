using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using Newtonsoft.Json;


namespace FCS_AlterraHub.ModItems.FCSPDA.Struct
{
    internal struct CartItemSaveData
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal TechType ReceiveTechType { get; set; }
        [JsonProperty] internal int ReturnAmount { get; set; }
        [JsonProperty] internal FCSAlterraHubGUISender Sender;
        //internal AlterraDronePortController Port { get; set; }

        public void Refund()
        {
            for (int i = 0; i < ReturnAmount; i++)
            {
                AccountService.main.Refund(Sender, ReceiveTechType);
            }
        }
    }
}
