using System.Linq;
using Deucarian.Editor;
using Deucarian.XRUI.Controls;
using NUnit.Framework;
using UnityEditor;

namespace Deucarian.XRUI.Tests
{
    public sealed class XrUiControlCenterTests
    {
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
                    "Assets/Deucarian/XR UI/Resources/CustomButtonSettings.asset") != null &&
                AssetDatabase.LoadAssetAtPath<XrUiColorPalette>(
                    "Assets/Deucarian/XR UI/Resources/XrUiColorPalette.asset") != null;

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
        }
    }
}