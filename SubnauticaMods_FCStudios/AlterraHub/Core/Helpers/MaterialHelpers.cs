using FCSCommon.Utilities;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;
using UWE;
using System.Collections;

namespace FCS_AlterraHub.Core.Helpers;

public static class MaterialHelpers
{
    private static Material _glassMaterial;
    private static GameObject _laterialBubbles;

    /// <summary>
    /// Removes (Clone) from the object name string.
    /// </summary>
    /// <param name="oldName">Fulll object name</param>
    /// <returns>Object name without (Clone)</returns>
    private static string RemoveClone(string oldName)
    {
        return Regex.Replace(oldName, @"[(][a-zA-Z]*[)]", "");
    }

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

            QuickLogger.Info($"[FindTexture2D] Object Count: {objects.Count}");

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] is Texture2D)
                {
                    QuickLogger.Info($"[FindTexture2D] Object Name: {((Texture2D)objects[i]).name.ToLower()}");

                    if (((Texture2D)objects[i]).name.Equals(textureName, StringComparison.OrdinalIgnoreCase))
                    {
                        QuickLogger.Info($"Found Texture: {textureName}");
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
    /// Applies the properties for the MarmosetUBER shader that has a emission texture.
    /// </summary>
    /// <param name="materialName">The name of the material to look for on the object.</param>
    /// <param name="textureName">The name of the texture to look for in the assetBundle.</param>
    /// <param name="gameObject">The game object to process.</param>
    /// <param name="assetBundle">The assetBundle to search in.</param>
    /// <param name="emissionColor">The color to use on the emission material.</param>
    public static Material CreateV2EmissionMaterial(Material mat, string textureName, AssetBundle assetBundle, Shader shader, Color emissionColor, float emissionMuli = 1.0f)
    {
        if (mat != null)
        {
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
    public static Material CreateV2NormalMaterial(Material mat, string textureName, AssetBundle assetBundle, Shader shader)
    {
        if (mat != null)
        {
            mat.EnableKeyword("_NORMALMAP");

            mat.SetTexture("_BumpMap", FindTexture2D(textureName, assetBundle));
        }

        return mat;
    }

    /// <summary>
    ///  Applies the properties for the MarmosetUBER shader that has a specular texture.
    /// <param name="mat">The name of the material to look for on the object.</param>
    /// <param name="textureName">The name of the texture to look for in the assetBundle.</param>
    /// <param name="specInt"></param>
    /// <param name="shininess"></param>
    /// <param name="assetBundle">The assetBundle to search in.</param>
    /// <param name="shader">The MarmosetUBER shader</param>
    /// <returns></returns>
    public static Material CreateV2Specular(Material mat, string textureName, float specInt, float shininess, AssetBundle assetBundle, Shader shader)
    {
        if (mat != null)
        {
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
            //mat.SetVector("_SpecTex_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
        }

        return mat;
    }

    /// <summary>
    /// Applies the properties for the MarmosetUBER shader that has a ColorMask texture.
    /// </summary>
    /// <param name="mat">The name of the material to look for on the object.</param>
    /// <param name="maskTexture">The mane of the mask texture for the object.</param>
    /// <param name="color">Color value for the color property.</param>
    /// <param name="color2">Color value for the color2 property.</param>
    /// <param name="color3">Color value for the color3 property.</param>
    /// <param name="assetBundle">The assetBundle to search in.</param>
    /// <param name="shader">The MarmosetUBER shader</param>
    public static void CreateV2ColorMaskShader(Material mat, string maskTexture, Color color, Color color2, Color color3, AssetBundle assetBundle)
    {

        QuickLogger.Info("Setting Color Mask Shader", true);
               
       
        mat.EnableKeyword("UWE_3COLOR");
        Vector4 vec = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        Texture2D mask = FindTexture2D(maskTexture, assetBundle);
        if (mask != null)
        {
            mat.SetTexture("_MultiColorMask", mask);
            mat.SetVector("_Color2", color2);
            mat.SetVector("_SpecColor2", GetSpecColor(color2));
        }
        else
        {
            mat.SetVector("_Color2", vec);
            mat.SetVector("_SpecColor2", vec);
        }

        mat.SetVector("_Color",color);

        mat.SetVector("_SpecColor", GetSpecColor(color));

        mat.SetVector("_Color3", color3);
        mat.SetVector("_SpecColor3", GetSpecColor(color3));
    }


    private static readonly Color specularDark = new Color(0.39f, 0.39f, 0.39f, 1f);
    private static readonly Color specularLight = new Color(1f, 1f, 1f, 1f);

    private static Color GetSpecColor(Color color)
    {
        float luminance = GetLuminance(color);
        return Color.Lerp(specularDark, specularLight, luminance);
        
    }

    private static float GetLuminance(Color color)
    {
        return 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
    }

    public static void GetIngameObjects()
    {
        if (PrefabDatabase.TryGetPrefabFilename(CraftData.GetClassIdForTechType(TechType.Aquarium),
            out string filepath))
        {
            AddressablesUtility.LoadAsync<GameObject>(filepath).Completed += (prefab) =>
            {
                var aquarium = prefab.Result;

                QuickLogger.Info("In GetIngameObjects");

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
                                QuickLogger.Info($"Aquarium glass result: {_glassMaterial?.name}");
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

  
    public static Renderer _Renderer;
    public static IEnumerator GetGameBaseMaterial(Action callBack)
    {
        if (_Renderer != null)
        {
            callBack?.Invoke();
            yield return null;
        }

        string _name = "Assets/Prefabs/Base/GeneratorPieces/BaseLargeRoomExteriorTop.prefab";
        UWE.IPrefabRequest _IPrefabRequest = UWE.PrefabDatabase.GetPrefabForFilenameAsync(_name);
        yield return _IPrefabRequest;
        if (_IPrefabRequest.TryGetPrefab(out GameObject prefab))
        {
            Renderer[] Renderers = prefab.transform.GetComponentsInChildren<Renderer>();
            for (int j = 0; j < Renderers.Length; j++)
            {
                if (Renderers[j].gameObject.name == "LargeRoomExteriorTop_02")
                {
                    _Renderer = Renderers[j];
                    callBack?.Invoke();
                    yield break;
                }
            }
        }
    }


    /// <summary>
    /// Adds glass material to the gameobject.
    /// </summary>
    /// <param name="gameObject">The game object to add the glass material</param>
    public static void ApplyGlassShaderTemplate(GameObject gameObject, string matchName, string newMaterialName = "object", float fresnel = 0.7f, float shininess = 7f, float specInt = 7f)
    {
        GetIngameObjects();

        var models = GameObjectHelpers.FindGameObjects(gameObject, matchName, SearchOption.EndsWith);

        foreach (var model in models)
        {
            if (model != null)
            {
                var render = model.GetComponent<Renderer>();

                if (render == null) continue;

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

    /// <summary>
    /// Applies properties for the Alpha Material
    /// </summary>
    /// <param name="mat">Material to apply the to</param>
    /// <param name="assetBundle">The <see cref="AssetBundle"/> to search in.</param>
    /// <returns>Returns the modified <see cref="Material"/></returns>
    public static Material CreateV2ApplyAlphaMaterial(Material mat, AssetBundle assetBundle)
    {
        var shader = Shader.Find("MarmosetUBER");
        if (mat != null)
        {
            mat.shader = shader;
            mat.EnableKeyword("MARMO_ALPHA_CLIP");
            mat.SetFloat("_EnableCutOff", 1f);
            mat.SetFloat("_Cutoff", 0.5f);

        }

        return mat;
    }

    /// <summary>
    /// Changes the color on the material color property
    /// </summary>
    /// <param name="materialName">Name of the material to look for.</param>
    /// <param name="gameObject">The gameobject the material belongs too.</param>
    /// <param name="color">The color to use for the color proptery.</param>
    /// <param name="color2">The color to use for the color2 proptery.</param>
    /// <param name="color3">The color3 to use for the color proptery.</param>
    /// <returns>Returns a <see cref="bool"/> of true if the operation is successful</returns>
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
    
    /// <summary>
    /// Changes the emission color of the provided material by name. 
    /// </summary>
    /// <param name="materialName">Name of the material to change emission properties</param>
    /// <param name="gameObject">The <see cref="GameObject"/> to search for the provided material</param>
    /// <param name="color">The <see cref="Color"/> to change the emission _GlowColor property to.</param>
    /// <returns>Returns <see cref="bool"/> if operation was successful.</returns>
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

    /// <summary>
    /// Changes the emission color of the provided material by name. 
    /// </summary>
    /// <param name="material">Name of the material to change emission properties</param>
    /// <param name="color">The <see cref="Color"/> to change the emission _GlowColor property to.</param>
    public static void ChangeEmissionColor(Material material, Color color)
    {
        material.SetVector("_GlowColor", color);
    }

    /// <summary>
    /// Toggles the emission property on and off
    /// </summary>
    /// <param name="materialName">Name of the emission material</param>
    /// <param name="gameObject">The <see cref="GameObject"/> to search for the material.</param>
    /// <param name="value">True of to turn on the emission and false to turn off the emission</param>
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

    /// <summary>
    /// Applies a shader to the provided material.
    /// </summary>
    /// <param name="gameObject"><see cref="GameObject"/> to search for the material.</param>
    /// <param name="materialName">Name of the matertial to search for.</param>
    /// <param name="shaderName">NAme of the <see cref="Shader"/> to apply</param>
    public static void ApplyShaderToMaterial(GameObject gameObject, string materialName, string shaderName = "MarmosetUBER")
    {
        var shader = Shader.Find(shaderName);
        if (shader == null)
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

    /// <summary>
    /// Gets all the materials of the <see cref="GameObject"/>
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to search.</param>
    /// <param name="materialName">Name of the material to look for.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Changes the emission strength of the provided material
    /// </summary>
    /// <param name="materialName">Name of the material to search for</param>
    /// <param name="gameObject"><see cref="GameObject"/> to search for the material.</param>
    /// <param name="value">The value to set the strength property</param>
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

    /// <summary>
    /// Changed the tecture of the provided material by name.
    /// </summary>
    /// <param name="go"><see cref="GameObject"/> of the materal</param>
    /// <param name="materialName">Name of the material to look for.</param>
    /// <param name="filePath">Path to the texture to replace on the material</param>
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

    /// <summary>
    /// Turns a PNG to a <see cref="Texture2D"/>
    /// </summary>
    /// <param name="filePath">Path to the PNG file on the system</param>
    /// <returns></returns>
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

    /// <summary>
    /// Changes the main <see cref="Texture2D"/> to the provided <see cref="Texture2D"/>
    /// </summary>
    /// <param name="materialName">Name of the material to search for</param>
    /// <param name="go">The gameObject to search </param>
    /// <param name="texture">The <see cref="Texture2D"/> to apply the settings to.</param>
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

    /// <summary>
    /// Find the material on the provided gameobject
    /// </summary>
    /// <param name="go">The <see cref="GameObject"/> to search on.</param>
    /// <param name="materialName">The name of the material to look for.</param>
    /// <returns></returns>
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

    public static void ApplyParticlesUber(GameObject gameObject, string materialName, float srcBlend, float srcBlend2, float dstBlend, float dstBlend2, float mode, bool cutOff, Vector4 speed, Vector4 color)
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

                    material.SetFloat("_SrcBlend", srcBlend);
                    material.SetFloat("_SrcBlend2", srcBlend2);
                    material.SetFloat("_DstBlend", dstBlend);
                    material.SetFloat("_DstBlend2", dstBlend2);
                    material.SetFloat("_Mode", mode);
                    if (cutOff)
                        material.EnableKeyword("_Cutoff");
                    material.SetVector("_Speed", speed);
                    material.SetVector("_Color", color);
                }
            }
        }
    }

    public static void ChangeImage(GameObject gameObject, Texture2D texture2D, string materialName)
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
}
