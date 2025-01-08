using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalSanity
{
    public class TextManager : MonoBehaviour
    {
        private TextMeshProUGUI textBox;
        private TextMeshProUGUI sanityText;

        internal static TextManager instance;

        void Start()
        {
            // Create the TextMeshPro object
            GameObject textObject = new GameObject("TextBox");

            // Add TextMeshProUGUI component
            textBox = textObject.AddComponent<TextMeshProUGUI>();

            // Set the text and font
            textBox.fontSize = 10; // Optional, adjust size

            // Create a Canvas for the text object
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Create a CanvasScaler and GraphicRaycaster
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObject.transform);
            var panelRT = panel.AddComponent<RectTransform>();
            var ri = panel.AddComponent<RawImage>();
            ri.color = new(0.1f, 0.1f, 0.1f, 0.5f);
            panelRT.anchorMin = new Vector2(0, 0); // Bottom-left anchor
            panelRT.anchorMax = new Vector2(1, 0); // Bottom-right anchor
            panelRT.pivot = new Vector2(0.5f, 0); // Center the pivot horizontally
            panelRT.anchoredPosition = new Vector2(0, 10); // 10px from the bottom edge

            // Set width to 400px, and adjust height (you can change this based on your needs)
            panelRT.sizeDelta = new Vector2(400, 200); // Width 400px, height 50px (adjust as needed)

            panelRT.transform.localPosition = new(190, -635, 0);

            // Set the Canvas as the parent of the textObject
            textObject.transform.SetParent(canvasObject.transform);

            // Adjust RectTransform for bottom stretch with width of 400px
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0); // Bottom-left anchor
            rectTransform.anchorMax = new Vector2(1, 0); // Bottom-right anchor
            rectTransform.pivot = new Vector2(0.5f, 0); // Center the pivot horizontally
            rectTransform.anchoredPosition = new Vector2(0, 10); // 10px from the bottom edge

            // Set width to 400px, and adjust height (you can change this based on your needs)
            rectTransform.sizeDelta = new Vector2(400, 50); // Width 400px, height 50px (adjust as needed)

            rectTransform.transform.localPosition = new(210, -500, 0);

            var b = UnityEngine.Object.Instantiate(GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/WeightUI/"), GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/WeightUI/").transform.parent);
            sanityText = b.GetComponentInChildren<TextMeshProUGUI>();
            b.transform.localPosition -= new Vector3(0, 25, 0);
            sanityText.text = "0 s";

            instance = this;
        }

        public void UpdateSanity(float sanity, bool decreasing)
        {
            sanityText.text = ($"{sanity.ToString("0")} s " + (decreasing ? "<" : ">"));
        }

        // Method to update the text dynamically
        public void UpdateText(string newText)
        {
            if (textBox != null)
            {
                textBox.text += $"{newText}\n";
            }
        }

        public void Clear() => textBox.text = string.Empty;
    }
}
