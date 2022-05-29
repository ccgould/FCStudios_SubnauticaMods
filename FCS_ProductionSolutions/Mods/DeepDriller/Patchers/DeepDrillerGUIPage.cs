using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Patchers
{
    public abstract class DeepDrillerGUIPage : MonoBehaviour
    {
        protected bool IsBeingDestroyed;
        
        public DeepDrillerHUD Hud { get; set; }

        public virtual void Awake()
        {
            var backBtn = gameObject.FindChild("BackBTN")?.GetComponent<Button>();
            backBtn?.onClick.AddListener((() =>
            {
                Hud.GoToPage(DeepDrillerHudPages.Home);
            }));
        }

        private void OnDestroy()
        {
            IsBeingDestroyed = true;
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }


    }
}