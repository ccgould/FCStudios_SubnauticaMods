using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FCSCommon.Utilities;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Helpers
{
    public class MaterialHelpers
    {
        private static Material _glassMaterial;
        private static GameObject _laterialBubbles;


        /// <summary>
        /// Finds a <see cref="Texture2D"/> in the asset bundle with the specified name.
        /// </summary>
        /// <param name="textureName"></param>
        /// <param name="assetBundle"></param>
        /// <returns></returns>
        public static Texture2D FindTexture2D(string textureName, AssetBundle assetBundle)
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
        public static Material FindMaterial(string materialName, AssetBundle assetBundle)
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
        public static void ApplyEmissionShader(string materialName, string textureName, GameObject gameObject, AssetBundle assetBundle, Color emissionColor, float emissionMuli = 1.0f)
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
                        //QuickLogger.Debug($"Adding Emission to: Found {material.name} || search {materialName}");

                        material.shader = shader;
                        material.EnableKeyword("MARMO_EMISSION");
                        material.SetTexture("_Illum", FindTexture2D(textureName, assetBundle));
                        material.SetFloat("_EnableGlow", 1);
                        material.SetFloat("_EnableLighting", 1);
                        material.SetColor("_GlowColor", emissionColor);
                    }
                }
            }
        }

        /// <summary>
        /// Applies the properties for the MarmosetUBER shader that has a emission texture.
        /// </summary>
        /// <param name="materialName">The name of the material to look for on the object.</param>
        /// <param name="gameObject">The game object to process.</param>
        /// <param name="assetBundle">The assetBundle to search in.</param>
        /// <param name="emissionColor">The color to use on the emission material.</param>
        public static void ApplyEmissionShader(string materialName, GameObject gameObject, Color emissionColor, float emissionLM = 1.0f, float glowStrenght = 1.0f, float glowStrenghtLM = 1.0f, float emissionMuli = 1.0f)
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
                        //QuickLogger.Debug($"Adding Emission to: Found {material.name} || search {materialName}");

                        material.shader = shader;
                        material.EnableKeyword("MARMO_EMISSION");
                        material.SetFloat("_EnableGlow", 1);
                        material.SetFloat("_EnableLighting", 1);
                        material.SetFloat("_GlowStrength", glowStrenght);
                        material.SetFloat("_GlowStrengthNight", glowStrenghtLM);
                        material.SetFloat("_EmissionLM", emissionLM);
                        material.SetColor("_GlowColor", emissionColor);
                    }
                }
            }
        }

        /// <summary>
        /// Applies the properties for the MarmosetUBER shader that has a emission texture.
        /// </summary>
        /// <param name="materialName">The name of the material to look for on the object.</param>
        /// <param name="textureName">The name of the texture to look for in the assetBundle.</param>
        /// <param name="gameObject">The game object to process.</param>
        /// <param name="assetBundle">The assetBundle to search in.</param>
        /// <param name="emissionColor">The color to use on the emission material.</param>
        public static Material CreateV2EmissionMaterial(Material mat, string textureName, AssetBundle assetBundle, Color emissionColor, float emissionMuli = 1.0f)
        {
            var shader = Shader.Find("MarmosetUBER");

            if (mat != null)
            {

                mat.shader = shader;
                mat.EnableKeyword("MARMO_EMISSION");
                mat.SetTexture("_Illum", FindTexture2D(textureName, assetBundle));
                mat.SetFloat("_EnableGlow", 1);
                mat.SetFloat("_EnableLighting", 1);
                mat.SetColor("_GlowColor", emissionColor);
            }
            return mat;
        }



        /// <summary>
        /// Applies the properties for the MarmosetUBER shader that has a normal texture.
        /// </summary>
        /// <param name="materialName">The name of the material to look for on the object.</param>
        /// <param name="textureName">The name of the texture to look for in the assetBundle.</param>
        /// <param name="gameObject">The game object to process.</param>
        /// <param name="assetBundle">The assetBundle to search in.</param>
        public static void ApplyNormalShader(string materialName, string textureName, GameObject gameObject, AssetBundle assetBundle)
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


        /// <summary>
        /// Applies the properties for the MarmosetUBER shader that has a normal texture.
        /// </summary>
        /// <param name="materialName">The name of the material to look for on the object.</param>
        /// <param name="textureName">The name of the texture to look for in the assetBundle.</param>
        /// <param name="gameObject">The game object to process.</param>
        /// <param name="assetBundle">The assetBundle to search in.</param>
        public static Material CreateV2NormalMaterial(Material mat, string textureName, AssetBundle assetBundle)
        {
            var shader = Shader.Find("MarmosetUBER");

            if (mat != null)
            {
                mat.shader = shader;

                mat.EnableKeyword("_NORMALMAP");

                mat.SetTexture("_BumpMap", FindTexture2D(textureName, assetBundle));
            }

            return mat;
        }


        public static bool ChangeMaterialColor(string materialName, GameObject gameObject, Color color, Color color2 = default, Color color3 = default)
        {
            var result = false;
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (RemoveClone(material.name).Trim().Equals(materialName, StringComparison.OrdinalIgnoreCase))
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
        
        private static string RemoveClone(string oldName)
        {
            return Regex.Replace(oldName, @"[(][a-zA-Z]*[)]", "");
        }
        
        /// <summary>
        /// Changes the speed of the tree animation
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="value"></param>
        public static void ChangeWavingSpeed(GameObject gameObject, Vector4 value)
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
        public static void ApplyMetallicShader(string materialName, string textureName, GameObject gameObject, AssetBundle assetBundle, float glossiness)
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
        public static void ApplySpecShader(string materialName, string textureName, GameObject gameObject, float specInt, float shininess, AssetBundle assetBundle)
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
        }        /// <summary>

                 /// <summary>
                 /// Applies the properties for the MarmosetUBER shader that has a specular texture.
                 /// </summary>
                 /// <param name="materialName">The name of the material to look for on the object.</param>
                 /// <param name="textureName">The name of the texture to look for in the assetBundle.</param>
                 /// <param name="gameObject">The game object to process.</param>
                 /// <param name="specInt">The amount of specular to apply in <see cref="float"/>.</param>
                 /// <param name="shininess">The amount of shine to apply to the specular in <see cref="float"/>.</param>
                 /// <param name="assetBundle">The assetBundle to search in.</param>
        public static void ApplySpecShader(string materialName, GameObject gameObject, float specInt, float shininess)
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
                        material.SetFloat("_Fresnel", 0f);
                        material.SetVector("_SpecTex_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                    }
                }
            }
        }


        public static Material CreateV2Specular(Material mat, string textureName, float specInt, float shininess, AssetBundle assetBundle)
        {
            var shader = Shader.Find("MarmosetUBER");
            if (mat != null)
            {
                mat.shader = shader;

                mat.EnableKeyword("MARMO_SPECMAP");
                  
                mat.SetColor("_SpecColor", new Color(0.796875f, 0.796875f, 0.796875f, 0.796875f));
                mat.SetFloat("_SpecInt", specInt);
                mat.SetFloat("_Shininess", shininess);

                var texture = FindTexture2D(textureName, assetBundle);
                if (texture != null)
                {
                    mat.SetTexture("_SpecTex", texture);
                }

                mat.SetFloat("_Fresnel", 0f);
                mat.SetVector("_SpecTex_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
            }

            return mat;
        }

        public static void ChangeSpecSettings(string materialName, string textureName, GameObject gameObject, float specInt, float shininess)
        {
            var shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.SetFloat("_SpecInt", specInt);
                        material.SetFloat("_Shininess", shininess);
                    }
                }
            }
        }


        /// <summary>
        /// Adds glass material to the gameobject.
        /// </summary>
        /// <param name="gameObject">The game object to add the glass material</param>
        public static void ApplyGlassShaderTemplate(GameObject gameObject,string matchName, string newMaterialName = "object",float fresnel= 0.7f,float shininess = 7f,float specInt = 7f)
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
                    var glassMat = render.material;
                    glassMat.SetFloat("_Fresnel", fresnel);
                    glassMat.SetFloat("_Shininess", shininess);
                    glassMat.SetFloat("_SpecInt", specInt);
                }
                else
                {
                    QuickLogger.Error($"[ApplyGlassShaderTemplate] Model was not found with the matching name {matchName}");
                }
            }
            
        }

        public static void AddNewBubbles(GameObject gameObject, Vector3 position, Vector3 rotation)
        {
            GetIngameObjects();

            var newBubbles = GameObject.Instantiate(_laterialBubbles);
            newBubbles.transform.SetParent(gameObject.transform);
            newBubbles.transform.localPosition = position;
            newBubbles.transform.Rotate(rotation);
            newBubbles.SetActive(false);
        }

#if SUBNAUTICA_STABLE
        public static void GetIngameObjects()
        {
            if (PrefabDatabase.TryGetPrefabFilename(CraftData.GetClassIdForTechType(TechType.Aquarium),
                out string filepath))
            {
                var prefab = Resources.Load<GameObject>(filepath);

                QuickLogger.Debug("In GetIngameObjects");

                if (_glassMaterial == null)
                {
                    Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

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

                _glassEnd:

                if (_laterialBubbles == null)
                {
                    var bubbles = prefab.FindChild("Bubbles").FindChild("xBubbles").FindChild("xLateralBubbles");
                    if (bubbles == null)
                    {
                        QuickLogger.Error("Failed to find bubbles in the aquarium");
                        return;
                    }

                    _laterialBubbles = GameObject.Instantiate(bubbles);
                    QuickLogger.Debug($"Laterial Bubbles result: {_laterialBubbles?.name}");
                }
            }
        }
#else
        public static void GetIngameObjects()
        {
            if (PrefabDatabase.TryGetPrefabFilename(CraftData.GetClassIdForTechType(TechType.Aquarium),
                out string filepath))
            {
                AddressablesUtility.LoadAsync<GameObject>(filepath).Completed += (prefab) =>
                {
                    var aquarium = prefab.Result;

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

                    _glassEnd:

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
                };
            }
        }
#endif



        public static void ApplyPrecursorShader(string materialName, string normalMap, string metalicmap, GameObject gameObject, AssetBundle assetBundle, float glossiness)
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

        public static void ApplyColorMaskShader(string materialName, string maskTexture, Color color, Color color2, Color color3, GameObject gameObject, AssetBundle assetBundle)
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
        public static void ApplyAlphaShader(string materialName, GameObject gameObject)
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
#if SUBNAUTICA
                        material.EnableKeyword("MARMO_ALPHA_CLIP");
#endif
                    }
                }
            }
        }


        public static Material CreateV2ApplyAlphaMaterial(Material mat, AssetBundle assetBundle)
        {
            var shader = Shader.Find("MarmosetUBER");
            if (mat != null)
            {
                mat.shader = shader;
#if SUBNAUTICA
                mat.EnableKeyword("MARMO_ALPHA_CLIP");
#endif
            }

                return mat;
        }

        [Obsolete("This method will be removed in upcoming update please use MaterialHelpers.ChangeMaterialColor instead.")]
        /// <summary>
        /// Change the color of the body of the gameobject
        /// </summary>
        /// <param name="matNameColor">The material to change</param>
        /// <param name="color">The color to change to</param>
        /// <param name="gameObject">The game object to apply the change too.</param>
        public static void ChangeBodyColor(string matNameColor, Color color, GameObject gameObject)
        {
            MaterialHelpers.ChangeMaterialColor(matNameColor, gameObject, color);
        }

        public static void ReplaceEmissionTexture(string materialName, string replacementTexture, GameObject gameObject, AssetBundle assetBundle)
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

        public static bool ChangeEmissionColor(string materialName, GameObject gameObject, Color color)
        {
            var result = false;
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (RemoveClone(material.name).Trim().Equals(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.SetVector("_GlowColor", color);
                        result = true;
                    }
                }
            }

            return result;
        }

        public static void ChangeEmissionColor(Material material, Color color)
        {
            material.SetVector("_GlowColor", color);
        }

        public static void ToggleEmission(string materialName, GameObject gameObject, bool value)
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

        public static Color? GetBodyColor(GameObject gameObject, string materialName)
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

        public static void SetTextureFromFile(GameObject go, string materialName, string filePath)
        {
            if (File.Exists(filePath))
            {
                var shader = Shader.Find("MarmosetUBER");
                byte[] fileData = File.ReadAllBytes(filePath);

                /* Working fine dont delete
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(fileData);

                Texture2D texPNG = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
                texPNG.SetPixels(tex.GetPixels());
                texPNG.Apply();
                */

                Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer render in renderers)
                {
                    foreach (Material material in render.materials)
                    {
                        if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                        {
                            material.mainTexture = LoadPNG(filePath);
                            material.shader = shader;
                            material.EnableKeyword("MARMO_ALPHA_CLIP");
                        }
                    }
                }
            }
        }

        public static Texture2D LoadPNG(string filePath)
        {

            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }

        public static void SetTexture(string materialName, GameObject go, Texture2D texture)
        {
            var shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer render in renderers)
            {
                foreach (Material material in render.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.mainTexture = texture;
                        material.shader = shader;
                        material.EnableKeyword("MARMO_ALPHA_CLIP");
                    }
                }
            }
        }

        public static Material GetMaterial(GameObject go, string materialName)
        {
            var m_Material = go.GetComponent<Renderer>();
            foreach (var material in m_Material.materials)
            {
                if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                {
                    return material;
                }
            }

            return null;
        }

        public static void ApplyParticlesUber(GameObject gameObject, string materialName,float srcBlend,float srcBlend2,float dstBlend,float dstBlend2, float mode, bool cutOff, Vector4 speed,Vector4 color)
        {
            var shader = Shader.Find("UWE/Particles/UBER");

            ParticleSystemRenderer[] renderers = gameObject.GetComponentsInChildren<ParticleSystemRenderer>(true);

            foreach (ParticleSystemRenderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.shader = shader;

                        material.SetFloat("_SrcBlend",srcBlend);
                        material.SetFloat("_SrcBlend2", srcBlend2);
                        material.SetFloat("_DstBlend",dstBlend);
                        material.SetFloat("_DstBlend2",dstBlend2);
                        material.SetFloat("_Mode",mode);
                        if(cutOff)
                            material.EnableKeyword("_Cutoff");
                        material.SetVector("_Speed", speed);
                        material.SetVector("_Color", color);
                    }
                }
            }
        }

        public static void ChangeImage(GameObject gameObject, Texture2D texture2D,string materialName)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.StartsWith(materialName, StringComparison.OrdinalIgnoreCase))
                    {
                        material.SetTexture("_MainTex", texture2D);
                    }
                }
            }
        }

        public static void CreateFloodLightCone(GameObject gameObject)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (RemoveClone(material.name).Trim().Equals("x_TechLight_Cone", StringComparison.OrdinalIgnoreCase))
                    {
                        material.shader = Shader.Find("UWE/Particles/UBER");
                        material.SetColor("_Color", new Color(0.37f, 0.4599999f, 0.5f, 1f));
                        material.SetFloat("_Mode", 3f);
                        material.SetFloat("_SrcBlend", 1f);
                        material.SetFloat("_DstBlend", 1f);
                        material.SetFloat("_SrcBlend2", 0f);
                        material.SetFloat("_DstBlend2", 10f);
                        material.SetFloat("_ZOffset", 0f);
                        material.SetFloat("_Cutoff", 0f);
                        material.SetVector("_ColorStrength", new Vector4(1f, 1f, 1f, 1f));
                        material.SetVector("_Scale", new Vector4(0f, 1f, 0f, 0f));
                        material.SetVector("_Frequency", new Vector4(0.5f, 0.5f, 0.5f, 0f));
                        material.SetVector("_Speed", new Vector4(0f, 0f, 0f, 0f));
                        material.SetFloat("_MyCullVariable", 0f);

                    }
                }
            }
        }
    }
}
