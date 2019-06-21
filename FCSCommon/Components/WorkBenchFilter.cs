
using FCSCommon.Enums;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCSCommon.Components
{
    public class WorkBenchFilter : MonoBehaviour
    {
        private static readonly Font arialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        public Text InfoDisplay;

        public FilterTypes FilterType { get; set; }

        public void AddDisplayText(uGUI_ItemIcon icon)
        {
            // This code was made possible with the help of Waisie Milliams Hah

            QuickLogger.Debug("Added Display Text", true);

            var textGO = new GameObject("FilterLabel");

            textGO.transform.parent = icon.transform;
            textGO.AddComponent<Text>();

            Text text = textGO.GetComponent<Text>();
            text.font = arialFont;
            text.material = arialFont.material;
            text.text = string.Empty;
            text.fontSize = 16;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = Color.black;

            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            rectTransform.anchoredPosition3D = Vector3.zero;

            InfoDisplay = text;
        }
    }
}
