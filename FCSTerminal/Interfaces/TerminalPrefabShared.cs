using UnityEngine;
using UnityEngine.UI;

namespace FCSTerminal.Interfaces
{
    public static class TerminalPrefabShared
    {
        internal static Canvas CreateCanvas(Transform parent)
        {
            var canvas = new GameObject("Canvas", typeof(RectTransform)).AddComponent<Canvas>();
            var t = canvas.transform;
            t.SetParent(parent, false);
            canvas.sortingLayerID = 1;

            var raycaster = canvas.gameObject.AddComponent<uGUI_GraphicRaycaster>();

            var rt = t as RectTransform;
            RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            RectTransformExtensions.SetSize(rt, 1.7f, 3.0f);

            t.localPosition = new Vector3(0, 0, 0.345f);
            t.localRotation = new Quaternion(0, 1, 0, 0);
            t.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            canvas.scaleFactor = 0.01f;
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.referencePixelsPerUnit = 100;

            var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 20;

            return canvas;
        }

        internal static Image CreateBackground(Transform parent)
        {
            var background = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
            var rt = background.rectTransform;
            RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
            RectTransformExtensions.SetSize(rt, 114, 241);
            background.color = new Color(0, 0, 0);

            background.transform.localScale = new Vector3(0.01f, 0.01f, 1);
            background.type = Image.Type.Sliced;

            return background;
        }

        internal static Image CreateIcon(Transform parent, Color color, int y)
        {
            var icon = new GameObject("Text", typeof(RectTransform)).AddComponent<Image>();
            var rt = icon.rectTransform;
            RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
            RectTransformExtensions.SetSize(rt, 40, 40);

            rt.anchoredPosition = new Vector2(0, y);
            icon.color = color;

            return icon;
        }

        internal static Text CreateText(Transform parent, Text prefab, Color color, int y, int size, string initial)
        {
            var text = new GameObject("Text", typeof(RectTransform)).AddComponent<Text>();
            var rt = text.rectTransform;
            RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), parent);
            RectTransformExtensions.SetSize(rt, 120, 200);
            rt.anchoredPosition = new Vector2(0, y);

            text.font = prefab.font;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = initial;

            return text;
        }
    }
}
