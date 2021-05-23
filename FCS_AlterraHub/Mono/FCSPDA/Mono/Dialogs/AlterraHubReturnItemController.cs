using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono.Dialogs
{
    internal class AlterraHubReturnItemController : MonoBehaviour
    {
        internal InventoryItem  InventoryItem { get; set; }
        internal bool IsChecked => _toggle.isOn;

        private Toggle _toggle;


        internal void Initialize(InventoryItem item, Transform list)
        {
            InventoryItem = item;
            gameObject.FindChild("ItemName").GetComponent<Text>().text = $"Item: {Language.main.Get(item.item.GetTechType())}";
            _toggle = gameObject.GetComponentInChildren<Toggle>();
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.SetParent(list,false);
        }

        public void UnRegisterAndDestroy()
        {
            Destroy(gameObject);
        }
    }
}