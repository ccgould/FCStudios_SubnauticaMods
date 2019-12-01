using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSTechFabricator.Helpers;
using UnityEngine;

namespace FCSTechFabricator.Models
{
    internal class FCSTechFabController: MonoBehaviour
    {
        private bool _initalized;

        private void OnEnable()
        {
            if (_initalized)
            {
                PatchHelpers.RegisterItems();
                _initalized = true;
            }
        }
    }
}
