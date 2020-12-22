using System.Collections.Generic;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.SeaBreeze.Display;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    internal class CookerItemController : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        private GameObject _hover;
        private CookerItemDialog _dialog;
        public TechType TechType { get; private set; }
        public TechType CookedTechType { get; private set; }
        public TechType CuredTechType { get; set; }
        public CookerMode Mode { get; private set; }

        public void Initialize(KeyValuePair<TechType, TechType> cookingPair,CookerItemDialog dialog,CookerMode mode)
        {
            _dialog = dialog;
            TechType = cookingPair.Key;
            CookedTechType = cookingPair.Value;
            CuredTechType = cookingPair.Value;
            Mode = mode;
            if (mode == CookerMode.Cook || mode == CookerMode.Custom)
            {
                var icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(CookedTechType);
                TextLineOne = Language.main.Get(CookedTechType);
            }
            else if(mode == CookerMode.Cure)
            {
                var icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(CuredTechType);
                TextLineOne = Language.main.Get(CuredTechType);
            }

            _hover = gameObject.FindChild("Hover");
            TextLineTwo = AuxPatchers.IngredientsFormat(Language.main.Get(TechType));
            
        }

        

        public override void Update()
        {
            base.Update();
            if (_hover == null) return;
            _hover.SetActive(Inventory.main.container.Contains(TechType));
        }

        public override void OnPointerClick(PointerEventData pointerEventData)
        {
            base.OnPointerClick(pointerEventData);
            _dialog.Show(this);
        }

        public CookerItemDialog GetCookerItemDialog()
        {
            return _dialog;
        }
    }
}