using FCSPowerStorage.Logging;
using FCSPowerStorage.Model.Components;
using FCSPowerStorage.Utilities.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCSPowerStorage.Helpers
{
    public static class ColorChanger
    {
        //public static Color MainBodyColor { get; set; } = new Color(0.8359375f, 0.8359375f, 0.8359375f);

        //public static Material SystemLights { get; set; }

        public static Material BatteriesAndDetails { get; set; }
        public static Color CurrentColor { get; set; }
        public static Color Blue = new Color(0, 0.921875f, 0.9453125f);
        public static Color Red = new Color(1, 0, 0);
        public static Color Orange = new Color(0.99609375f, 0.62890625f, 0.01953125f);


        public static void ChangeBodyColor(GameObject powerStorage, FCSPowerStorageDisplay display, Color newColor)
        {
            display.CustomBatteryController.MainBodyColor = newColor;
            ApplyMaterials(powerStorage, newColor);
        }

        public static void ApplyMaterials(GameObject powerStorage, Color mainBodyColor)
        {
            Shader shader = Shader.Find("MarmosetUBER");

            MeshRenderer mainBaseBodyRenderer = powerStorage.FindChild("model").FindChild("MainBaseBody").GetComponent<MeshRenderer>();

            MeshRenderer batteriesAndDetailsRender = powerStorage.FindChild("model").FindChild("BatteriesAndDetails").GetComponent<MeshRenderer>();

            // == Materials == //
            var mainBaseBody = mainBaseBodyRenderer.materials[0];
            BatteriesAndDetails = batteriesAndDetailsRender.materials[0];

            // == Set Public Materials == //

            // Log.Info("MAIN BASE BODY");

            // == MAIN BASE BODY == //
            mainBaseBody.shader = shader;
            mainBaseBody.EnableKeyword("_METALLICGLOSSMAP");
            mainBaseBody.SetFloat("_Fresnel", 0f);
            mainBaseBody.SetColor("_Color", mainBodyColor);
            BatteriesAndDetails.SetFloat("_Glossiness", 0.5f);
            mainBaseBody.SetTexture("_MetallicGlossMap", FindTexture2D("Power_Storage_StorageBaseColor_MetallicSmoothness"));


            // == BATTERIES AND DETAILS == //
            BatteriesAndDetails.shader = shader;
            BatteriesAndDetails.EnableKeyword("_METALLICGLOSSMAP");
            BatteriesAndDetails.SetFloat("_Fresnel", 0f);
            BatteriesAndDetails.SetColor("_Color", new Color(0.0f, 0.0f, 0.0f));
            BatteriesAndDetails.SetFloat("_Glossiness", 1f);
            BatteriesAndDetails.SetTexture("_MetallicGlossMap", FindTexture2D("Power_Storage_Details_MetallicSmoothness"));
        }

        public static void ChangeSystemColor(GameObject powerStorage, Color systemLightsColor)
        {
            Shader shader = Shader.Find("MarmosetUBER");

            MeshRenderer systemLightsRender = powerStorage.FindChild("model").FindChild("SystemLights").GetComponent<MeshRenderer>();


            // == Materials == //
            var systemLights = systemLightsRender.materials[0];


            Log.Info($"SYSTEM LIGHTS Changed to {systemLightsColor}");

            // == SYSTEM LIGHTS == //
            systemLights.shader = shader;
            systemLights.EnableKeyword("MARMO_SPECMAP");
            systemLights.EnableKeyword("_ZWRITE_ON");
            systemLights.EnableKeyword("MARMO_EMISSION");
            systemLights.EnableKeyword("_EMISSION");
            systemLights.SetFloat("_EmissionLM", 0f);
            systemLights.SetVector("_EmissionColor", systemLightsColor);
            systemLights.SetColor("_Illum", systemLightsColor);
            systemLights.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
            systemLights.SetFloat("_EnableGlow", 1);
            systemLights.SetColor("_GlowColor", systemLightsColor);
            systemLights.SetFloat("_GlowStrength", 1f);
            systemLights.SetColor("_Color", systemLightsColor);
            systemLights.SetColor("_SpecColor", Color.white);
        }

        public static Texture2D FindTexture2D(string textureName)
        {
            List<object> textures = new List<object>(AssetHelper.Asset.LoadAllAssets(typeof(object)));

            //Log.Info("FindTexture2D");

            for (int i = 0; i < textures.Count; i++)
            {
                if (textures[i] is Texture2D)
                    if (((Texture2D)textures[i]).name.Equals(textureName))
                    {
                        Log.Info($"Found : {textureName} in Asset Bundle");
                        Log.Info(((Texture2D)textures[i]).NullOrID());
                        return ((Texture2D)textures[i]);
                    }
            }

            return null;
        }

        public static void ConfigureSystemLights(FCSPowerStates powerState, GameObject powerStorage)
        {
            switch (powerState)
            {
                case FCSPowerStates.Powered:
                    //_systemLightsColor = Blue;
                    ChangeSystemColor(powerStorage, Blue);
                    CurrentColor = Blue;
                    break;
                case FCSPowerStates.Unpowered:
                    //_systemLightsColor = Red;
                    ChangeSystemColor(powerStorage, Red);
                    CurrentColor = Red;
                    break;
                case FCSPowerStates.Buffer:
                    //_systemLightsColor = Red;
                    ChangeSystemColor(powerStorage, Orange);
                    CurrentColor = Orange;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(powerState), powerState, null);

            }
        }
    }
}
