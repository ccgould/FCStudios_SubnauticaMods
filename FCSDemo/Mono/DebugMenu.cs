using System;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Utilities;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
#endif
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

        #region Menus
        private void ColorChangerMenu()
        {

            GUILayout.BeginVertical("Box");
            _selGridInt = GUILayout.SelectionGrid(_selGridInt, _selStrings, 1);
            GUILayout.EndVertical();


            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Get Demo Objects"))
            {
                GetFCSDemos();
            }
            GUILayout.EndHorizontal();
        }

        

        public void SelectAndShowObject(Transform target)
        {
            SelectedTransform = target;

            if (target == null) return;
            
            var controller = target.GetComponentInChildren<FCSDemoController>();

            if(controller == null) return;

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
