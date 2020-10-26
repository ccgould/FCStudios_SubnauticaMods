using System;
using FCSCommon.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCSCommon.Helpers
{
    internal class MaterialHelpers
    {
        private static Material _glassMaterial;
        private static GameObject _laterialBubbles;


        /// <summary>
        /// Finds a <see cref="Texture2D"/> in the asset bundle with the specified name.
        /// </summary>
        /// <param name="textureName"></param>
        /// <param name="assetBundle"></param>
        /// <returns></returns>
        internal static Texture2D FindTexture2D(string textureName, AssetBundle assetBundle)
        {
            if (assetBundle != null)
            {
                var objects = new List<object>(assetBundle.LoadAllAssets(typeof(object)));

                //QuickLogger.Debug($"[FindTexture2D] Object Count: {objects.Count}");

                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i] is Texture2D)
                    {
                        //QuickLogger.Debug($"[FindTexture2D] Object Name: {((Texture2D)objects[i]).name.ToLower()}");

                        if (((Texture2D)objects[i]).name.Equals(textureName, StringComparison.OrdinalIgnoreCase))
                        {
                            //QuickLogger.Debug($"Found Texture: {textureName}");
                            return ((Texture2D)objects[i]);
                        }
                    }
                }
            }
            else
            {
                QuickLogger.Error($"Couldn't Find Bundle");
            }

            return null;
        }

        /// <summary>
        /// Finds a material in the assetBundle of the specified name.
        /// </summary>
        /// <param name="materialName">Name of the material to locate in the asset bundle.</param>
        /// <param name="assetBundle">The asset Bundle to search in.</param>
        /// <returns>Returns the <see cref="Material"/> of the specified type</returns>
        internal static Material FindMaterial(string materialName, AssetBundle assetBundle)
        {
            var objects = new List<object>(assetBundle.LoadAllAssets(typeof(object)));

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] is Material)
                {
                    if (((Material)objects[i]).name.Equals(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        return ((Material)objects[i]);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Applies the properties for the MarmosetUBER shader that has a emission texture.
        /// </summary>
        /// <param name="materialName">The name of the material to look for on the object.</param>
        /// <param name="textureName">The name of the texture to look for in the assetBundle.</param>
        /// <param name="gameObject">The game object to process.</param>
        /// <param name="assetBundle">The assetBundle to search in.</param>
        /// <param name="emissionColor">The color to use on the emission material.</param>
        internal static void ApplyEmissionShader(string materialName, string textureName, GameObject gameObject, AssetBundle assetBundle, Color emissionColor, float emissionMuli = 1.0f)
        {
            //Use this to do the Emission
            var shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.shader = shader;
                        material.EnableKeyword("MARMO_EMISSION");
                        material.SetTexture("_Illum", FindTexture2D(textureName, assetBundle));
                        material.SetFloat("_EnableGlow", 1);
                        material.SetColor("_GlowColor", emissionColor);
                    }
                }
            }
        }

        /// <summary>
        /// Applies the properties for the MarmosetUBER shader that has a metallic texture.
        /// </summary>
        /// <param name="materialName">The name of the material to look for on the object.</param>
        /// <param name="textureName">The name of the texture to look for in the assetBundle.</param>
        /// <param name="gameObject">The game object to process.</param>
        /// <param name="assetBundle">The assetBundle to search in.</param>
        internal static void ApplyNormalShader(string materialName, string textureName, GameObject gameObject, AssetBundle assetBundle)
        {
            var shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.shader = shader;

                        material.EnableKeyword("_NORMALMAP");

                        material.SetTexture("_BumpMap", FindTexture2D(textureName, assetBundle));
                    }
                }
            }
        }

        internal static bool ChangeMaterialColor(string materialName, GameObject gameObject, Color color, Color color2 = default, Color color3 = default)
        {
            var result = false;
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        //QuickLogger.Debug($"Found material {materialName} changing color to {color2}",true);
                        material.SetColor("_Color", color);
                        material.SetColor("_Color2", color2);
                        material.SetColor("_Color3", color3);
                        result = true;
                    }
                }
            }

            return result;
        }
        
        /// <summary>
        /// Changes the speed of the tree animation
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="value"></param>
        internal static void ChangeWavingSpeed(GameObject gameObject, Vector4 value)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    //Set the property _ObjectUp in the UWE_WAVING or the UWE/SIG AlphaCutOut + Noisey Wave
                    material.SetVector("_ObjectUp", value);
                }
            }
        }

        /// <summary>
        /// Applies the properties for the MarmosetUBER shader that has a metallic texture.
        /// </summary>
        /// <param name="materialName">The name of the material to look for on the object.</param>
        /// <param name="textureName">The name of the texture to look for in the assetBundle.</param>
        /// <param name="gameObject">The game object to process.</param>
        /// <param name="assetBundle">The assetBundle to search in.</param>
        /// <param name="glossiness">The amount of gloss to apply to the metallic material.</param>
        internal static void ApplyMetallicShader(string materialName, string textureName, GameObject gameObject, AssetBundle assetBundle, float glossiness)
        {
            var shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.shader = shader;
                        material.EnableKeyword("_METALLICGLOSSMAP");
                        material.SetColor("_Color", Color.white);
                        material.SetTexture("_Metallic", MaterialHelpers.FindTexture2D(textureName, assetBundle));
                        material.SetFloat("_Glossiness", glossiness);
                    }
                }
            }
        }
        /// <summary>
        /// Applies the properties for the MarmosetUBER shader that has a specular texture.
        /// </summary>
        /// <param name="materialName">The name of the material to look for on the object.</param>
        /// <param name="textureName">The name of the texture to look for in the assetBundle.</param>
        /// <param name="gameObject">The game object to process.</param>
        /// <param name="specInt">The amount of specular to apply in <see cref="float"/>.</param>
        /// <param name="shininess">The amount of shine to apply to the specular in <see cref="float"/>.</param>
        /// <param name="assetBundle">The assetBundle to search in.</param>
        internal static void ApplySpecShader(string materialName, string textureName, GameObject gameObject, float specInt, float shininess, AssetBundle assetBundle)
        {
            var shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.shader = shader;

                        material.EnableKeyword("MARMO_SPECMAP");

                        material.SetColor("_SpecColor", new Color(0.796875f, 0.796875f, 0.796875f, 0.796875f));
                        material.SetFloat("_SpecInt", specInt);
                        material.SetFloat("_Shininess", shininess);

                        var texture = FindTexture2D(textureName, assetBundle);
                        if (texture != null)
                        {
                            material.SetTexture("_SpecTex", texture);
                        }

                        material.SetFloat("_Fresnel", 0f);
                        material.SetVector("_SpecTex_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                    }
                }
            }
        }

        /// <summary>
        /// Adds glass material to the gameobject.
        /// </summary>
        /// <param name="gameObject">The game object to add the glass material</param>
        internal static void ApplyGlassShaderTemplate(GameObject gameObject,string matchName, string newMaterialName = "object")
        {
            GetIngameObjects();

            var models = GameObjectHelpers.FindGameObjects(gameObject, matchName, SearchOption.EndsWith);
            
            foreach (var model in models)
            {
                if (model != null)
                {
                    var render = model.GetComponent<Renderer>();

                    if(render == null) continue;

                    render.material = _glassMaterial;
                }
                else
                {
                    QuickLogger.Error($"[ApplyGlassShaderTemplate] Model was not found with the matching name {matchName}");
                }
            }
            
        }

        internal static void AddNewBubbles(GameObject gameObject, Vector3 position, Vector3 rotation)
        {
            GetIngameObjects();

            var newBubbles = GameObject.Instantiate(_laterialBubbles);
            newBubbles.transform.SetParent(gameObject.transform);
            newBubbles.transform.localPosition = position;
            newBubbles.transform.Rotate(rotation);
            newBubbles.SetActive(false);
        }

        internal static void GetIngameObjects()
        {
            var aquarium = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.Aquarium));

            try
            {
                QuickLogger.Debug("In GetIngameObjects");

                

                if (_glassMaterial == null)
                {

                    Renderer[] renderers = aquarium.GetComponentsInChildren<Renderer>(true);

                    foreach (Renderer renderer in renderers)
                    {
                        foreach (Material material in renderer.materials)
                        {
                            if (material.name.StartsWith("Aquarium_glass", StringComparison.OrdinalIgnoreCase))
                            {
                                _glassMaterial = material;
                                QuickLogger.Debug($"Aquarium glass result: {_glassMaterial?.name}");
                                goto _glassEnd;
                            }
                        }
                    }
                }
                _glassEnd: ;

                if (_laterialBubbles == null)
                {
                    var bubbles = aquarium.FindChild("Bubbles").FindChild("xBubbles").FindChild("xLateralBubbles");
                    if (bubbles == null)
                    {
                        QuickLogger.Error("Failed to find bubbles in the aquarium");
                        return;
                    }

                    _laterialBubbles = GameObject.Instantiate(bubbles);
                    QuickLogger.Debug($"Laterial Bubbles result: {_laterialBubbles?.name}");
                }
            }
            finally
            {
                GameObject.Destroy(aquarium);
            }

            

        }

        internal static void ApplyPrecursorShader(string materialName, string normalMap, string metalicmap, GameObject gameObject, AssetBundle assetBundle, float glossiness)
        {
            var shader = Shader.Find("UWE/Marmoset/IonCrystal");

            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.shader = shader;
                        material.EnableKeyword("_NORMALMAP");

                        //material.EnableKeyword("_METALLICGLOSSMAP");

                        material.SetTexture("_BumpMap", FindTexture2D(normalMap, assetBundle));

                        material.SetColor("_BorderColor", new Color(0.14f, 0.55f, 0.43f));

                        material.SetColor("_Color", new Color(0.33f, 0.83f, 0.17f));

                        material.SetColor("_DetailsColor", new Color(0.42f, 0.85f, 0.26f));

                        material.SetTexture("_MarmoSpecEnum", FindTexture2D(metalicmap, assetBundle));

                        material.SetFloat("_Glossiness", glossiness);

                    }
                }
            }
        }

        internal static void ApplyColorMaskShader(string materialName, string maskTexture, Color color, Color color2, Color color3, GameObject gameObject, AssetBundle assetBundle)
        {
            QuickLogger.Debug($"[ApplyColorMaskShader] In Apply Color Mask");

            var shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);

            QuickLogger.Debug($"[ApplyColorMaskShader] Renderer Count: {renderers.Length}");


            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    QuickLogger.Debug($"[ApplyColorMaskShader] Material Name: {material.name} || Compare to: {materialName} || Result: {material.name.ToLower().StartsWith(materialName.ToLower())} || Render:{renderer.name}");
                    

                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.shader = shader;
                        QuickLogger.Debug("Setting Color Mask Shader",true);
                        
                        material.EnableKeyword("UWE_3COLOR");
                        QuickLogger.Debug("Setting Color Mask UWE_3COLOR", true);

                        material.SetFloat("_Enable3Color", 1);
                        
                        material.SetTexture("_MultiColorMask", FindTexture2D(maskTexture, assetBundle));
                        QuickLogger.Debug("Setting Color Mask _MultiColorMask", true);

                        material.SetColor("_Color", color);
                        QuickLogger.Debug("Setting Color Mask _Color", true);

                        material.SetColor("_Color2", color2);
                        QuickLogger.Debug("Setting Color Mask _Color2", true);

                        material.SetColor("_Color3", color3);
                        QuickLogger.Debug("Setting Color Mask _Color3", true);

                    }
                }
            }
        }

        /// <summary>
        /// Applies the properties for the MarmosetUBER shader to make a material that has a transparency layer become transparent.
        /// </summary>
        /// <param name="materialName">The name of the material to look for on the object.</param>
        /// <param name="gameObject">The game object to process.</param>
        internal static void ApplyAlphaShader(string materialName, GameObject gameObject)
        {
            var shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.shader = shader;
                        material.EnableKeyword("MARMO_ALPHA_CLIP");
                    }
                }
            }
        }

        [Obsolete("This method will be removed in upcoming update please use MaterialHelpers.ChangeMaterialColor instead.")]
        /// <summary>
        /// Change the color of the body of the gameobject
        /// </summary>
        /// <param name="matNameColor">The material to change</param>
        /// <param name="color">The color to change to</param>
        /// <param name="gameObject">The game object to apply the change too.</param>
        internal static void ChangeBodyColor(string matNameColor, Color color, GameObject gameObject)
        {
            MaterialHelpers.ChangeMaterialColor(matNameColor, gameObject, color);
        }

        internal static void ReplaceEmissionTexture(string materialName, string replacementTexture, GameObject gameObject, AssetBundle assetBundle)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.SetTexture("_Illum", FindTexture2D(replacementTexture, assetBundle));
                    }
                }
            }
        }

        internal static void ChangeEmissionColor(string materialName, GameObject gameObject, Color color)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.SetVector("_GlowColor", color);
                    }
                }
            }
        }

        internal static void ToggleEmission(string materialName, GameObject gameObject, bool value)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (value)
                        {
                            material.EnableKeyword("MARMO_EMISSION");
                        }
                        else
                        {
                            material.DisableKeyword("MARMO_EMISSION");
                        }
                    }
                }
            }
        }

        internal static Color? GetBodyColor(GameObject gameObject, string materialName)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        return material.GetColor("_Color");
                    }
                }
            }

            return null;
        }

        public static Color? GetBodyMaskColor(GameObject gameObject, string materialName)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        return material.GetColor("_Color2");
                    }
                }
            }

            return null;
        }

        public static void ApplyShaderToMaterial(GameObject gameObject, string materialName,string shaderName = "MarmosetUBER")
        {
            var shader = Shader.Find(shaderName);
            if(shader == null)
            {
                QuickLogger.Error($"Cannot find a shader by the name : {shaderName}");
                return;
            }
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer render in renderers)
            {
                if (render.material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                {
                    render.material.shader = shader;
                }
            }
        }

        public static IEnumerable<Material> GetMaterials(GameObject gameObject, string materialName)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer render in renderers)
            {
                if (render.material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                {
                    yield return render.material;
                }
            }
        }

        public static void ChangeEmissionStrength(string materialName, GameObject gameObject, float value)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.SetFloat("_GlowStrength", value);
                        material.SetFloat("_GlowStrengthNight", value);
                    }
                }
            }
        }
    }
}
