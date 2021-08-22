using System.Collections.Generic;
using System.Linq;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Model.GUI
{
    internal class Initialize_uGUI : MonoBehaviour
    {
        //Code made possible by senna7608 

        private bool _isPatched;
        private uGUI_EquipmentSlot _tempSlot;
        internal static Initialize_uGUI Instance { get; private set; }

        private const float Unit = 200f;
        private const float RowStep = Unit * 2.2f / 3;
        private const float TopRow = Unit;
        private const float SecondRow = TopRow - RowStep;
        private const float ThirdRow = SecondRow - RowStep;
        private const float FourthRow = ThirdRow - RowStep;
        private const float FifthRow = FourthRow - RowStep;
        private const float CenterColumn = 0f;
        private const float RightColumn = RowStep;
        private const float LeftColumn = -RowStep;

        private static readonly Vector2[] slotPos = new Vector2[12]
        {
            new Vector2(LeftColumn, TopRow), //slot 1
            new Vector2(CenterColumn, TopRow),  //slot 2
            new Vector2(RightColumn, TopRow),   //slot 3

            new Vector2(LeftColumn, SecondRow),  //slot 4
            new Vector2(CenterColumn, SecondRow), //slot 5
            new Vector2(RightColumn, SecondRow),   //slot 6

            new Vector2(LeftColumn, ThirdRow),  //slot 7
            new Vector2(CenterColumn, ThirdRow),  //slot 8
            new Vector2(RightColumn, ThirdRow),   //slot 9

            new Vector2(LeftColumn, FourthRow),   //slot 10
            new Vector2(CenterColumn, FourthRow),  //slot 11
            new Vector2(RightColumn, FourthRow)  //slot 12
        };


        public void Awake()
        {
            Instance = gameObject.GetComponent<Initialize_uGUI>();
        }

        internal void Add_uGUIslots(uGUI_Equipment instance, Dictionary<string, uGUI_EquipmentSlot> allSlots)
        {
            if (!_isPatched)
            {
                foreach (uGUI_EquipmentSlot slot in instance.GetComponentsInChildren<uGUI_EquipmentSlot>(true))
                {
                    if (slot.name == "PowerCellCharger1" || slot.name == "BatteryCharger1")
                    {
                        foreach (KeyValuePair<string, SlotInformation> slotID in EquipmentConfiguration.SlotIDs)
                        {
                            _tempSlot = Instantiate(slot, slot.transform.parent);
                            _tempSlot.name = slotID.Key;
                            _tempSlot.slot = slotID.Key;
                            allSlots.Add(slotID.Key, _tempSlot);
                        }
                        break;
                    }
                }

                foreach (KeyValuePair<string, uGUI_EquipmentSlot> item in allSlots)
                {
                    try
                    {
                        var postion = EquipmentConfiguration.SlotIDs[item.Value.name].Position;
                        item.Value.rectTransform.anchoredPosition = postion;
                    }
                    catch
                    {
                        QuickLogger.Error($"Add text to slot error!");
                    }
                }
                
                QuickLogger.Debug("uGUI_EquipmentSlots Patched!");
                _isPatched = true;
            }
        }

        public void OnDestroy()
        {
            Destroy(Instance);
        }

        public static Vector2 GetPositionForSlot(int i)
        {
            var index = i - 1;
            if (slotPos.Any() && index >= 0)
            {
                return slotPos[index];
            }

            return new Vector2();
        }
    }
}
