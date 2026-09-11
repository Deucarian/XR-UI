using System;
using System.Collections.Generic;
using Deucarian.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Ui = Deucarian.Editor.DeucarianEditorWorkspaceControls;

namespace Deucarian.XRUI.Controls.Editor
{
    internal sealed class XrUiSettingsPage
    {
        private readonly DeucarianEditorWorkspace workspace;
        private readonly List<DeucarianEditorSerializedForm> bindings = new List<DeucarianEditorSerializedForm>();
        private bool showSettings;
        private CustomButtonSettings settings;
        private XrUiColorPalette palette;
        private DeucarianEditorControlSpecimen specimen;
        public IDeucarianEditorPage Page { get; }

        internal XrUiSettingsPage()
        {
            var root = new VisualElement();
            workspace = new DeucarianEditorWorkspace(root, Application.productName);
            workspace.Title.text = "XR UI";
            workspace.Subtitle.text = "Set up the shared controls for your XR app.";
            DeucarianEditorWorkspaceNavigation.Populate(workspace, "deucarian.xr-ui.settings");
            Page = new DeucarianEditorPage(root, activate: Activate, dispose: Dispose);
            Render();
        }

        private void Activate(string route)
        {
            bool openSettings = showSettings || route == "settings";
            if (openSettings != showSettings || settings != AssetDatabase.LoadAssetAtPath<CustomButtonSettings>(CustomButtonReplacementEditor.SettingsPath)
                || palette != AssetDatabase.LoadAssetAtPath<XrUiColorPalette>(CustomButtonReplacementEditor.PalettePath))
            {
                showSettings = openSettings; Render();
            }
            if (palette != null) specimen?.SetColors(palette.Background, palette.Primary, palette.BodyText);
        }

        private void Render()
        {
            foreach (var binding in bindings) binding.Dispose();
            bindings.Clear();
            settings = AssetDatabase.LoadAssetAtPath<CustomButtonSettings>(CustomButtonReplacementEditor.SettingsPath);
            palette = AssetDatabase.LoadAssetAtPath<XrUiColorPalette>(CustomButtonReplacementEditor.PalettePath);
            workspace.Content.Clear();
            var scroll = Ui.Scroll("xr-ui-settings");
            workspace.Content.Add(scroll);
            var cards = new VisualElement();
            BuildAsset(cards, "xr-controls", "UI settings", "Configure the shared XR UI behavior.",
                DeucarianEditorIconIds.Package, settings, () => CustomButtonReplacementEditor.CreateOrSelectGlobalSettings());
            BuildAsset(cards, "xr-palette", "Palette", "Choose the colors and styling for XR UI.",
                DeucarianEditorIconIds.Palette, palette, () => CustomButtonReplacementEditor.CreateOrSelectGlobalColorPalette());
            var preview = new VisualElement();
            specimen = new DeucarianEditorControlSpecimen();
            if (palette != null) specimen.SetColors(palette.Background, palette.Primary, palette.BodyText);
            preview.Add(specimen);
            var split = Ui.Split(cards, preview);
            split.AddToClassList("dw-spatial-split");
            split.AddToClassList("dw-asset-preview-split");
            scroll.Add(split);
            var actions = Ui.EndActions(Ui.Button(showSettings ? "Close settings" : "Open settings", () =>
            { showSettings = !showSettings; Render(); }, true));
            scroll.Add(actions);
            if (!showSettings) return;
            if (settings != null) AddSettings(scroll, "Control behavior", settings);
            if (palette != null) AddSettings(scroll, "Palette colors", palette);
        }

        private void BuildAsset(VisualElement parent, string id, string title, string description,
            string icon, UnityEngine.Object asset, Func<UnityEngine.Object> create)
        {
            var card = new DeucarianEditorFeatureSection(id, title, description, icon);
            card.Root.AddToClassList("dw-feature-settings");
            parent.Add(card.Root);
            if (asset == null)
            {
                card.Details.Add(Ui.Label("No project asset yet. Package defaults are in use.", "dw-muted"));
                card.Actions.Add(Ui.Button("Create project asset", () => { create(); Render(); }));
                card.SetState(true);
                return;
            }
            var field = new ObjectField { objectType = asset.GetType(), allowSceneObjects = false, value = asset };
            field.SetEnabled(false);
            var controls = Ui.Actions(field, Ui.Button("Select", () => { Selection.activeObject = asset; EditorGUIUtility.PingObject(asset); }));
            controls.AddToClassList("dw-asset-actions");
            card.Details.Add(Ui.Field(id == "xr-controls" ? "Settings asset" : "Palette asset", controls));
            card.SetState(true);
        }

        private void AddSettings(VisualElement parent, string title, UnityEngine.Object asset)
        {
            var panel = Ui.Panel(null, title);
            parent.Add(panel);
            var binding = new DeucarianEditorSerializedForm(panel, asset);
            bindings.Add(binding);
            binding.Remaining();
        }

        private void Dispose()
        {
            foreach (var binding in bindings) binding.Dispose();
            bindings.Clear();
            workspace.Dispose();
        }
    }
}
