using Harmony;

namespace FCS_DeepDriller.Patchers
{
    // Token: 0x02000019 RID: 25
    [HarmonyPatch(typeof(Builder), "CheckAsSubModule")]
    internal class Builder_CheckAsSubModule_Patch
    {
        //// Token: 0x060000DF RID: 223 RVA: 0x000044A4 File Offset: 0x000026A4
        //private static bool Prefix(ref bool __result)
        //{
        //    if (!Builder.prefab.GetComponent<FCSDeepDrillerController>())
        //    {
        //        return true;
        //    }
        //    __result = false;
        //    Transform aimTransform = Builder.GetAimTransform();
        //    Builder.placementTarget = null;
        //    RaycastHit hit;
        //    if (Physics.Raycast(aimTransform.position, aimTransform.forward, out hit, Builder.placeMaxDistance, Builder.placeLayerMask.value, QueryTriggerInteraction.Ignore))
        //    {
        //        Builder.SetPlaceOnSurface(hit, ref Builder.placePosition, ref Builder.placeRotation);
        //        return false;
        //    }
        //    if (Builder.placePosition.y > 0f)
        //    {
        //        return false;
        //    }
        //    __result = Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds, Builder.placeLayerMask.value, hit.collider);
        //    return false;
        //}
    }
}
