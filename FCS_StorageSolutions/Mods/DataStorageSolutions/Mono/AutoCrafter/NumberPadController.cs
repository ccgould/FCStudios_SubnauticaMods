using System.Text;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCSCommon.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class NumberPadController : MonoBehaviour
    {
        private GameObject[] _numbers;
        private readonly StringBuilder _sb = new StringBuilder();
        private Text _numberDisplay;
        private CraftingItem _craftItem;


        private void Awake()
        {
            _numbers = gameObject.FindChild("NumberPad").GetChildren();
            InterfaceHelpers.CreateButton(gameObject.FindChild("DeleteButton"), "DeleteBTN",
                InterfaceButtonMode.Background, OnButtonClicked, new Color(0.6980392f, 0.1333333f, 0.1333333f, 1),
                new Color(0.5754717f, 0.122152f, 0.122152f, 1), 2.5f);

            InterfaceHelpers.CreateButton(gameObject.FindChild("OkBTN"), "OkButton",
                InterfaceButtonMode.Background, OnButtonClicked, Color.white, new Color(0f,1f,1f,1f), 2.5f);


            _numberDisplay = gameObject.FindChild("NumberView").FindChild("Text").GetComponent<Text>();

            for (int i = 0; i < _numbers.Length; i++)
            {
                var pad = _numbers[i].AddComponent<PadButton>();
                pad.Initialize(i);
                pad.OnButtonClick += (s,o) =>
                {
                    AddItemToDisplay((int)o);
                };
            }
        }

        private void OnButtonClicked(string btnName, object Tag)
        {
            switch (btnName)
            {
                case "DeleteBTN":
                    if (_sb.Length == 1)
                    {
                        _sb.Clear();
                        _sb.Append("0");
                    }
                    else if (_sb.Length > 1)
                    {
                        _sb.Length--;
                    }


                    UpdateDisplay(_sb.ToString());
                    break;
                case "OkButton":
                    _craftItem.Amount = int.Parse(_sb.ToString());
                    Hide();
                    break;
            }
        }

        internal void Show(CraftingItem item)
        {
            _craftItem = item;
            gameObject.SetActive(true);
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        internal void AddItemToDisplay(int number)
        {
            if (number < 11 && _sb.Length + 1 < 5)
            {

                if (_sb.Length == 1 && _sb.ToString().Equals("0"))
                {
                    _sb.Length--;
                }

                _sb.Append(number);
                UpdateDisplay(_sb.ToString());
            }
        }

        private void UpdateDisplay(string value)
        {
            _numberDisplay.text = value;
        }

        private class PadButton : InterfaceButton
        {
            public void Initialize(int index)
            {
                STARTING_COLOR = Color.white;
                HOVER_COLOR = new Color(0, 1, 1, 1);
                ButtonMode = InterfaceButtonMode.Background;
                Tag = index;
            }
        }
    }
}