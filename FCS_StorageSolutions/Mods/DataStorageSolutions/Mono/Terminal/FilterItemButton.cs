using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class FilterItemButton : OnScreenButton, IPointerEnterHandler, IPointerExitHandler
    {
        private Text _techTypeText;
        private InterfaceButton _deleteButton;
        private TechType _techType;
        public MoonPoolPageController MoonPoolPage { get; set; }


        internal void Reset()
        {
            TextLineOne = string.Empty;
            gameObject.SetActive(false);
        }

        internal void Set(TechType techType)
        {
            if (_deleteButton == null)
            {
                var deleteBTN = gameObject.FindChild("DeleteBTN");
                _deleteButton = InterfaceHelpers.CreateButton(deleteBTN, "DeleteBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f);
            }

            if (_techTypeText == null)
            {
                _techTypeText = gameObject.GetComponentInChildren<Text>();
            }

            _techType = techType;
            _techTypeText.text = Language.main.Get(techType);
            gameObject.SetActive(true);
        }

        private void OnButtonClick(string btnName, object Tag)
        {
            switch (btnName)
            {
                case "DeleteBTN":
                    MoonPoolPage.GetManager().DockingBlackList.Remove(_techType);
                    Reset();
                    MoonPoolPage?.RefreshBlackListItems();
                    break;
            }
        }
    }
}