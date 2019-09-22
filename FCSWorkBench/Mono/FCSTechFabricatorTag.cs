using System.Collections;
using UnityEngine;

namespace FCSTechFabricator.Mono
{
    public class FCSTechFabricatorTag : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(EnableBoxCollider());
        }

        private IEnumerator EnableBoxCollider()
        {
            while (Player.main.IsInSub())
            {
                yield return null;
            }

            var bc = gameObject.GetComponent<BoxCollider>();
            if (!bc.enabled)
            {
                bc.enabled = true;
            }
        }
    }
}
