using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Enumerators;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal;
    internal class uGUI_MultiplierButton : MonoBehaviour
{
    [SerializeField] private Text _label;
    [SerializeField] private DSSTerminalController controller;
    [SerializeField] private string TextLineOne;
    [SerializeField] private string TextLineTwo;


    private void Start()
    {
        UpdateLabel();
    }

    public void OnClick()
    {

        if (_label != null)
        {
            switch (controller.BulkMultiplier)
            {
                case BulkMultipliers.TimesTen:
                    controller.BulkMultiplier = BulkMultipliers.TimesOne;
                    break;
                case BulkMultipliers.TimesEight:
                    controller.BulkMultiplier = BulkMultipliers.TimesTen;
                    break;
                case BulkMultipliers.TimesSix:
                    controller.BulkMultiplier = BulkMultipliers.TimesEight;
                    break;
                case BulkMultipliers.TimesFour:
                    controller.BulkMultiplier = BulkMultipliers.TimesSix;
                    break;
                case BulkMultipliers.TimesTwo:
                    controller.BulkMultiplier = BulkMultipliers.TimesFour;
                    break;
                case BulkMultipliers.TimesOne:
                    controller.BulkMultiplier = BulkMultipliers.TimesTwo;
                    break;
            }

            UpdateLabel();
        }
    }

    internal void UpdateLabel()
    {
        _label.text = $"x{(int)controller.BulkMultiplier}";
    }
}
