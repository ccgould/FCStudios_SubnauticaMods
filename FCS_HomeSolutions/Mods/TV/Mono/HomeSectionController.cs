using FCS_AlterraHub.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.TV.Mono
{
    internal class HomeSectionController : MonoBehaviour
    {
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}