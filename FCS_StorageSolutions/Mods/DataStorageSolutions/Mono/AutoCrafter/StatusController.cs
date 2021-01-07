using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class StatusController : MonoBehaviour
    {
        internal uGUI_Icon Slot1Icon;
        internal Text Slot1Text;
        internal uGUI_Icon Slot2Icon;
        internal Text Slot2Text;
        private bool _isInititalized;

        private void Initialize()
        {
            if (_isInititalized) return;
            Slot1Icon = gameObject.FindChild("ProductSlot1").FindChild("Icon").AddComponent<uGUI_Icon>();
            Slot1Icon.sprite = SpriteManager.defaultSprite;
            Slot1Text = gameObject.FindChild("ProductSlot1").FindChild("ItemCount").GetComponent<Text>();

            Slot2Icon = gameObject.FindChild("ProductSlot2").FindChild("Icon").AddComponent<uGUI_Icon>();
            Slot2Icon.sprite = SpriteManager.defaultSprite;
            Slot2Text = gameObject.FindChild("ProductSlot2").FindChild("ItemCount").GetComponent<Text>();
            _isInititalized = true;

        }

        internal void RefreshSlotOne(TechType techType, int amount, int total)
        {
            Initialize();
            Slot1Icon.sprite = SpriteManager.Get(techType);
            Slot1Text.text = $"{amount}/{total}";
        }

        internal void RefreshSlotTwo(TechType techType, int amount, int total)
        {
            Initialize();
            Slot2Icon.sprite = SpriteManager.Get(techType);
            Slot2Text.text = $"{amount}/{total}";
        }

        public void Reset()
        {
            RefreshSlotOne(TechType.None, 0, 0);
            RefreshSlotTwo(TechType.None, 0, 0);
        }
    }
}