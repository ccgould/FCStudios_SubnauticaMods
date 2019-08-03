using System.Collections;
using UnityEngine;

namespace FCSCommon.Helpers
{
    public abstract class AIDisplay : MonoBehaviour
    {
        /// <summary>
        /// The max distance to the screen
        /// </summary>
        public float MAX_INTERACTION_DISTANCE { get; set; } = 2.5f;

        public int CurrentPage = 1;

        public int MaxPage = 1;

        public abstract void ClearPage();

        public int ITEMS_PER_PAGE;

        /// <summary>
        /// Changes page by a certain amount
        /// </summary>
        /// <param name="amountToChangePageBy"></param>
        public virtual void ChangePageBy(int amountToChangePageBy)
        {
            DrawPage(CurrentPage + amountToChangePageBy);
        }

        public abstract void OnButtonClick(string btnName, object tag);

        public abstract void ItemModified(TechType item, int newAmount = 0);

        public abstract bool FindAllComponents();

        public virtual void ShutDownDisplay()
        {
            StartCoroutine(ShutDown());
        }

        public virtual void PowerOnDisplay()
        {
            StartCoroutine(PowerOn());
        }

        public virtual void PowerOffDisplay()
        {
            StartCoroutine(PowerOff());
        }

        public abstract IEnumerator PowerOff();

        public abstract IEnumerator PowerOn();

        public abstract IEnumerator ShutDown();

        public abstract IEnumerator CompleteSetup();

        public abstract void DrawPage(int page);

        public virtual void UpdatePaginator() { }
    }
}
