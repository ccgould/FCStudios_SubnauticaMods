using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Mono;
using FCS_StorageSolutions.Services;
using Nautilus.Commands;
using System;
using System.Collections;
using UnityEngine;
using UWE;

namespace FCS_StorageSolutions.Configuration;

internal class Commands
{
    private static TeleportScreenFXController _teleportEffects;

    [ConsoleCommand("AddToDSS")]
    public static string AddItemToDSS(string text, int amount)
    {
        //char[] separator = new char[]
        //{
        //    ' ',
        //    '\t'
        //};
        //string text = value.Trim();
        //string[] array = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        //if (array.Length == 0)
        //{
        //    return string.Empty;
        //}
        //string techTypeString = array[0];
        
        //int amount = 1;

 
        if (UWE.Utils.TryParseEnum<TechType>(text, out var techType))
        {
            if (CraftData.IsAllowed(techType))
            {
                //int number = 1;
                //int num;
                //if (array.Length > 1 && int.TryParse(array[1], out num))
                //{
                //    number = num;
                //}

                var habitat = FCSModsAPI.PublicAPI.GetPlayerHabitat();

                CoroutineHost.StartCoroutine(ItemCmdSpawnAsync(amount, techType, habitat));

                //for (int i = 0; i < amount; i++)
                //{
                //    DSSService.main.AddItemToBase(habitat, Inventory.main.container.RemoveItem(TechType.Copper).inventoryItem);
                //}
            }
        }
        return $"Parameters: {techType} {amount}";
    }

    private static IEnumerator ItemCmdSpawnAsync(int number, TechType techType,HabitatManager habitat)
    {
        TaskResult<GameObject> result = new TaskResult<GameObject>();
        int num;
        for (int i = 0; i < number; i = num + 1)
        {
            yield return CraftData.InstantiateFromPrefabAsync(techType, result, false);
            GameObject gameObject = result.Get();
            if (gameObject != null)
            {
                CrafterLogic.NotifyCraftEnd(gameObject, techType);
                Pickupable component = gameObject.GetComponent<Pickupable>();
                if (component != null)
                {
                    DSSService.main.AddItemToBase(habitat, new InventoryItem(component));
                }
            }
            num = i;
        }
        yield break;
    }
}
