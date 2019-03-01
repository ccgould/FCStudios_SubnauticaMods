using FCSPowerStorage.Model.Components;
using FCSPowerStorage.Utilities.Enums;
using FCSTerminal.Logging;
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

        public static Color Blue = new Color(0, 0.921875f, 0.9453125f);
        public static Color Red = new Color(1, 0, 0);
        //private static Vector4 _systemLightsColor = Blue;

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

            //MeshRenderer systemLightsRender = powerStorage.FindChild("model").FindChild("SystemLights").GetComponent<MeshRenderer>();

            //GameObject batteryIndicators = powerStorage.FindChild("model").FindChild("BatteryLights");

            // == Materials == //
            var mainBaseBody = mainBaseBodyRenderer.materials[0];
            BatteriesAndDetails = batteriesAndDetailsRender.materials[0];
            //SystemLights = systemLightsRender.materials[0];

            // == Set Public Materials == //

            Log.Info("MAIN BASE BODY");

            // == MAIN BASE BODY == //
            mainBaseBody.shader = shader;
            //mainBaseBody.EnableKeyword("MARMO_SPECMAP");
            mainBaseBody.EnableKeyword("_METALLICGLOSSMAP");
            mainBaseBody.SetFloat("_Fresnel", 0f);
            mainBaseBody.SetColor("_Color", mainBodyColor);
            BatteriesAndDetails.SetFloat("_Glossiness", 0.5f);
            mainBaseBody.SetTexture("_MetallicGlossMap", FindTexture2D("Power_Storage_StorageBaseColor_MetallicSmoothness"));
            //mainBaseBody.SetColor("_SpecColor", Color.white);
            //mainBaseBody.SetFloat("_SpecInt", 1f);
            //mainBaseBody.SetFloat("_Shininess", 0.104f);


            // == BATTERIES AND DETAILS == //
            BatteriesAndDetails.shader = shader;
            //batteriesAndDetails.EnableKeyword("MARMO_SPECMAP");
            BatteriesAndDetails.EnableKeyword("_METALLICGLOSSMAP");
            BatteriesAndDetails.SetFloat("_Fresnel", 0f);
            BatteriesAndDetails.SetColor("_Color", new Color(0.0f, 0.0f, 0.0f));
            BatteriesAndDetails.SetFloat("_Glossiness", 1f);
            BatteriesAndDetails.SetTexture("_MetallicGlossMap", FindTexture2D("Power_Storage_Details_MetallicSmoothness"));
            //batteriesAndDetails.SetColor("_SpecColor", Color.white);
            //batteriesAndDetails.SetFloat("_SpecInt", 1f);
            //batteriesAndDetails.SetFloat("_Shininess", 0.1f);

            //Log.Info("BATTERIES AND DETAILS");

            //// == SYSTEM LIGHTS == //
            //SystemLights.shader = shader;
            //SystemLights.EnableKeyword("MARMO_SPECMAP");
            //SystemLights.EnableKeyword("_ZWRITE_ON");
            //SystemLights.EnableKeyword("MARMO_EMISSION");
            //SystemLights.EnableKeyword("_EMISSION");
            //SystemLights.SetFloat("_EmissionLM", 0f);
            //SystemLights.SetVector("_EmissionColor", _systemLightsColor);
            //SystemLights.SetColor("_Illum", _systemLightsColor);
            //SystemLights.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
            //SystemLights.SetFloat("_EnableGlow", 1);
            //SystemLights.SetColor("_GlowColor", _systemLightsColor);
            //SystemLights.SetFloat("_GlowStrength", 1f);
            //SystemLights.SetColor("_Color", _systemLightsColor);
            //SystemLights.SetColor("_SpecColor", Color.white);




            //batteriesAndDetails.PrintAllMarmosetUBERShaderProperties();

            //batteriesAndDetails.EnableKeyword("MARMO_SPECMAP");
            //batteriesAndDetails.SetTexture("_MetallicGlossMap", FindTexture2D("Power_Storage_Details_Metallic"));



            Log.Info("BATTERY IND.");

            // == BATTERY IND. == //
            //foreach (Transform curindicator in batteryIndicators.transform)
            //{
            //    Log.Info(curindicator.ToString());
            //    var indicator = curindicator.GetComponent<MeshRenderer>().material;
            //    indicator.shader = shader;
            //    indicator.EnableKeyword("_EMISSION");
            //    indicator.EnableKeyword("MARMO_EMISSION");
            //    indicator.EnableKeyword("MARMO_SPECMAP");
            //    indicator.SetColor("_Color", new Color(0, 0.921875f, 0.9453125f));
            //    indicator.SetColor("_SpecColor", Color.white);
            //    indicator.SetFloat("_EmissionLM", 0f);
            //    indicator.SetVector("_EmissionColor", Vector4.zero);
            //    indicator.SetColor("_GlowColor", new Color(0, 0.921875f, 0.9453125f));
            //    indicator.SetFloat("_GlowStrength", 0.1f);
            //    indicator.SetFloat("_EnableGlow", 1.3f);
            //}

            //batteryIndicator_Top_L.SetFloat("_EmissionLM", 0f);
            //batteryIndicator_Top_L.SetVector("_EmissionColor", new Color(66, 217, 219, 255));
            //batteryIndicator_Top_L.SetFloat("_EnableGlow", 1.3f);


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
                    break;
                case FCSPowerStates.Unpowered:
                    //_systemLightsColor = Red;
                    ChangeSystemColor(powerStorage, Red);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(powerState), powerState, null);

            }
        }


    }
}
