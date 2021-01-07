using System.Collections;
using UnityEngine;

namespace FCSCommon.Abstract
{
    internal abstract class AIDisplay : MonoBehaviour
    {
        /// <summary>
        /// The max distance to the screen
        /// </summary>
        public float MAX_INTERACTION_DISTANCE { get; set; } = 2.5f;

        public abstract void OnButtonClick(string btnName, object tag);

        public abstract bool FindAllComponents();

        public virtual void PowerOnDisplay()
        {
        }

        public virtual void HibernateDisplay()
        {
        }

        public virtual void TurnOnDisplay()
        {
        }

        public virtual void TurnOffDisplay()
        {
        }

        public virtual void GoToPage(int index)
        {
            
        }
    }
}
