using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_ProductionSolutions.Buildable;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Mono
{
    internal class CrafterBeltController : MonoBehaviour
    {
        private GameObject _beltPathPoints;
        private Transform[] _crafterBeltPath;
        private int _itemsOnBelt;

        public AutoCrafterController Crafter;

        private void Start()
        {
            _beltPathPoints = GameObjectHelpers.FindGameObject(gameObject, "Belt1WayPoints");
            _crafterBeltPath = _beltPathPoints.GetChildrenT();
        }

        private void OnPathComplete()
        {
            if (_itemsOnBelt == 0) return;
            _itemsOnBelt -= 1;
            if (_itemsOnBelt < 0) _itemsOnBelt = 0;
        }

        internal bool ItemsOnBelt()
        {
            return _itemsOnBelt > 0;
        }

        internal void SpawnBeltItem(TechType techType)
        {
            if (_crafterBeltPath == null) return;

            //Spawn items
            var inv = GameObject.Instantiate(ModelPrefab.DSSCrafterCratePrefab);
            inv.transform.SetParent(_beltPathPoints.transform, false);
            var follow = inv.AddComponent<AutoCrafterCrateController>();
            follow.Initialize(_crafterBeltPath, Crafter, techType);
            follow.OnPathComplete += OnPathComplete;
            _itemsOnBelt++;
        }
    }
}
