using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using FCS_AlterraHub.Core.Components.uGUIComponents;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.uGUI;
internal class DSSVehicleItemButton : MonoBehaviour
{
    private ItemsContainer _itemContainer;
    [SerializeField] private Text _indexNumber;
    private FCSToolTip _toolTip;

    private void Awake()
    {
        _toolTip = GetComponent<FCSToolTip>();
    }

    public void OnClick()
    {
        Player main = Player.main;
        PDA pda = main.GetPDA();
        Inventory.main.SetUsedStorage(_itemContainer);
        pda.Open(PDATab.Inventory);
    }

    public void Update()
    {
        if (_itemContainer != null)
            _toolTip.ToolTipStringDelegate += () =>
            {
                return $"Item Count: {_itemContainer.count}";
            }; 
    }

    internal void Reset()
    {
        _itemContainer = null;
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
