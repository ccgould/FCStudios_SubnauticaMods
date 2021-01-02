using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.FormattingStation
{
    internal class FilterDisplayItem : MonoBehaviour
    {
        private Filter _filter;
        private bool _isInitialized;
        private Text _itemLBL;

        internal void Initialize(DSSFormattingStationController controller)
        {
            if (_isInitialized) return;
            _itemLBL = InterfaceHelpers.FindGameObject(gameObject, "TechType").GetComponent<Text>();
            var deleteBTN = GameObjectHelpers.FindGameObject(gameObject, "DeleteBTN").AddComponent<InterfaceButton>();
            deleteBTN.ButtonMode = InterfaceButtonMode.Background;
            deleteBTN.OnButtonClick += (s, o) =>
            {
                gameObject.SetActive(false);
                controller.RemoveFilter(_filter);

            };
            _isInitialized = true;
        }

        public void Set(Filter filter)
        {
            _filter = filter;
            _itemLBL.text = filter.IsCategory() ? filter.Category : Language.main.Get(filter.Types[0]);
            gameObject.SetActive(true);

        }
        public void Reset()
        {
            gameObject.SetActive(false);
        }
    }
}