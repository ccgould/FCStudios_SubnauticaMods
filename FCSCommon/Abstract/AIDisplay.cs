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

        public virtual IEnumerator PowerOff()
        {
            return null;
        }

        public virtual IEnumerator PowerOn()
        {
            return null;
        }

        public virtual IEnumerator ShutDown()
        {
            return null;
        }

        public virtual IEnumerator CompleteSetup()
        {
            return null;
        }
    }
}
