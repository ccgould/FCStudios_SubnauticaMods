using FCS_LifeSupportSolutions.ModItems.Buildables.BaseOxygenTank.Mono.Service;
using System;
using UnityEngine;

namespace FCS_LifeSupportSolutions.ModItems.Buildables.BaseOxygenTank.Mono;
internal class BaseOxygenTankManager : MonoBehaviour
{
    private Base baseComponent;

    private int activeBaseOxygenTankCount;

    private void Awake()
    {
        baseComponent = gameObject.GetComponent<Base>();
        BaseOxygenTankService.main.RegisterBaseOxygenTankManager(gameObject.GetComponent<PrefabIdentifier>().id, this);
    }

    private void OnDestroy()
    {
        BaseOxygenTankService.main.UnRegisterBaseOxygenTankManager(gameObject.GetComponent<PrefabIdentifier>().id);

    }

    public float GetRequiredTankCount(bool hardcore)
    {
        float bigRooms = 0;
        float smallRooms = 0;

        foreach (Int3 cell in baseComponent.AllCells)
        {
            Base.CellType cellType = baseComponent.GetCell(cell);

            switch (cellType)
            {
                case Base.CellType.Corridor:
                    smallRooms += 1;
                    break;
                case Base.CellType.Observatory:
                    smallRooms += 1;
                    break;
                case Base.CellType.MapRoom:
                    if (hardcore)
                        bigRooms += 1f / 9f;
                    else
                        smallRooms += 1f / 9f;
                    break;
                case Base.CellType.MapRoomRotated:
                    if (hardcore)
                        bigRooms += 1f / 9f;
                    else
                        smallRooms += 1f / 9f;
                    break;
                case Base.CellType.Moonpool:
                    bigRooms += 1f / 12f;
                    break;
                case Base.CellType.Room:
                    bigRooms += 1f / 9f;
                    break;
            }
        }

        return (float)Math.Round(bigRooms / (hardcore ? 1 : 2) + smallRooms / (hardcore ? 4 : 10), 2);
    }

    internal void AddActiveTank()
    {
        activeBaseOxygenTankCount++;
    }

    internal void RemoveActiveTank()
    {
        activeBaseOxygenTankCount--;
    }

    internal int GetActiveTankCount()
    { 
        return activeBaseOxygenTankCount;
    }
}
