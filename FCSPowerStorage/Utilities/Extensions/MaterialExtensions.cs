using FCSTerminal.Logging;
using UnityEngine;

namespace FCSPowerStorage.Utilities.Extensions
{
    /**
    * Extension methods for the Material object.
    */
    public static class MaterialExtensions
    {
        public static void PrintAllMarmosetUBERShaderProperties(this Material material)
        {
            Log.Info(string.Format("Property {0} {1}", "_Color", material.GetVector("_Color")));
            Log.Info(string.Format("Property {0} {1}", "_Mode", material.GetFloat("_Mode")));
            Log.Info(string.Format("Property {0} {1}", "_SrcBlend", material.GetFloat("_SrcBlend")));
            Log.Info(string.Format("Property {0} {1}", "_DstBlend", material.GetFloat("_DstBlend")));
            Log.Info(string.Format("Property {0} {1}", "_SrcBlend2", material.GetFloat("_SrcBlend2")));
            Log.Info(string.Format("Property {0} {1}", "_DstBlend2", material.GetFloat("_DstBlend2")));
            Log.Info(string.Format("Property {0} {1}", "_AddSrcBlend", material.GetFloat("_AddSrcBlend")));
            Log.Info(string.Format("Property {0} {1}", "_AddDstBlend", material.GetFloat("_AddDstBlend")));
            Log.Info(string.Format("Property {0} {1}", "_AddSrcBlend2", material.GetFloat("_AddDstBlend2")));
            Log.Info(string.Format("Property {0} {1}", "_EnableMisc", material.GetFloat("_EnableMisc")));
            Log.Info(string.Format("Property {0} {1}", "_MyCullVariable", material.GetFloat("_MyCullVariable")));
            Log.Info(string.Format("Property {0} {1}", "_ZWrite", material.GetFloat("_ZWrite")));
            Log.Info(string.Format("Property {0} {1}", "_ZOffset", material.GetFloat("_ZOffset")));
            Log.Info(string.Format("Property {0} {1}", "_EnableCutOff", material.GetFloat("_EnableCutOff")));
            Log.Info(string.Format("Property {0} {1}", "_Cutoff", material.GetFloat("_Cutoff")));
            Log.Info(string.Format("Property {0} {1}", "_EnableDitherAlpha", material.GetFloat("_EnableDitherAlpha")));
            Log.Info(string.Format("Property {0} {1}", "_EnableVrFadeOut", material.GetFloat("_EnableVrFadeOut")));
            Log.Info(string.Format("Property {0} {1}", "_VrFadeMask", material.GetTexture("_VrFadeMask")?.name));
            Log.Info(string.Format("Property {0} {1}", "_EnableLighting", material.GetFloat("_EnableLighting")));
            Log.Info(string.Format("Property {0} {1}", "_IBLreductionAtNight", material.GetFloat("_IBLreductionAtNight")));
            Log.Info(string.Format("Property {0} {1}", "_EnableSimpleGlass", material.GetFloat("_EnableSimpleGlass")));
            Log.Info(string.Format("Property {0} {1}", "_EnableVertexColor", material.GetFloat("_EnableVertexColor")));
            Log.Info(string.Format("Property {0} {1}", "_EnableSchoolFish", material.GetFloat("_EnableSchoolFish")));
            Log.Info(string.Format("Property {0} {1}", "_EnableMainMaps", material.GetFloat("_EnableMainMaps")));
            Log.Info(string.Format("Property {0} {1}", "_MainTex", material.GetTexture("_MainTex")?.name));
            Log.Info(string.Format("Property {0} {1}", "_BumpMap", material.GetTexture("_BumpMap")?.name));
            Log.Info(string.Format("Property {0} {1}", "_MarmoSpecEnum", material.GetFloat("_MarmoSpecEnum")));
            Log.Info(string.Format("Property {0} {1}", "_SpecTex", material.GetTexture("_SpecTex")?.name));
            Log.Info(string.Format("Property {0} {1}", "_SpecColor", material.GetColor("_SpecColor")));
            Log.Info(string.Format("Property {0} {1}", "_SpecInt", material.GetFloat("_SpecInt")));
            Log.Info(string.Format("Property {0} {1}", "_Shininess", material.GetFloat("_Shininess")));
            Log.Info(string.Format("Property {0} {1}", "_Fresnel", material.GetFloat("_Fresnel")));
            Log.Info(string.Format("Property {0} {1}", "_EnableGlow", material.GetFloat("_EnableGlow")));
            Log.Info(string.Format("Property {0} {1}", "_GlowColor", material.GetVector("_GlowColor")));
            Log.Info(string.Format("Property {0} {1}", "_Illum", material.GetTexture("_Illum")?.name));
            Log.Info(string.Format("Property {0} {1}", "_GlowUVfromVC", material.GetFloat("_GlowUVfromVC")));
            Log.Info(string.Format("Property {0} {1}", "_GlowStrength", material.GetFloat("_GlowStrength")));
            Log.Info(string.Format("Property {0} {1}", "_GlowStrengthNight", material.GetFloat("_GlowStrengthNight")));
            Log.Info(string.Format("Property {0} {1}", "_EmissionLM", material.GetFloat("_EmissionLM")));
            Log.Info(string.Format("Property {0} {1}", "_EmissionLMNight", material.GetFloat("_EmissionLMNight")));
            Log.Info(string.Format("Property {0} {1}", "_EnableDetailMaps", material.GetFloat("_EnableDetailMaps")));
            Log.Info(string.Format("Property {0} {1}", "_DetailDiffuseTex", material.GetTexture("_DetailDiffuseTex")?.name));
            Log.Info(string.Format("Property {0} {1}", "_DetailBumpTex", material.GetTexture("_DetailBumpTex")?.name));
            Log.Info(string.Format("Property {0} {1}", "_DetailSpecTex", material.GetTexture("_DetailSpecTex")?.name));
            Log.Info(string.Format("Property {0} {1}", "_DetailIntensities", material.GetVector("_DetailIntensities")));
            Log.Info(string.Format("Property {0} {1}", "_EnableLightmap", material.GetFloat("_EnableLightmap")));
            Log.Info(string.Format("Property {0} {1}", "_Lightmap", material.GetTexture("_Lightmap")?.name));
            Log.Info(string.Format("Property {0} {1}", "_LightmapStrength", material.GetFloat("_LightmapStrength")));
            Log.Info(string.Format("Property {0} {1}", "_Enable3Color", material.GetFloat("_Enable3Color")));
            Log.Info(string.Format("Property {0} {1}", "_MultiColorMask", material.GetTexture("_MultiColorMask")?.name));
            Log.Info(string.Format("Property {0} {1}", "_Color2", material.GetVector("_Color2")));
            Log.Info(string.Format("Property {0} {1}", "_Color3", material.GetVector("_Color3")));
            Log.Info(string.Format("Property {0} {1}", "_SpecColor2", material.GetVector("_SpecColor2")));
            Log.Info(string.Format("Property {0} {1}", "_SpecColor3", material.GetVector("_SpecColor3")));
            Log.Info(string.Format("Property {0} {1}", "FX", material.GetFloat("FX")));
            Log.Info(string.Format("Property {0} {1}", "_DeformMap", material.GetTexture("_DeformMap")?.name));
            Log.Info(string.Format("Property {0} {1}", "_DeformParams", material.GetVector("_DeformParams")));
            Log.Info(string.Format("Property {0} {1}", "_FillSack", material.GetFloat("_FillSack")));
            Log.Info(string.Format("Property {0} {1}", "_OverlayStrength", material.GetFloat("_OverlayStrength")));
            Log.Info(string.Format("Property {0} {1}", "_GlowScrollColor", material.GetVector("_GlowScrollColor")));
            Log.Info(string.Format("Property {0} {1}", "_GlowScrollMask", material.GetTexture("_GlowScrollMask")?.name));
            Log.Info(string.Format("Property {0} {1}", "_Hypnotize", material.GetFloat("_Hypnotize")));
            Log.Info(string.Format("Property {0} {1}", "_ScrollColor", material.GetVector("_ScrollColor")));
            Log.Info(string.Format("Property {0} {1}", "_ColorStrength", material.GetVector("_ColorStrength")));
            Log.Info(string.Format("Property {0} {1}", "_ScrollTex", material.GetTexture("_ScrollTex")?.name));
            Log.Info(string.Format("Property {0} {1}", "_GlowMask", material.GetTexture("_GlowMask")?.name));
            Log.Info(string.Format("Property {0} {1}", "_GlowMaskSpeed", material.GetVector("_GlowMaskSpeed")));
            Log.Info(string.Format("Property {0} {1}", "_ScrollTex2", material.GetTexture("_ScrollTex2")?.name));
            Log.Info(string.Format("Property {0} {1}", "_ScrollSpeed", material.GetVector("_ScrollSpeed")));
            Log.Info(string.Format("Property {0} {1}", "_NoiseTex", material.GetTexture("_NoiseTex")?.name));
            Log.Info(string.Format("Property {0} {1}", "_DetailsColor", material.GetVector("_DetailsColor")));
            Log.Info(string.Format("Property {0} {1}", "_SquaresColor", material.GetVector("_SquaresColor")));
            Log.Info(string.Format("Property {0} {1}", "_SquaresTile", material.GetFloat("_SquaresTile")));
            Log.Info(string.Format("Property {0} {1}", "_SquaresSpeed", material.GetFloat("_SquaresSpeed")));
            Log.Info(string.Format("Property {0} {1}", "_SquaresIntensityPow", material.GetFloat("_SquaresIntensityPow")));
            Log.Info(string.Format("Property {0} {1}", "_NoiseSpeed", material.GetVector("_NoiseSpeed")));
            Log.Info(string.Format("Property {0} {1}", "_FakeSSSparams", material.GetVector("_FakeSSSparams")));
            Log.Info(string.Format("Property {0} {1}", "_FakeSSSSpeed", material.GetVector("_FakeSSSSpeed")));
            Log.Info(string.Format("Property {0} {1}", "_BorderColor", material.GetVector("_BorderColor")));
            Log.Info(string.Format("Property {0} {1}", "_Built", material.GetFloat("_Built")));
            Log.Info(string.Format("Property {0} {1}", "_BuildParams", material.GetVector("_BuildParams")));
            Log.Info(string.Format("Property {0} {1}", "_BuildLinear", material.GetFloat("_BuildLinear")));
            Log.Info(string.Format("Property {0} {1}", "_EmissiveTex", material.GetTexture("_EmissiveTex")?.name));
            Log.Info(string.Format("Property {0} {1}", "_NoiseThickness", material.GetFloat("_NoiseThickness")));
            Log.Info(string.Format("Property {0} {1}", "_NoiseStr", material.GetFloat("_NoiseStr")));
            Log.Info(string.Format("Property {0} {1}", "FX_Vertex", material.GetFloat("FX_Vertex")));
            Log.Info(string.Format("Property {0} {1}", "_Scale", material.GetVector("_Scale")));
            Log.Info(string.Format("Property {0} {1}", "_Frequency", material.GetVector("_Frequency")));
            Log.Info(string.Format("Property {0} {1}", "_Speed", material.GetVector("_Speed")));
            Log.Info(string.Format("Property {0} {1}", "_AnimMask", material.GetTexture("_AnimMask")?.name));
            Log.Info(string.Format("Property {0} {1}", "_ObjectUp", material.GetVector("_ObjectUp")));
            Log.Info(string.Format("Property {0} {1}", "_Fallof", material.GetFloat("_Fallof")));
            Log.Info(string.Format("Property {0} {1}", "_RopeGravity", material.GetFloat("_RopeGravity")));
            Log.Info(string.Format("Property {0} {1}", "_minYpos", material.GetFloat("_minYpos")));
            Log.Info(string.Format("Property {0} {1}", "_maxYpos", material.GetFloat("_maxYpos")));
            Log.Info(string.Format("Property {0} {1}", "_EnableBurst", material.GetFloat("_EnableBurst")));
            Log.Info(string.Format("Property {0} {1}", "_DispTex", material.GetTexture("_DispTex")?.name));
            Log.Info(string.Format("Property {0} {1}", "_Displacement", material.GetFloat("_Displacement")));
            Log.Info(string.Format("Property {0} {1}", "_BurstStrength", material.GetFloat("_BurstStrength")));
            Log.Info(string.Format("Property {0} {1}", "_Range", material.GetVector("_Range")));
            Log.Info(string.Format("Property {0} {1}", "_ClipRange", material.GetFloat("_ClipRange")));
            Log.Info(string.Format("Property {0} {1}", "_EnableInfection", material.GetFloat("_EnableInfection")));
            Log.Info(string.Format("Property {0} {1}", "_EnablePlayerInfection", material.GetFloat("_EnablePlayerInfection")));
            Log.Info(string.Format("Property {0} {1}", "_InfectionHeightStrength", material.GetFloat("_InfectionHeightStrength")));
            Log.Info(string.Format("Property {0} {1}", "_InfectionScale", material.GetVector("_InfectionScale")));
            Log.Info(string.Format("Property {0} {1}", "_InfectionOffset", material.GetVector("_InfectionOffset")));
            Log.Info(string.Format("Property {0} {1}", "_InfectionNoiseTex", material.GetTexture("_InfectionNoiseTex")?.name));
            Log.Info(string.Format("Property {0} {1}", "_InfectionSpeed", material.GetVector("_InfectionSpeed")));

            Log.Info("Shader keywords: " + material.shaderKeywords.Length);
            foreach (string keyword in material.shaderKeywords)
            {
                Log.Info("Keyword: " + keyword);
            }

            Log.Info("Render queue: " + material.renderQueue);
            Log.Info("Done print");
        }
    }
}
