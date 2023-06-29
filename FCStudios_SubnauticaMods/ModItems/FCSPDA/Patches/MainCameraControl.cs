using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace FCS_AlterraHub.ModItems.FCSPDA.Patches;

[HarmonyPatch]
internal class MainCameraControl_Patches
{

    [HarmonyPatch(typeof(MainCameraControl), "OnUpdate")]
    [HarmonyPrefix]
    private static bool OnUpdate_Prefix(MainCameraControl __instance)
    {

        if(FCSPDAController.Main is null || !FCSPDAController.Main.isInUse)
        {
            return true;
        }


        float deltaTime = Time.deltaTime;
        if (__instance.underWaterTracker.isUnderWater)
        {
            __instance.swimCameraAnimation = Mathf.Clamp01(__instance.swimCameraAnimation + deltaTime);
        }
        else
        {
            __instance.swimCameraAnimation = Mathf.Clamp01(__instance.swimCameraAnimation - deltaTime);
        }
        float num = __instance.minimumY;
        float num2 = __instance.maximumY;
        Vector3 velocity = __instance.playerController.velocity;
        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        bool inExosuit = Player.main.inExosuit;
        bool flag4 = uGUI_BuilderMenu.IsOpen();
        if (Player.main != null)
        {
            flag = FCSPDAController.Main.isInUse;
            flag3 = (Player.main.motorMode == Player.MotorMode.Vehicle);
            flag2 = (flag || flag3 || __instance.cinematicMode);
            if (XRSettings.enabled && VROptions.gazeBasedCursor)
            {
                flag2 = (flag2 || flag4);
            }
        }
        if (flag2 != __instance.wasInLockedMode || __instance.lookAroundMode != __instance.wasInLookAroundMode)
        {
            __instance.camRotationX = 0f;
            __instance.camRotationY = 0f;
            __instance.wasInLockedMode = flag2;
            __instance.wasInLookAroundMode = __instance.lookAroundMode;
        }
        bool flag5 = (!__instance.cinematicMode || (__instance.lookAroundMode && !flag)) && __instance.mouseLookEnabled && (flag3 || AvatarInputHandler.main == null || AvatarInputHandler.main.IsEnabled() || Builder.isPlacing);
        if (flag3 && !XRSettings.enabled && !inExosuit)
        {
            flag5 = false;
        }
        Transform transform = __instance.transform;
        float num3 = (float)((flag || __instance.lookAroundMode || Player.main.GetMode() == Player.Mode.LockedPiloting) ? 1 : -1);
        if (!flag2 || (__instance.cinematicMode && !__instance.lookAroundMode))
        {
            __instance.cameraOffsetTransform.localEulerAngles = UWE.Utils.LerpEuler(__instance.cameraOffsetTransform.localEulerAngles, Vector3.zero, deltaTime * 5f);
        }
        else
        {
            transform = __instance.cameraOffsetTransform;
            __instance.rotationY = Mathf.LerpAngle(__instance.rotationY, 0f, PDA.deltaTime * 15f);
            __instance.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(__instance.transform.localEulerAngles.x, 0f, PDA.deltaTime * 15f), __instance.transform.localEulerAngles.y, 0f);
            __instance.cameraUPTransform.localEulerAngles = UWE.Utils.LerpEuler(__instance.cameraUPTransform.localEulerAngles, Vector3.zero, PDA.deltaTime * 15f);
        }
        if (!XRSettings.enabled)
        {
            Vector3 localPosition = __instance.cameraOffsetTransform.localPosition;
            localPosition.z = Mathf.Clamp(localPosition.z + PDA.deltaTime * num3 * 0.25f, 0f + __instance.camPDAZStart, __instance.camPDAZOffset + __instance.camPDAZStart);
            __instance.cameraOffsetTransform.localPosition = localPosition;
        }
        Vector2 vector = Vector2.zero;
        if (flag5 && FPSInputModule.current.lastGroup == null)
        {
            vector = GameInput.GetLookDelta();
            if (XRSettings.enabled && VROptions.disableInputPitch)
            {
                vector.y = 0f;
            }
            if (inExosuit)
            {
                vector.x = 0f;
            }
            vector *= Player.main.mesmerizedSpeedMultiplier;
        }
        __instance.UpdateCamShake();
        if (__instance.cinematicMode && !__instance.lookAroundMode)
        {
            __instance.camRotationX = Mathf.LerpAngle(__instance.camRotationX, 0f, deltaTime * 2f);
            __instance.camRotationY = Mathf.LerpAngle(__instance.camRotationY, 0f, deltaTime * 2f);
            __instance.transform.localEulerAngles = new Vector3(-__instance.camRotationY + __instance.camShake, __instance.camRotationX, 0f);
        }
        else if (flag2)
        {
            if (!XRSettings.enabled)
            {
                bool flag6 = !__instance.lookAroundMode || flag;
                bool flag7 = !__instance.lookAroundMode || flag;
                Vehicle vehicle = Player.main.GetVehicle();
                if (vehicle != null)
                {
                    flag6 = (vehicle.controlSheme != Vehicle.ControlSheme.Mech || flag);
                }
                __instance.camRotationX += vector.x;
                __instance.camRotationY += vector.y;
                __instance.camRotationX = Mathf.Clamp(__instance.camRotationX, -60f, 60f);
                __instance.camRotationY = Mathf.Clamp(__instance.camRotationY, -60f, 60f);
                if (flag7)
                {
                    __instance.camRotationX = Mathf.LerpAngle(__instance.camRotationX, 0f, PDA.deltaTime * 10f);
                }
                if (flag6)
                {
                    __instance.camRotationY = Mathf.LerpAngle(__instance.camRotationY, 0f, PDA.deltaTime * 10f);
                }
                __instance.cameraOffsetTransform.localEulerAngles = new Vector3(-__instance.camRotationY, __instance.camRotationX + __instance.camShake, 0f);
            }
        }
        else
        {
            __instance.rotationX += vector.x;
            __instance.rotationY += vector.y;
            __instance.rotationY = Mathf.Clamp(__instance.rotationY, __instance.minimumY, __instance.maximumY);
            __instance.cameraUPTransform.localEulerAngles = new Vector3(Mathf.Min(0f, -__instance.rotationY + __instance.camShake), 0f, 0f);
            transform.localEulerAngles = new Vector3(Mathf.Max(0f, -__instance.rotationY + __instance.camShake), __instance.rotationX, 0f);
        }
        __instance.UpdateStrafeTilt();
        Vector3 localEulerAngles = __instance.transform.localEulerAngles + new Vector3(0f, 0f, __instance.cameraAngleMotion.y * __instance.cameraTiltMod + __instance.strafeTilt + __instance.camShake * 0.5f);
        float num4 = 0f - __instance.skin;
        if (!flag2 && __instance.GetCameraBob())
        {
            float to = Mathf.Min(1f, velocity.magnitude / 5f);
            __instance.smoothedSpeed = UWE.Utils.Slerp(__instance.smoothedSpeed, to, deltaTime);
            num4 += (Mathf.Sin(Time.time * 6f) - 1f) * (0.02f + __instance.smoothedSpeed * 0.15f) * __instance.swimCameraAnimation;
        }
        if (__instance.impactForce > 0f)
        {
            __instance.impactBob = Mathf.Min(0.9f, __instance.impactBob + __instance.impactForce * deltaTime);
            __instance.impactForce -= Mathf.Max(1f, __instance.impactForce) * deltaTime * 5f;
        }
        num4 -= __instance.impactBob;
        num4 -= __instance.stepAmount;
        if (__instance.impactBob > 0f)
        {
            __instance.impactBob = Mathf.Max(0f, __instance.impactBob - Mathf.Pow(__instance.impactBob, 0.5f) * Time.deltaTime * 3f);
        }
        __instance.stepAmount = Mathf.Lerp(__instance.stepAmount, 0f, deltaTime * Mathf.Abs(__instance.stepAmount));
        __instance.transform.localPosition = new Vector3(0f, num4, 0f);
        __instance.transform.localEulerAngles = localEulerAngles;
        if (Player.main.motorMode == Player.MotorMode.Vehicle)
        {
            __instance.transform.localEulerAngles = Vector3.zero;
        }
        Vector3 localEulerAngles2 = new Vector3(0f, __instance.transform.localEulerAngles.y, 0f);
        Vector3 localPosition2 = __instance.transform.localPosition;
        if (XRSettings.enabled)
        {
            if (flag2 && !flag3)
            {
                localEulerAngles2.y = __instance.viewModelLockedYaw;
            }
            else
            {
                localEulerAngles2.y = 0f;
            }
            if (!flag3 && !__instance.cinematicMode)
            {
                if (!flag2)
                {
                    Quaternion rotation = __instance.playerController.forwardReference.rotation;
                    localEulerAngles2.y = (__instance.gameObject.transform.parent.rotation.GetInverse() * rotation).eulerAngles.y;
                }
                localPosition2 = __instance.gameObject.transform.parent.worldToLocalMatrix.MultiplyPoint(__instance.playerController.forwardReference.position);
            }
        }
        __instance.viewModel.transform.localEulerAngles = localEulerAngles2;
        __instance.viewModel.transform.localPosition = localPosition2;

        return false;

        ////QuickLogger.Debug("Main Camera Control Update", true);

        //bool flag = false;
        //bool flag2 = false;
        //bool flag3 = false;
        //bool flag4 = false;

        //if (Player.main != null)
        //{
        //    flag = FCSPDAController.Main.isInUse;
        //    QuickLogger.Debug($"IS FCSPDA InUse:{flag}",true);
        //    flag3 = (Player.main.motorMode == Player.MotorMode.Vehicle);
        //    flag2 = (flag || flag3 || __instance.cinematicMode);

        //    if (XRSettings.enabled && VROptions.gaze__instancedCursor)
        //    {
        //        flag2 = (flag2 || flag4);
        //        QuickLogger.Debug($"Flag after  XRS && VROptions:{flag2}",true);
        //    }
        //}

        //QuickLogger.Debug($"Condition Check {flag2 != __instance.wasInLockedMode || __instance.lookAroundMode != __instance.wasInLookAroundMode}", true);
        //if (flag2 != __instance.wasInLockedMode || __instance.lookAroundMode != __instance.wasInLookAroundMode)
        //{
        //    QuickLogger.Debug($"Fix Camera", true);
        //    __instance.camRotationX = 0f;
        //    __instance.camRotationY = 0f;
        //    __instance.wasInLockedMode = flag2;
        //    __instance.wasInLookAroundMode = __instance.lookAroundMode;
        //}
    }
}
