using System.Text;
using UnityEngine;

namespace FCSCommon.Extensions
{
    /**
    * Extension methods for the Material object.
    */
    public static class MaterialExtensions
    {
        public static string PrintAllMarmosetUBERShaderProperties(this Material material)
        {
            var sb = new StringBuilder();
            sb.Append($"Property {"_Color"} {material.GetVector("_Color")}");
            sb.Append($"Property {"_Mode"} {material.GetFloat("_Mode")}");
            sb.Append($"Property {"_SrcBlend"} {material.GetFloat("_SrcBlend")}");
            sb.Append($"Property {"_DstBlend"} {material.GetFloat("_DstBlend")}");
            sb.Append($"Property {"_SrcBlend2"} {material.GetFloat("_SrcBlend2")}");
            sb.Append($"Property {"_DstBlend2"} {material.GetFloat("_DstBlend2")}");
            sb.Append($"Property {"_AddSrcBlend"} {material.GetFloat("_AddSrcBlend")}");
            sb.Append($"Property {"_AddDstBlend"} {material.GetFloat("_AddDstBlend")}");
            sb.Append($"Property {"_AddSrcBlend2"} {material.GetFloat("_AddDstBlend2")}");
            sb.Append($"Property {"_EnableMisc"} {material.GetFloat("_EnableMisc")}");
            sb.Append($"Property {"_MyCullVariable"} {material.GetFloat("_MyCullVariable")}");
            sb.Append($"Property {"_ZWrite"} {material.GetFloat("_ZWrite")}");
            sb.Append($"Property {"_ZOffset"} {material.GetFloat("_ZOffset")}");
            sb.Append($"Property {"_EnableCutOff"} {material.GetFloat("_EnableCutOff")}");
            sb.Append($"Property {"_Cutoff"} {material.GetFloat("_Cutoff")}");
            sb.Append($"Property {"_EnableDitherAlpha"} {material.GetFloat("_EnableDitherAlpha")}");
            sb.Append($"Property {"_EnableVrFadeOut"} {material.GetFloat("_EnableVrFadeOut")}");
            sb.Append($"Property {"_VrFadeMask"} {material.GetTexture("_VrFadeMask")?.name}");
            sb.Append($"Property {"_EnableLighting"} {material.GetFloat("_EnableLighting")}");
            sb.Append($"Property {"_IBLreductionAtNight"} {material.GetFloat("_IBLreductionAtNight")}");
            sb.Append($"Property {"_EnableSimpleGlass"} {material.GetFloat("_EnableSimpleGlass")}");
            sb.Append($"Property {"_EnableVertexColor"} {material.GetFloat("_EnableVertexColor")}");
            sb.Append($"Property {"_EnableSchoolFish"} {material.GetFloat("_EnableSchoolFish")}");
            sb.Append($"Property {"_EnableMainMaps"} {material.GetFloat("_EnableMainMaps")}");
            sb.Append($"Property {"_MainTex"} {material.GetTexture("_MainTex")?.name}");
            sb.Append($"Property {"_BumpMap"} {material.GetTexture("_BumpMap")?.name}");
            sb.Append($"Property {"_MarmoSpecEnum"} {material.GetFloat("_MarmoSpecEnum")}");
            sb.Append($"Property {"_SpecTex"} {material.GetTexture("_SpecTex")?.name}");
            sb.Append($"Property {"_SpecColor"} {material.GetColor("_SpecColor")}");
            sb.Append($"Property {"_SpecInt"} {material.GetFloat("_SpecInt")}");
            sb.Append($"Property {"_Shininess"} {material.GetFloat("_Shininess")}");
            sb.Append($"Property {"_Fresnel"} {material.GetFloat("_Fresnel")}");
            sb.Append($"Property {"_EnableGlow"} {material.GetFloat("_EnableGlow")}");
            sb.Append($"Property {"_GlowColor"} {material.GetVector("_GlowColor")}");
            sb.Append($"Property {"_Illum"} {material.GetTexture("_Illum")?.name}");
            sb.Append($"Property {"_GlowUVfromVC"} {material.GetFloat("_GlowUVfromVC")}");
            sb.Append($"Property {"_GlowStrength"} {material.GetFloat("_GlowStrength")}");
            sb.Append($"Property {"_GlowStrengthNight"} {material.GetFloat("_GlowStrengthNight")}");
            sb.Append($"Property {"_EmissionLM"} {material.GetFloat("_EmissionLM")}");
            sb.Append($"Property {"_EmissionLMNight"} {material.GetFloat("_EmissionLMNight")}");
            sb.Append($"Property {"_EnableDetailMaps"} {material.GetFloat("_EnableDetailMaps")}");
            sb.Append($"Property {"_DetailDiffuseTex"} {material.GetTexture("_DetailDiffuseTex")?.name}");
            sb.Append($"Property {"_DetailBumpTex"} {material.GetTexture("_DetailBumpTex")?.name}");
            sb.Append($"Property {"_DetailSpecTex"} {material.GetTexture("_DetailSpecTex")?.name}");
            sb.Append($"Property {"_DetailIntensities"} {material.GetVector("_DetailIntensities")}");
            sb.Append($"Property {"_EnableLightmap"} {material.GetFloat("_EnableLightmap")}");
            sb.Append($"Property {"_Lightmap"} {material.GetTexture("_Lightmap")?.name}");
            sb.Append($"Property {"_LightmapStrength"} {material.GetFloat("_LightmapStrength")}");
            sb.Append($"Property {"_Enable3Color"} {material.GetFloat("_Enable3Color")}");
            sb.Append($"Property {"_MultiColorMask"} {material.GetTexture("_MultiColorMask")?.name}");
            sb.Append($"Property {"_Color2"} {material.GetVector("_Color2")}");
            sb.Append($"Property {"_Color3"} {material.GetVector("_Color3")}");
            sb.Append($"Property {"_SpecColor2"} {material.GetVector("_SpecColor2")}");
            sb.Append($"Property {"_SpecColor3"} {material.GetVector("_SpecColor3")}");
            sb.Append($"Property {"FX"} {material.GetFloat("FX")}");
            sb.Append($"Property {"_DeformMap"} {material.GetTexture("_DeformMap")?.name}");
            sb.Append($"Property {"_DeformParams"} {material.GetVector("_DeformParams")}");
            sb.Append($"Property {"_FillSack"} {material.GetFloat("_FillSack")}");
            sb.Append($"Property {"_OverlayStrength"} {material.GetFloat("_OverlayStrength")}");
            sb.Append($"Property {"_GlowScrollColor"} {material.GetVector("_GlowScrollColor")}");
            sb.Append($"Property {"_GlowScrollMask"} {material.GetTexture("_GlowScrollMask")?.name}");
            sb.Append($"Property {"_Hypnotize"} {material.GetFloat("_Hypnotize")}");
            sb.Append($"Property {"_ScrollColor"} {material.GetVector("_ScrollColor")}");
            sb.Append($"Property {"_ColorStrength"} {material.GetVector("_ColorStrength")}");
            sb.Append($"Property {"_ScrollTex"} {material.GetTexture("_ScrollTex")?.name}");
            sb.Append($"Property {"_GlowMask"} {material.GetTexture("_GlowMask")?.name}");
            sb.Append($"Property {"_GlowMaskSpeed"} {material.GetVector("_GlowMaskSpeed")}");
            sb.Append($"Property {"_ScrollTex2"} {material.GetTexture("_ScrollTex2")?.name}");
            sb.Append($"Property {"_ScrollSpeed"} {material.GetVector("_ScrollSpeed")}");
            sb.Append($"Property {"_NoiseTex"} {material.GetTexture("_NoiseTex")?.name}");
            sb.Append($"Property {"_DetailsColor"} {material.GetVector("_DetailsColor")}");
            sb.Append($"Property {"_SquaresColor"} {material.GetVector("_SquaresColor")}");
            sb.Append($"Property {"_SquaresTile"} {material.GetFloat("_SquaresTile")}");
            sb.Append($"Property {"_SquaresSpeed"} {material.GetFloat("_SquaresSpeed")}");
            sb.Append($"Property {"_SquaresIntensityPow"} {material.GetFloat("_SquaresIntensityPow")}");
            sb.Append($"Property {"_NoiseSpeed"} {material.GetVector("_NoiseSpeed")}");
            sb.Append($"Property {"_FakeSSSparams"} {material.GetVector("_FakeSSSparams")}");
            sb.Append($"Property {"_FakeSSSSpeed"} {material.GetVector("_FakeSSSSpeed")}");
            sb.Append($"Property {"_BorderColor"} {material.GetVector("_BorderColor")}");
            sb.Append($"Property {"_Built"} {material.GetFloat("_Built")}");
            sb.Append($"Property {"_BuildParams"} {material.GetVector("_BuildParams")}");
            sb.Append($"Property {"_BuildLinear"} {material.GetFloat("_BuildLinear")}");
            sb.Append($"Property {"_EmissiveTex"} {material.GetTexture("_EmissiveTex")?.name}");
            sb.Append($"Property {"_NoiseThickness"} {material.GetFloat("_NoiseThickness")}");
            sb.Append($"Property {"_NoiseStr"} {material.GetFloat("_NoiseStr")}");
            sb.Append($"Property {"FX_Vertex"} {material.GetFloat("FX_Vertex")}");
            sb.Append($"Property {"_Scale"} {material.GetVector("_Scale")}");
            sb.Append($"Property {"_Frequency"} {material.GetVector("_Frequency")}");
            sb.Append($"Property {"_Speed"} {material.GetVector("_Speed")}");
            sb.Append($"Property {"_AnimMask"} {material.GetTexture("_AnimMask")?.name}");
            sb.Append($"Property {"_ObjectUp"} {material.GetVector("_ObjectUp")}");
            sb.Append($"Property {"_Fallof"} {material.GetFloat("_Fallof")}");
            sb.Append($"Property {"_RopeGravity"} {material.GetFloat("_RopeGravity")}");
            sb.Append($"Property {"_minYpos"} {material.GetFloat("_minYpos")}");
            sb.Append($"Property {"_maxYpos"} {material.GetFloat("_maxYpos")}");
            sb.Append($"Property {"_EnableBurst"} {material.GetFloat("_EnableBurst")}");
            sb.Append($"Property {"_DispTex"} {material.GetTexture("_DispTex")?.name}");
            sb.Append($"Property {"_Displacement"} {material.GetFloat("_Displacement")}");
            sb.Append($"Property {"_BurstStrength"} {material.GetFloat("_BurstStrength")}");
            sb.Append($"Property {"_Range"} {material.GetVector("_Range")}");
            sb.Append($"Property {"_ClipRange"} {material.GetFloat("_ClipRange")}");
            sb.Append($"Property {"_EnableInfection"} {material.GetFloat("_EnableInfection")}");
            sb.Append($"Property {"_EnablePlayerInfection"} {material.GetFloat("_EnablePlayerInfection")}");
            sb.Append($"Property {"_InfectionHeightStrength"} {material.GetFloat("_InfectionHeightStrength")}");
            sb.Append($"Property {"_InfectionScale"} {material.GetVector("_InfectionScale")}");
            sb.Append($"Property {"_InfectionOffset"} {material.GetVector("_InfectionOffset")}");
            sb.Append($"Property {"_InfectionNoiseTex"} {material.GetTexture("_InfectionNoiseTex")?.name}");
            sb.Append($"Property {"_InfectionSpeed"} {material.GetVector("_InfectionSpeed")}");

            sb.Append("Shader keywords: " + material.shaderKeywords.Length);
            foreach (string keyword in material.shaderKeywords)
            {
                sb.Append("Keyword: " + keyword);
            }

            sb.Append("Render queue: " + material.renderQueue);
            sb.Append("Done print");

            return sb.ToString();
        }
    }
}
