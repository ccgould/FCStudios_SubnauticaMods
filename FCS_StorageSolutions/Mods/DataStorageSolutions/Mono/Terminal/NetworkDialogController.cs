using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class NetworkDialogController : InterfaceButton
    {
        private BaseListController _dialog;

        internal void Initialize(BaseManager manager, GameObject root, DSSTerminalDisplayManager displayController)
        {
            TextLineOne = AuxPatchers.GlobalNetwork();
            TextLineTwo = AuxPatchers.GlobalNetworkDesc();
            STARTING_COLOR = Color.white;
            HOVER_COLOR = new Color(0f,1f,1f,1f);
            _dialog = GameObjectHelpers.FindGameObject(root, "BaseList").EnsureComponent<BaseListController>();
            _dialog.Initialize(manager, displayController);
        }

        internal BaseListController GetList()
        {
            return _dialog;
        }

        public override void OnPointerClick(PointerEventData pointerEventData)
        {
            base.OnPointerClick(pointerEventData);
            _dialog.ToggleVisibility();
        }

        public void Hide()
        {
            _dialog.Hide();
        }

        public void Refresh(SubRoot subRoot)
        {
            _dialog.UpdateList();
        }
    }
}