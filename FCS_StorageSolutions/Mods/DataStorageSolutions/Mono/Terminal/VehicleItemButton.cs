using FCS_AlterraHub.Mono;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class VehicleItemButton : OnScreenButton, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private ItemsContainer _itemContainer;
        private Text _indexNumber;

        public override void OnPointerClick(PointerEventData pointerEventData)
        {
            base.OnPointerClick(pointerEventData);
            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_itemContainer);
#if SUBNAUTICA
            pda.Open(PDATab.Inventory, null, null, 4f);
#else
            pda.Open(PDATab.Inventory);

#endif
        }

        public override void Update()
        {
            base.Update();
            if (_itemContainer != null)
                TextLineOne = $"Item Count: {_itemContainer.count}";
        }

        internal void Reset()
        {
            _itemContainer = null;
            TextLineOne = string.Empty;
            if (_indexNumber != null)
                _indexNumber.text = "0";
            gameObject.SetActive(false);
        }

        internal void Set(Vehicle vehicle, ItemsContainer itemContainer, int index)
        {
            if (_indexNumber == null)
            {
                _indexNumber = gameObject.GetComponentInChildren<Text>();
            }

            _itemContainer = itemContainer;
            _indexNumber.text = index.ToString();
            gameObject.SetActive(true);
        }
    }
}