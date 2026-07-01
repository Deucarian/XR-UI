using Deucarian.XRUI;
using Deucarian.XRUI.Controls;
using Deucarian.XRUI.Dropdowns;
using UnityEngine;
using UnityEngine.UI;

public sealed class XrUiPressableControlsSample : MonoBehaviour
{
    [SerializeField] private Font font;
    private int _buttonPressCount;

    private void Start()
    {
        Canvas canvas = CreateCanvas();
        CreateButton(canvas.transform, "Button", new Vector2(0f, 90f));
        CreateToggle(canvas.transform, "Toggle", new Vector2(0f, 30f));
        CreateSlider(canvas.transform, new Vector2(0f, -30f));
        CreateDropdown(canvas.transform, new Vector2(0f, -90f));
    }

    private Canvas CreateCanvas()
    {
        var root = new GameObject("XR UI Sample Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        root.transform.localPosition = Vector3.forward * 1.2f;
        root.transform.localScale = Vector3.one * 0.002f;

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = XrUiRuntimeServices.ResolveEventCamera();
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(360f, 260f);
        return canvas;
    }

    private void CreateButton(Transform parent, string label, Vector2 position)
    {
        var go = CreateControlRoot(parent, label, position, new Vector2(240f, 44f));
        go.AddComponent<Image>();
        CustomButton button = go.AddComponent<CustomButton>();
        button.OnButtonClick.AddListener(() => _buttonPressCount++);
        AddLabel(go.transform, label);
    }

    private void CreateToggle(Transform parent, string label, Vector2 position)
    {
        var go = CreateControlRoot(parent, label, position, new Vector2(240f, 44f));
        go.AddComponent<Image>();
        Toggle toggle = go.AddComponent<Toggle>();
        go.AddComponent<CustomTogglePressTarget>();
        AddLabel(go.transform, label);
        toggle.targetGraphic = go.GetComponent<Image>();
    }

    private void CreateSlider(Transform parent, Vector2 position)
    {
        var go = CreateControlRoot(parent, "Slider", position, new Vector2(240f, 36f));
        Slider slider = go.AddComponent<CustomSlider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;
        go.AddComponent<Image>();
    }

    private void CreateDropdown(Transform parent, Vector2 position)
    {
        var go = CreateControlRoot(parent, "Dropdown", position, new Vector2(240f, 44f));
        go.AddComponent<Image>();
        Dropdown dropdown = go.AddComponent<CustomDropdown>();
        dropdown.options.Add(new Dropdown.OptionData("One"));
        dropdown.options.Add(new Dropdown.OptionData("Two"));
        AddLabel(go.transform, "Dropdown");
    }

    private GameObject CreateControlRoot(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)go.transform;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return go;
    }

    private void AddLabel(Transform parent, string text)
    {
        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)labelObject.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text label = labelObject.GetComponent<Text>();
        label.text = text;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = ColorPalette.BodyTextColor;
        label.font = font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.raycastTarget = false;
    }
}
