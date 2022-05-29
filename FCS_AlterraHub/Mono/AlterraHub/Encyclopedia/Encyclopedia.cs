using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Mono.FCSPDA.Mono;

namespace FCS_AlterraHub.Mono.AlterraHub.Encyclopedia
{
    internal static class Encyclopedia
    {
        internal static Dictionary<string, FCSEncyclopediaData> EncyclopediaEntries = new Dictionary<string, FCSEncyclopediaData>
        {
            {"Ency_AlterraHubIntroduction",new FCSEncyclopediaData
                {
                    Title = "Introduction to Alterra Hub",
                    Body = @"If you are reading this and you have survived an emergency evacuation of a capital-class ship equipped with Alterra technology: Congratulations! The hard part is over. If you are reading this and have just arrived at a remote Alterra Facility: Congratulations! The hard part is just starting.

    A blueprint for the Alterra Hub has been uploaded to your PDA because your Alterra Universal Transponder is unable to establish connection to an Authorized Alterra Network. The blueprint directs the Alterra Hub to run in Autonomous Mode. 

    This operating mode has one directive: to ensure you never feel abandoned by Alterra Corp.

    The Alterra Hub brings you:
     - high-quality goods & services at excellent prices
     - a clear conscience, knowing you won't accidentally use a licensed item without paying royalties 
     - the peace of mind that comes with paying your debts on time


    The Fabricator and Builder Tool are designed to create objects from Alterra-Kits (made with Alterra Pre-Packaged Matter Technology) but can be adapted to use local materials in an emergency. Adaptability to planetary environments depends on the available materials, making some environments more challenging, or more deadly, than others. (Remember that materials you gather are the property of the Alterra Corporation and you will be liable to reimburse the full market price for all the materials you use.)

    Rather than cobbling together specific, possibly hard to find, local materials, the Alterra Hub lets you purchase Alterra-Kits from the available Digital Catalogs using your Local Account Balance.

    The Builder Tool will reformat your Alterra-Kits into your purchased product at your desired location. With Alterra Hub, you will never fear dying for want of a hunk of molybdenum while floating on an ocean of mercury (subject to available Local Credit).

    In an emergency, you may not have access to your Alterra Project Coordinator to monitor and provide your Job Assignments or authorize payments. In light of this, specialized blueprints for the Alterra Ore Consumer have been uploaded to your PDA to allow you to construct the Alterra Ore Consumer from local materials (for more information on the Alterra Ore Consumer see: Production > Alterra Ore Consumer).

    Collecting vital materials for Alterra Corporation along with diligent work in your Job Assignment will let you build your Local Account balance and service your debts in a timely manner.

    Psychological Health Notice: Not paying your debts has been shown to lead to anxiety and depression. Keep your spirits up by paying your debts at Alterra Hub!

    The Alterra Hub: keeping you and Alterra Corp connected no matter where you roam.
    ",
                    Image = API.FCSAssetBundlesService.PublicAPI.GetEncyclopediaSprite("AlterraHub", Mod.AssetBundleName)
                }
            }
        };
    }
}
