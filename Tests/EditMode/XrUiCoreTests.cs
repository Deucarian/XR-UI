using Deucarian.XRUI.Controls;
using NUnit.Framework;
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
    }
}
