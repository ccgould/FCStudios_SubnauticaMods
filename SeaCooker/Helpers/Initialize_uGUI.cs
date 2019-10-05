using FCSCommon.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace AE.SeaCooker.Helpers
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

        private readonly Vector2[] slotPos = new Vector2[12]
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
                    if (slot.name == "PowerCellCharger1")
                    {
                        foreach (string slotID in Configuration.Configuration.SlotIDs)
                        {
                            _tempSlot = Instantiate(slot, slot.transform.parent);
                            _tempSlot.name = slotID;
                            _tempSlot.slot = slotID;
                            allSlots.Add(slotID, _tempSlot);
                        }
                        break;
                    }
                }


                foreach (KeyValuePair<string, uGUI_EquipmentSlot> item in allSlots)
                {
                    try
                    {
                        if (item.Value.name.StartsWith("SC"))
                        {
                            int.TryParse(item.Key.Substring(9), out int slotNum);

                            switch (slotNum)
                            {
                                case 1:
                                    item.Value.rectTransform.anchoredPosition = slotPos[5];
                                    break;
                            }
                        }
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
    }
}
