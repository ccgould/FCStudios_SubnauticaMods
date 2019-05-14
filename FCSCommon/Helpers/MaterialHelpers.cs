using System;
using System.Collections.Generic;
using System.Text;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSCommon.Helpers
{
    public class MaterialHelpers
    {
        public static Texture2D FindTexture2D(string textureName, AssetBundle assetBundle)
        {
            List<object> objects = new List<object>(assetBundle.LoadAllAssets(typeof(object)));


            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] is Texture2D)
                {
                    if (((Texture2D)objects[i]).name.Equals(textureName))
                    {
                        return ((Texture2D)objects[i]);
                    }
                }
            }

            return null;
        }

        public static Material FindMaterial(string materialName, AssetBundle assetBundle)
        {
            List<object> objects = new List<object>(assetBundle.LoadAllAssets(typeof(object)));


            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] is Material)
                {
                    if (((Material)objects[i]).name.Equals(materialName))
                    {
                        return ((Material)objects[i]);
                    }
                }
            }

            return null;
        }
        
        public static void ApplyEmissionShader(string materialName, string textureName,Transform tr, AssetBundle assetBundle, Color emissionColor)
        {
            //Use this to do the Emission
            var shader = Shader.Find("MarmosetUBER");
            var renderers = tr.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName))
                    {
                        material.shader = shader;

                        //material.EnableKeyword("_EMISSION");
                        material.EnableKeyword("MARMO_EMISSION");

                        material.SetVector("_EmissionColor", emissionColor * 1.0f);
                        material.SetTexture("_Illum",FindTexture2D(textureName, assetBundle));
                        material.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                    }
                }
            }
        }

        public static void ApplyNormalShader(string materialName, string textureName, Transform tr, AssetBundle assetBundle)
        {
            var shader = Shader.Find("MarmosetUBER");
            var renderers = tr.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    material.shader = shader;

                    if (material.name.StartsWith(materialName))
                    {
                        material.EnableKeyword("_NORMALMAP");

                        material.SetTexture("_BumpMap", FindTexture2D(textureName, assetBundle));

                        
                    }
                }
            }
        }

        public static void ApplyMetallicShader(string materialName, string textureName, Transform tr, AssetBundle assetBundle, Color color, float glossiness)
        {
            var shader = Shader.Find("MarmosetUBER");
            var renderers = tr.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    material.shader = shader;

                    if (material.name.StartsWith(materialName))
                    {
                        material.EnableKeyword("_METALLICGLOSSMAP");

                        material.SetColor("_Color", Color.white);
                        material.SetTexture("_Metallic", MaterialHelpers.FindTexture2D(textureName, assetBundle));
                        material.SetFloat("_Glossiness", glossiness);
                    }
                }
            }
        }

        public static void ApplySpecShader(string materialName, string textureName, Transform tr, Color specColor, float specInt, float shininess, AssetBundle assetBundle)
        {
            var shader = Shader.Find("MarmosetUBER");
            var renderers = tr.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    material.shader = shader;

                    if (material.name.StartsWith(materialName))
                    {
                        material.EnableKeyword("MARMO_SPECMAP");

                        material.SetColor("_SpecColor", specColor);
                        material.SetFloat("_SpecInt", specInt);
                        material.SetFloat("_Shininess", shininess);
                        material.SetTexture("_SpecTex", FindTexture2D(textureName, assetBundle));
                        material.SetVector("_SpecTex_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                    }
                }
            }
        }

        public static void ApplyAlphaShader(string materialName, Transform tr)
        {
            var shader = Shader.Find("MarmosetUBER");
            var renderers = tr.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    material.shader = shader;

                    if (material.name.StartsWith(materialName))
                    {
                        material.EnableKeyword("_ZWRITE_ON");
                        material.EnableKeyword("MARMO_ALPHA");
                        material.EnableKeyword("MARMO_ALPHA_CLIP");
                    }
                }
            }
        }

        public static Material ApplyParticalShader(string materialName, AssetBundle assetBundle)
        {
            var shader = Shader.Find("Particles/Additive");
            var mat = FindMaterial(materialName, assetBundle);
            mat.shader = shader;
            return mat;
        }

        public static void ApplyCustomShader(string materialName, string textureName, Transform tr, AssetBundle assetBundle, Shader shader, Color emissionColor)
        {
            if (shader == null) return;
            //Use this to do the Emission
            //var shader = Shader.Find("Custom/Dissolve");

            QuickLogger.Info($"Shader {shader.name}", true);

            var renderers = tr.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName))
                    {
                        material.shader = shader;

                        //material.EnableKeyword("_EMISSION");
                        //material.EnableKeyword("MARMO_EMISSION");

                        material.SetVector("_BurnColor", emissionColor * 1.0f);
                        material.SetTexture("_SliceGuide", FindTexture2D(textureName, assetBundle));
                        material.SetFloat("_BurnSize", 0.02f);
                        material.SetFloat("_EmissionAmount", 5.27f);
                        material.SetFloat("_SliceAmount", 0f);
                        material.SetColor("_BurnColor", Color.green);

                        //material.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                    }
                }
            }
        }
    }
}
