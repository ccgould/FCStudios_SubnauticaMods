using System.Collections;
using UnityEngine;

namespace FCSTechFabricator.Mono
{
    public class FCSTechFabricatorTag : MonoBehaviour
    {
        private Coroutine _enableBoxcollider;

        private void Start()
        {
            //Start a coroutine to wait for player to leave the cyclops to apply the box collider
            _enableBoxcollider =  StartCoroutine(EnableBoxCollider());
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

        private void OnDestroy()
        {
            //Stop the coroutine if not null
            if (_enableBoxcollider == null) return;
            StopCoroutine(_enableBoxcollider);
        }
    }
}
