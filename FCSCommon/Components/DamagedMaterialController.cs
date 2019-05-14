using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using FCSCommon.Extensions;
using UnityEngine;

namespace FCSCommon.Components
{
    public class DamagedMaterialController: MonoBehaviour
    {
        private bool _found;
        private StringBuilder sb = new StringBuilder();
        private List<object> _assetBundleObjects;
        private List<Material> _assetBundleMaterials = new List<Material>();
        private List<Material> _defaultStateMaterials = new List<Material>();
        private List<Material> _damagedMaterials = new List<Material>();
        private Dictionary<string, Material> _defaultMaterialMatches = new Dictionary<string, Material>();
        private Dictionary<string, Material> _damagedMaterialMatches = new Dictionary<string, Material>();
        private Renderer[] _renderers;

        public bool ReplaceMaterials(AssetBundle bundle, Transform trans, out string log)
        {
            _found = false; //Flag to state wither the material was found thru the operation

            LogMessage($"In Replace Materials");


            /*
             * Note This methods is to apply a set of operations to apply damaged materials to the object
             * these changes will only happen if the material name is correct with the correct flags of with
             * _Damaged or _DefaultState
             */

            try
            {
                //Lets get all the renderer components in the gameObject
                _renderers = trans.GetComponentsInChildren<Renderer>();
                LogMessage($"Renderer Components Found: {_renderers.Length}");

                //Lets Get all the Materials
                _assetBundleObjects = new List<object>(bundle.LoadAllAssets(typeof(object)));
                LogMessage($"Asset Bundle Objects Found: {_assetBundleObjects.Count}");


                //Now that we have all the objects lets find all materials
                foreach (var assetBundleObject in _assetBundleObjects)
                {
                    if (assetBundleObject is Material)
                    {
                        var material = assetBundleObject as Material;
                        _assetBundleMaterials.Add(material);
                    }
                }
                LogMessage($"Found Materials Count: {_assetBundleMaterials?.Count}");

                //Lets make sure that the materials list is not null
                if (_assetBundleMaterials != null)
                {
                    //Lets get all the _DefaultStateMaterials
                    _defaultStateMaterials = _assetBundleMaterials.Where(x => x.name.EndsWith("_DefaultState")).ToList();
                    LogMessage($"Found Default State Materials Count: {_defaultStateMaterials.Count}");


                    //Lets get all the _DamagedMaterials
                    _damagedMaterials = _assetBundleMaterials.Where(x => x.name.EndsWith("_Damaged")).ToList();
                    LogMessage($"Found Damaged Materials Count: {_damagedMaterials.Count}");

                    //Lets find all matches
                    FindDefaultStateDamagedMaterials();
                    FindDamagedDefaultStateMaterials();

                    if (_defaultMaterialMatches != null)
                    {
                        foreach (var material in _defaultMaterialMatches)
                        {
                            LogMessage("// ========================== _defaultMaterialMatches Dic ====================//");
                            LogMessage($"Key Name {material.Key} || {material.Value.name}");
                            LogMessage("// ========================== _defaultMaterialMatches Dic ====================//");

                        }
                    }

                    if (_damagedMaterialMatches != null)
                    {
                        foreach (var material in _damagedMaterialMatches)
                        {
                            LogMessage("// ========================== _damagedMaterialMatches Dic ====================//");
                            LogMessage($"Key Name {material.Key} || {material.Value.name}");
                            LogMessage("// ========================== _damagedMaterialMatches Dic ====================//");

                        }
                    }

                    /*
                     * Ok so now that we have all the materials needed for this operation lets continue buy replacing the _DefaultState with
                     * _Damaged ones
                     */

                    ReplaceMaterials();

                }
            }
            catch (Exception e)
            {
                sb.Append(e.Message);
                log = sb.ToString();
            }

            log = sb.ToString();
            sb.Append(Environment.NewLine);
            return _found;
        }

        private void ReplaceMaterials()
        {
            LogMessage($"/// ===== In Replace Materials ===== ///");

            /*
             * For each of the renderers we would like to process the materials
             */

            foreach (Renderer renderer in _renderers)
            {
                var curRenderMaterials = renderer.materials;
                LogMessage($"Processing {curRenderMaterials}");
                ProcessRenerderMaterial(curRenderMaterials, renderer);
            }
        }

        private void ProcessRenerderMaterial(Material[] curRenderMaterials, Renderer renderer)
        {
            LogMessage($"Processing Renderer {renderer.name}");

            for (int i = 0; i < curRenderMaterials.Length; i++)
            {
                var materialName = curRenderMaterials[i].name;
                LogMessage($"Processing Renderer: Material = {materialName}");

                if (materialName.Contains("_DefaultState"))
                {
                    LogMessage($"Finding Default Match for {materialName}");
                    curRenderMaterials[i] = FindDefaultMatch(curRenderMaterials[i]);
                }


                if (materialName.Contains("_Damaged"))
                {
                    LogMessage($"Finding Damaged Match for {materialName}");
                    curRenderMaterials[i] = FindDamagedMatch(curRenderMaterials[i]);
                }

            }

            renderer.materials = curRenderMaterials;
        }

        private Material FindDefaultMatch(Material curRenderMaterial)
        {
           var isFound  = _defaultMaterialMatches.ContainsKey(curRenderMaterial.name.RemoveInstance());
            LogMessage($"Dictionary Search Result = {isFound} for Material {curRenderMaterial.name}");

            if (isFound)
            {
                var damaged = _defaultMaterialMatches[curRenderMaterial.name.RemoveInstance()];
                LogMessage($"Replaced {curRenderMaterial.name} => {damaged.name}");
                return damaged;
            }
            return curRenderMaterial;
        }

        private Material FindDamagedMatch(Material curRenderMaterial)
        {
            var isFound = _damagedMaterialMatches.ContainsKey(curRenderMaterial.name.RemoveInstance());
            LogMessage($"Dictionary Search Result = {isFound} for Material {curRenderMaterial.name}");

            if (isFound)
            {
                var defaultState = _damagedMaterialMatches[curRenderMaterial.name.RemoveInstance()];
                LogMessage($"Replaced {curRenderMaterial.name} => {defaultState.name}");
                return defaultState;
            }
            return curRenderMaterial;
        }

        private void FindDefaultStateDamagedMaterials()
        {
            LogMessage($"/// ===== In Find Default Matching Materials ===== ///");

            foreach (Material material in _defaultStateMaterials)
            {
                var name = material.name;
                var damaged = _damagedMaterials.FirstOrDefault(x => x.name.StartsWith(name.Remove(name.Length - 21)));

                if (damaged != null)
                {
                    LogMessage($"Match Found: {name} => {damaged.name}");
                    if (_defaultMaterialMatches.ContainsKey(material.name.RemoveInstance())) continue;
                    _defaultMaterialMatches.Add(material.name.RemoveInstance(),damaged);
                }
            }

            LogMessage($"/// ===== In Find Default Matching Materials ===== ///");
        }

        private void FindDamagedDefaultStateMaterials()
        {
            LogMessage($"/// ===== In Find Damaged Matching Materials ===== ///");

            foreach (Material material in _damagedMaterials)
            {
                var name = material.name;
                var defaultState = _defaultStateMaterials.FirstOrDefault(x => x.name.StartsWith(name.Remove(name.Length - 8)));

                if (defaultState != null)
                {
                    LogMessage($"Match Found: {name} => {defaultState.name}");

                    if(_damagedMaterialMatches.ContainsKey(material.name.RemoveInstance())) continue;
                    _damagedMaterialMatches.Add(material.name.RemoveInstance(), defaultState);
                }
            }

            LogMessage($"/// ===== In Find Damaged Matching Materials ===== ///");
        }

        private void LogMessage(string message)
        {
            sb.Append(message);
            sb.Append(Environment.NewLine);
        }
    }
}
