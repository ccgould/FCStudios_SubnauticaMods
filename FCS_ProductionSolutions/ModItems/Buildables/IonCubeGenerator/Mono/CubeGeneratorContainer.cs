using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Interfaces;
using FCSCommon.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono;

internal class CubeGeneratorContainer : ICubeContainer
{
#if SUBNAUTICA
    private static readonly Vector2int CubeSize = CraftData.GetItemSize(TechType.PrecursorIonCrystal);
#elif BELOWZERO
    private static readonly Vector2int CubeSize = TechData.GetItemSize(TechType.PrecursorIonCrystal);
#endif
    internal GameObject CubePrefab;

    private const int ContainerHeight = 2;
    private const int ContainerWidth = 2;

    /// <summary>
    /// The maximum allowed storage in the container
    /// </summary>
    /// <returns>An <see cref="int"/> of storage slots</returns>
    internal const int MaxAvailableSpaces = ContainerHeight * ContainerWidth;

    private ItemsContainer _cubeContainer = null;
    private ChildObjectIdentifier _containerRoot = null;


    public int NumberOfCubes
    {
        get => _cubeContainer.count;
        set
        {
            if (value < 0 || value > MaxAvailableSpaces)
                return;

            if (value < _cubeContainer.count)
            {
                do
                {
                    RemoveSingleCube();
                } while (value < _cubeContainer.count);
            }
            else if (value > _cubeContainer.count)
            {
                do
                {
                    SpawnCube();
                } while (value > _cubeContainer.count);
            }
        }
    }

    public bool IsFull => _cubeContainer.count == MaxAvailableSpaces || !_cubeContainer.HasRoomFor(CubeSize.x, CubeSize.y);

    internal CubeGeneratorContainer(IonCubeGeneratorController cubeGenerator)
    {
        CoroutineHost.StartCoroutine(this.GetCubePrefab());

        _containerRoot = cubeGenerator.GetComponentInChildren<ChildObjectIdentifier>();

        if (_cubeContainer == null)
        {
            // Logger.Log(Logger.Level.Debug, "Initializing Container");
            _cubeContainer = new ItemsContainer(ContainerWidth, ContainerHeight, _containerRoot.transform, "N/A", null); //TODO CubeGeneratorBuildable.StorageLabel()

            _cubeContainer.isAllowedToAdd += IsAllowedToAdd;
            _cubeContainer.isAllowedToRemove += IsAllowedToRemove;

            //_cubeContainer.onAddItem += cubeGenerator.OnAddItemEvent;
            //_cubeContainer.onRemoveItem += cubeGenerator.OnRemoveItemEvent;
        }
    }

    private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
    {
        return false;
    }

    private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
    {
        return true;
    }

    public IEnumerator OpenStorage()
    {
        QuickLogger.Debug($"Storage Button Clicked", true);

        //Close FCSPDA so in game pda can open with storage
        FCSPDAController.Main.Close();

        QuickLogger.Debug($"Closing FCS PDA", true);

        QuickLogger.Debug("Attempting to open the In Game PDA", true);
        Player main = Player.main;
        PDA pda = main.GetPDA();

        while (pda != null && pda.isInUse || pda.isOpen)
        {
            QuickLogger.Debug("Waiting for In Game PDA Settings to reset", true);
            yield return null;
        }


        Inventory.main.SetUsedStorage(_cubeContainer, false);
        pda.Open(PDATab.Inventory, _cubeContainer.tr, (s) =>
        {
            QuickLogger.Debug("Container Closed", true);
        });

        yield break;
    }

    private IEnumerator GetCubePrefab()
    {
        CoroutineTask<GameObject> result = CraftData.GetPrefabForTechTypeAsync(TechType.PrecursorIonCrystal, false);
        yield return result;
        CubePrefab = result.GetResult();
        yield break;
    }

    internal bool SpawnCube()
    {
        if (this.IsFull)
            return false;

        var gameObject = GameObject.Instantiate(CubePrefab);
        CubePrefab.SetActive(false);
        gameObject.SetActive(true);

        Pickupable pickupable = gameObject.GetComponent<Pickupable>();
        pickupable.Pickup(false);
        var item = new InventoryItem(pickupable);

        _cubeContainer.UnsafeAdd(item);
        return true;
    }

    private void RemoveSingleCube()
    {
        IList<InventoryItem> cubes = _cubeContainer.GetItems(TechType.PrecursorIonCrystal);
        _cubeContainer.RemoveItem(cubes[0].item);
    }

    public void AttemptToOpenStorage(FCSPDAController fcspda)
    {
        //QuickLogger.Debug("Attempting to open the In Game PDA",true);


        //Player main = Player.main;
        //PDA pda = main.GetPDA();
        //Inventory.main.SetUsedStorage(_cubeContainer, false);
        //pda.Open(PDATab.Inventory, _cubeContainer.tr, (s)=>
        //{
        //    QuickLogger.Debug("Container Closed",true);            
        //});
    }
}