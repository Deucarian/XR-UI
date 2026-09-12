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
            workspace.Subtitle.text = "Configure XR controls and connect their appearance to Theming.";
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
            var theming = new DeucarianEditorFeatureSection("xr-theming", "Visual styling",
                "Author palettes in Theming. The XR UI Theming Integration maps them to your controls.", DeucarianEditorIconIds.Palette);
            cards.Add(theming.Root);
            bool hasTheming = DeucarianToolRegistry.TryGet(DeucarianToolIds.ThemeManager, out _);
            theming.SetState(true);
            var openTheming = Ui.Button(hasTheming ? "Open Theming" : "Install Theming integration", () =>
                DeucarianEditorNavigation.Open(Page.Root, hasTheming ? "deucarian.theming.project-setup" : DeucarianToolIds.PackageInstaller));
            theming.Actions.Add(openTheming);
            theming.Details.Add(Ui.Label("Add a theme bridge and palette scope to the XR UI root. Existing authored palettes remain a fallback.", "dw-muted"));
            var preview = new VisualElement();
            preview.Add(Ui.Label("Fallback control preview", "dw-section-title"));
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
            if (palette != null)
            {
                var legacy = new Foldout { text = "Legacy palette fallback", value = false };
                legacy.AddToClassList("dw-foldout");
                scroll.Add(legacy);
                legacy.Add(Ui.Label("Used only without a theme bridge. Keep existing assets for compatibility; new colors belong in Theming.", "dw-muted"));
                AddSettings(legacy, "Authored fallback", palette);
            }
        }

        private void BuildAsset(VisualElement parent, string id, string title, string description,
            string icon, UnityEngine.Object asset, Func<UnityEngine.Object> create)
        {
            var card = new DeucarianEditorFeatureSection(id, title, description, icon);
            card.Root.AddToClassList("dw-feature-settings");
            parent.Add(card.Root);
            var resolved = asset != null ? asset : CustomButtonSettings.Global;
            var field = new DeucarianEditorAssetField(id + "-settings", resolved.GetType(), () => resolved,
                _ => Render(), create: asset == null ? create : null, allowSelection: false);
            card.Details.Add(Ui.Field("Settings asset", field.Root));
            card.Details.Add(Ui.Label(asset == null ? "Built-in runtime defaults · Create a project asset to customize shared controls."
                : "Shared controls load this project resource automatically.", "dw-muted"));
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
