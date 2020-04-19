using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Mono;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSDemo.Mono
{
    internal class DebugMenu : MonoBehaviour
    {
        private int _showDebugMenuNo = 4;
        private bool _isOpen = false;
        private static readonly Rect Size = new Rect(5, 5, 800, 600);
        private bool _showCursor = false;
        private static DebugMenu _main;
        private int _normalSkinSize;
        public float rSliderValue = 0.0F;
        public float gSliderValue = 0.0F;
        public float bSliderValue = 0.0F;
        private string _rTextBox = "0";
        private string _gTextBox = "0";
        private string _bTextBox = "0";
        private int _selGridInt = 0;
        private string[] _selStrings = new [] { " None" };
        private FCSDemoController[] _demoControllers = new FCSDemoController[0];
        private readonly LayerMask _clickMask = ~(1 << LayerMask.NameToLayer("layer19"));
        private readonly HashSet<GameObject> _openedObjects = new HashSet<GameObject>();
        private bool _wasObjectJustSelected;

        public void Open()
        {
            _isOpen = true;
            _showCursor = true;
        }

        public void Close()
        {
            _isOpen = false;
            _showCursor = false;
            UWE.Utils.alwaysLockCursor = true;
            UWE.Utils.lockCursor = true;
        }

        public void Toggle()
        {
            if (_isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Update()
        {
            if (_isOpen == false)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                _showCursor = !_showCursor;
            }

            if (_showCursor)
            {
                UWE.Utils.alwaysLockCursor = false;
                UWE.Utils.lockCursor = false;
            }
            else
            {
                UWE.Utils.alwaysLockCursor = true;
                UWE.Utils.lockCursor = true;
            }

            //var isRightClickDown =Input.GetMouseButtonDown(1);

            //if (isRightClickDown)
            //{
            //    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, _clickMask);

            //    if (isRightClickDown)
            //    {
            //        SelectAndShowObject(hit.collider.transform);
            //    }
            //}
        }

        public Transform SelectedTransform { get; set; }

        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), "FCSDemo");
        }

        private void OnGUI()
        {
            if (_isOpen == false)
            {
                return;
            }

            Rect windowRect = GUILayout.Window(2352, Size, (id) =>
            {
                GUILayout.Box("P to show/hide cursor");
                GUILayout.BeginVertical();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                _normalSkinSize = GUI.skin.label.fontSize;
                GUI.skin.label.fontSize = 15;
                GUILayout.Label("FCSDemo Controller");
                GUI.skin.label.fontSize = _normalSkinSize;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Color Changer Menu"))
                    _showDebugMenuNo = 0;

                GUILayout.EndHorizontal();

                GUILayout.Space(20f);

                if (_showDebugMenuNo == 0)
                    ColorChangerMenu();
                else
                    GUILayout.Label("No Menu Selected");

                GUILayout.EndVertical();

            }, "FCSDemo Controller");
        }

        private void GetInfo()
        {
            var reactorRod = CraftData.GetPrefabForTechType(TechType.Aquarium);
            Renderer[] renderers = reactorRod.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {

                    QuickLogger.Debug($"Materil Name: {material.name}");

                    //if (material.name.StartsWith("nuclear_reactor_rod_glass", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    material.name = $"{newMaterialName}_glass";
                    //    var glass = gameObject;
                    //    var render = glass.GetComponent<Renderer>();
                    //    render.material = material;
                    //    goto _end;
                    //}
                }
            }
            _end:;

            GameObject.Destroy(reactorRod);
        }

        #region Menus
        private void ColorChangerMenu()
        {

            GUILayout.BeginHorizontal();


            GUILayout.BeginVertical();
            GUILayout.Label("R");
            _rTextBox = GUILayout.TextField(_rTextBox, GUILayout.Width(100));
            rSliderValue = GUILayout.HorizontalSlider(rSliderValue, 0.0F, 1.0F);
            _rTextBox = rSliderValue.ToString();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("G");
            _gTextBox = GUILayout.TextField(_gTextBox, GUILayout.Width(100));
            gSliderValue = GUILayout.HorizontalSlider(gSliderValue, 0.0F, 1.0F);
            _gTextBox = gSliderValue.ToString();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("B");
            _bTextBox = GUILayout.TextField(_bTextBox, GUILayout.Width(100));
            bSliderValue = GUILayout.HorizontalSlider(bSliderValue, 0.0F, 1.0F);
            _bTextBox = bSliderValue.ToString();
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
            

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("Box");
            _selGridInt = GUILayout.SelectionGrid(_selGridInt, _selStrings, 1);
            GUILayout.EndVertical();


            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Get Demo Objects"))
            {
                GetFCSDemos();
            }
            if (GUILayout.Button("Change Color"))
            {
                ChangeSelectedColor();
            }
            GUILayout.EndHorizontal();
        }

        private void ChangeSelectedColor(FCSDemoController controller = null)
        {
            try
            {
                if (controller == null)
                {
                    var h = _demoControllers[_selGridInt];

                    if (h != null)
                    {
                        h.ChangeColor(new Vector3(rSliderValue, gSliderValue, bSliderValue));
                    }
                }
                else
                {
                    controller.ChangeColor(new Vector3(rSliderValue, gSliderValue, bSliderValue));
                }
            }
            catch (IndexOutOfRangeException)
            {
                QuickLogger.Error("No FCSDemo object Seletected",true);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Message:{e.Message}\nAtackTrace:{e.StackTrace}");
            }
        }

        public void SelectAndShowObject(Transform target)
        {
            SelectedTransform = target;

            if (target == null) return;
            
            var controller = target.GetComponentInChildren<FCSDemoController>();

            if(controller == null) return;

            ChangeSelectedColor(controller);
            QuickLogger.Info($"Model {controller.Name} Selected", true);

            //while (target != null)
            //{
            //    _openedObjects.Add(target.gameObject);
            //    target = target.parent;
            //}

            //_wasObjectJustSelected = true;
        }

        void GetFCSDemos()
        {
            _demoControllers = GameObject.FindObjectsOfType<FCSDemoController>();
            GetStrings();
        }

        private void GetStrings()
        {
            List<string> items = new List<string>();
            foreach (FCSDemoController controller in _demoControllers)
            {
                items.Add($"{controller.Name}_{controller.GetPrefabID()}");
                QuickLogger.Info( $"Added: {controller.Name}_{controller.GetPrefabID()}",true);
            }
            _selStrings = items.ToArray();
        }

        #endregion
        public static DebugMenu Main
        {
            get
            {
                if (_main == null)
                {
                    _main = Player.main.gameObject.AddComponent<DebugMenu>();
                }

                return _main;
            }
        }
    }
}
