using System.Linq;
using Deucarian.Editor;
using Deucarian.XRUI.Controls;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace Deucarian.XRUI.Tests
{
    public sealed class XrUiControlCenterTests
    {
        [Test]
        public void ReturningToUnchangedSettingsRetainsTheWorkingView()
        {
            Assert.IsTrue(DeucarianToolRegistry.TryGet("deucarian.xr-ui.settings", out var tool));
            using (var page = tool.CreatePage())
            {
                page.Activate("settings");
                var content = page.Root.Q("xr-ui-settings");
                page.Deactivate(); page.Activate(null); page.Activate("settings");
                Assert.AreSame(content, page.Root.Q("xr-ui-settings"));
            }
        }

        [Test]
        public void ContributionUsesCanonicalAssetsAndStableExperienceActions()
        {
            DeucarianControlCenterSnapshot snapshot =
                DeucarianControlCenterSnapshotBuilder.Capture();
            DeucarianToolDescriptor tool = snapshot.Tools.Single(candidate =>
                candidate.Id == "deucarian.xr-ui.settings");
            DeucarianControlCenterCard card = snapshot.Cards.Single(candidate =>
                candidate.Id == "com.deucarian.xr-ui.experience");
            bool complete =
                AssetDatabase.LoadAssetAtPath<CustomButtonSettings>(
                    "Assets/Deucarian/XR UI/Resources/CustomButtonSettings.asset") != null;

            Assert.That(tool.Area, Is.EqualTo(DeucarianControlCenterArea.Experience));
            Assert.That(card.Area, Is.EqualTo(DeucarianControlCenterArea.Experience));
            Assert.That(
                card.Status,
                Is.EqualTo(complete
                    ? DeucarianControlCenterStatus.Success
                    : DeucarianControlCenterStatus.Info));
            CollectionAssert.AreEqual(
                new[] { "settings", "palette" },
                card.Actions.Select(action => action.Id).ToArray());
            Assert.That(card.Actions.Single(action => action.Id == "palette").NavigationToolId,
                Is.EqualTo(DeucarianToolRegistry.TryGet("deucarian.theming.project-setup", out _)
                    ? "deucarian.theming.project-setup" : DeucarianToolIds.PackageInstaller));
        }
    }
}
