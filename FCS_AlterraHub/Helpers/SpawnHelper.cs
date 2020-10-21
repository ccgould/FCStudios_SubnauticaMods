using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Helpers
{
    public static class SpawnHelper
    {
        public static GameObject SpawnAtPoint(string location, Transform trans, float scale = 0.179f)
        {
            var obj = GameObject.Instantiate(Resources.Load<GameObject>(location));
            MaterialHelpers.ChangeWavingSpeed(obj,new Vector4(0f,0f,0.10f,0f));
            obj.transform.SetParent(trans,false);
            obj.transform.position = trans.position;
            obj.transform.localScale *= scale;
            Object.Destroy(obj.GetComponent<Rigidbody>());
            Object.Destroy(obj.GetComponent<WorldForces>());
            return obj;
        }
    }
}
