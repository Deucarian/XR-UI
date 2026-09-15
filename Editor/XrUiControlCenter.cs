using System.Collections.Generic;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.XRUI.Controls.Editor
{
    [InitializeOnLoad]
    internal static class XrUiControlCenter
    {
        private const string PackageId = "com.deucarian.xr-ui";
        private const string ToolId = "deucarian.xr-ui.settings";

        static XrUiControlCenter()
        {
            DeucarianToolRegistry.Register(new DeucarianToolDescriptor(
                ToolId,
                "XR UI Settings",
                "Configure XR controls and connect their appearance to Theming.",
                DeucarianControlCenterArea.Experience,
                OpenSettings,
                PackageId,
                searchTerms: new[] { "xr", "ui", "settings", "palette" },
                order: 320, createPage: () => new XrUiSettingsPage().Page));
            DeucarianControlCenterRegistry.RegisterCardProvider(new Provider());
        }

        private static void OpenSettings()
        {
            DeucarianEditorToolWindow.Open(ToolId);
        }

        private sealed class Provider : IDeucarianControlCenterCardProvider
        {
            public string Id => PackageId + ".control-center";

            public IEnumerable<DeucarianControlCenterCard> Capture(
                DeucarianControlCenterContext context)
            {
                bool hasSettings = AssetDatabase.LoadAssetAtPath<CustomButtonSettings>(
                    CustomButtonReplacementEditor.SettingsPath) != null;
                bool hasTheming = DeucarianToolRegistry.TryGet("deucarian.theming.project-setup", out _);
                string paletteTool = hasTheming ? "deucarian.theming.project-setup" : DeucarianToolIds.PackageInstaller;
                yield return new DeucarianControlCenterCard(
                    PackageId + ".experience",
                    DeucarianControlCenterArea.Experience,
                    "XR UI",
                    "Configure control behavior. Visual palettes are owned by Theming.",
                    PackageId,
                    hasSettings
                        ? DeucarianControlCenterStatus.Success
                        : DeucarianControlCenterStatus.Info,
                    hasSettings ? "Project controls configured" : "Package control defaults",
                    order: 320,
                    details: new[]
                    {
                        "Only asset presence is summarized; asset contents remain private."
                    },
                    actions: new[]
                    {
                        new DeucarianControlCenterAction(
                            "settings",
                            "Create or Select Settings",
                            () => CustomButtonReplacementEditor.CreateOrSelectGlobalSettings()),
                        new DeucarianControlCenterAction(
                            "palette",
                            hasTheming ? "Theming setup" : "Get Theming",
                            () => DeucarianEditorToolWindow.Open(paletteTool),
                            navigationToolId: paletteTool)
                    },
                    searchTerms: new[] { "xr", "controls", "palette", "settings" });
            }
        }
    }
}
