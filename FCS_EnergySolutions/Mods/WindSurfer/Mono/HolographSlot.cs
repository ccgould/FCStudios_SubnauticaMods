using System;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    public class HolographSlot : MonoBehaviour
    {
        public Transform Target { get; set; }
        internal WindSurferOperatorController WindSurferOperatorController => GetComponentInParent<WindSurferOperatorController>();
        internal PlatformController PlatformController => GetComponentInParent<PlatformController>();
        internal HoloGraphControl HoloGraphControl => GetComponentInParent<HoloGraphControl>();
        internal int ID;
        internal Vector2Int Direction
        {
            get
            {
                switch (ID)
                {
                    case 1:
                        return Vector2Int.up;
                    case 2:
                        return Vector2Int.left;
                    case 3:
                        return Vector2Int.down;
                    case 4:
                        return Vector2Int.right;
                    default:
                        return Vector2Int.zero;
                }
            }
        }

        void Start()
        {
            var idString = gameObject.name.Substring(gameObject.name.Length - 1);
            ID = Convert.ToInt32(idString);
            var button = gameObject.GetComponent<Button>();
            button.onClick.AddListener((() =>
            {
                WindSurferOperatorController.AddPlatform(this, WindSurferOperatorController.Grid.Position(HoloGraphControl) + Direction);
            }));
        }
    }
}