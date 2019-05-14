using UnityEngine;


namespace FCSCommon.Components
{
    public class CommonItemsContainer : HandTarget, IHandTarget
    {
        private bool _isDisabled;
        private string _label;
        public Transform Transform { get; set; }
        public ItemsContainer ItemsContainer { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }


        private void Startup()
        {
            
        }

        public void CreateContainer(int width, int height, Transform tr, string label, FMODAsset errorSound = null)
        {
            ItemsContainer = new ItemsContainer(width, height, tr, label, errorSound);
            ItemsContainer.Resize(width, height);
            Transform = tr;
            _label = label;
            isValidHandTarget = true;
        }

        public void DisableContainer()
        {
            if (ItemsContainer != null)
            {
                _isDisabled = true;
            }
        }

        public void EnableContainer()
        {
            if (ItemsContainer != null)
            {
                _isDisabled = false;
            }
        }
        
        public void OnHandHover(GUIHand hand)
        {
            var main = HandReticle.main;
            main.SetInteractText($"Open label", false, HandReticle.Hand.Left);
            main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {

            //if (Transform == null)
            //{
            //    ErrorMessage.AddMessage($"Transform is null");
            //    return;
            //}

            //if (ItemsContainer == null)
            //{
            //    ErrorMessage.AddMessage($"Items Container is null");
            //    return;
            //}

            //PDA pda = Player.main.GetPDA();
            //Inventory.main.SetUsedStorage(ItemsContainer);
            //pda.Open(PDATab.Inventory, Transform);
        }
    }
}
