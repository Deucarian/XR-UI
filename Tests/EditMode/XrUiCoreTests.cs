using Deucarian.XRUI.Controls;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.Tests
{
    public sealed class XrUiCoreTests
    {
        [Test]
        public void GlobalPaletteFallsBackWithoutResourceAsset()
        {
            XrUiColorPalette palette = XrUiColorPalette.Global;

            Assert.IsNotNull(palette);
            Assert.AreEqual(XrUiColorPalette.DefaultBackground, palette.Background);
        }

        [Test]
        public void RuntimePaletteOverrideOwnsGlobalSelectionAndNotifiesOnlyOnChange()
        {
            XrUiColorPalette.ClearRuntimePalette();
            XrUiColorPalette baseline = XrUiColorPalette.Global;
            XrUiColorPalette runtimeOverride =
                ScriptableObject.CreateInstance<XrUiColorPalette>();
            int changeCount = 0;
            XrUiColorPalette firstChangedPalette = null;
            XrUiColorPalette secondChangedPalette = null;

            void RecordChange(XrUiColorPalette palette)
            {
                changeCount++;
                if (changeCount == 1)
                {
                    firstChangedPalette = palette;
                }
                else if (changeCount == 2)
                {
                    secondChangedPalette = palette;
                }
            }

            try
            {
                XrUiColorPalette.PaletteChanged += RecordChange;

                XrUiColorPalette.SetRuntimePalette(runtimeOverride);
                Assert.AreSame(runtimeOverride, XrUiColorPalette.Global);

                XrUiColorPalette.SetRuntimePalette(runtimeOverride);
                Assert.AreEqual(1, changeCount);

                XrUiColorPalette.ClearRuntimePalette();
                Assert.AreSame(baseline, XrUiColorPalette.Global);
                Assert.AreEqual(2, changeCount);
                Assert.AreSame(runtimeOverride, firstChangedPalette);
                Assert.AreSame(baseline, secondChangedPalette);
            }
            finally
            {
                XrUiColorPalette.PaletteChanged -= RecordChange;
                XrUiColorPalette.ClearRuntimePalette();
                Object.DestroyImmediate(runtimeOverride);
            }
        }

        [Test]
        public void GlobalSettingsFallsBackWithoutResourceAsset()
        {
            CustomButtonSettings settings = CustomButtonSettings.Global;

            Assert.IsNotNull(settings);
            Assert.That(settings.ActivationDepth, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void RuntimeServicesFallbackToCameraMainWhenNoProviderRegistered()
        {
            Assert.DoesNotThrow(() => XrUiRuntimeServices.ResolveEventCamera());
        }

        [Test]
        public void CustomButtonCreatesPressableSurfaceAndFeedback()
        {
            var gameObject = new GameObject("Button", typeof(RectTransform), typeof(Image));
            try
            {
                CustomButton button = gameObject.AddComponent<CustomButton>();

                Assert.IsNotNull(button);
                Assert.IsNotNull(gameObject.GetComponent<CustomPressableSurface>());
                Assert.IsNotNull(gameObject.GetComponent<CustomSelectableFeedback>());
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ExclusionRegistryCanExcludeSelectable()
        {
            var gameObject = new GameObject("Selectable", typeof(RectTransform), typeof(Button));
            Button button = gameObject.GetComponent<Button>();
            bool Predicate(Selectable selectable) => selectable == button;

            try
            {
                XrUiControlExclusionRegistry.RegisterSelectablePredicate(Predicate);

                Assert.IsTrue(XrUiControlExclusionRegistry.IsExcluded(button));
            }
            finally
            {
                XrUiControlExclusionRegistry.UnregisterSelectablePredicate(Predicate);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void InputFieldPressTargetConfiguresAcrossSupportedTmpVersions()
        {
            var gameObject = new GameObject("Input Field", typeof(RectTransform));
            try
            {
                TMP_InputField inputField = gameObject.AddComponent<TMP_InputField>();
                CustomPressableSurface surface = gameObject.AddComponent<CustomPressableSurface>();
                CustomInputFieldPressTarget target = gameObject.AddComponent<CustomInputFieldPressTarget>();

                Assert.DoesNotThrow(() => target.Configure(inputField, surface));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
