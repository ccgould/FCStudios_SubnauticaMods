using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal.Enumerators;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class TerminalConfigurationPageController : MonoBehaviour, ITerminalPage
    {
        private DSSTerminalDisplayManager _mono;
        private FCSButton _moonpoolPage;
        private FCSButton _craftingPage;

        private void GoToPage(TerminalPages page)
        {
            _mono.GoToTerminalPage(page);
        }

        public void Initialize(DSSTerminalDisplayManager mono)
        {
            _mono = mono;
            _moonpoolPage = GameObjectHelpers.FindGameObject(gameObject, "MoonpoolBTN").EnsureComponent<FCSButton>();
            _moonpoolPage.TextLineOne = "Go to moonpool configuration.";
            _moonpoolPage.Subscribe(() => GoToPage(TerminalPages.MoonPoolSettings));

            _craftingPage = GameObjectHelpers.FindGameObject(gameObject, "CraftingBTN").EnsureComponent<FCSButton>();
            _craftingPage.TextLineOne = "Go to crafting configuration.";
            _craftingPage.Subscribe(() => GoToPage(TerminalPages.Crafting));
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}