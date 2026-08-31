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
                "Create or select project-local XR UI settings and palette assets.",
                DeucarianControlCenterArea.Experience,
                OpenSettings,
                PackageId,
                searchTerms: new[] { "xr", "ui", "settings", "palette" },
                order: 320));
            DeucarianControlCenterRegistry.RegisterCardProvider(new Provider());
        }

        private static void OpenSettings()
        {
            CustomButtonReplacementEditor.CreateOrSelectGlobalSettings();
        }

        private sealed class Provider : IDeucarianControlCenterCardProvider
        {
            public string Id => PackageId + ".control-center";

            public IEnumerable<DeucarianControlCenterCard> Capture(
                DeucarianControlCenterContext context)
            {
                bool hasSettings = AssetDatabase.LoadAssetAtPath<CustomButtonSettings>(
                    CustomButtonReplacementEditor.SettingsPath) != null;
                bool hasPalette = AssetDatabase.LoadAssetAtPath<XrUiColorPalette>(
                    CustomButtonReplacementEditor.PalettePath) != null;
                int configuredCount = (hasSettings ? 1 : 0) + (hasPalette ? 1 : 0);
                yield return new DeucarianControlCenterCard(
                    PackageId + ".experience",
                    DeucarianControlCenterArea.Experience,
                    "XR UI",
                    "Manage the domain-owned global settings and color palette assets.",
                    PackageId,
                    configuredCount == 2
                        ? DeucarianControlCenterStatus.Success
                        : DeucarianControlCenterStatus.Info,
                    configuredCount + " of 2 project assets configured",
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
                            "Create or Select Palette",
                            () => CustomButtonReplacementEditor.CreateOrSelectGlobalColorPalette())
                    },
                    searchTerms: new[] { "xr", "controls", "palette", "settings" });
            }
        }
    }
}
