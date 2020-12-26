using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Components;
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
    }
}