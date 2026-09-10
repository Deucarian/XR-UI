using System.Reflection;
using Deucarian.XRUI.Controls;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.Tests
{
    public sealed class XrUiPressableVisualTests
    {
        private GameObject target;
        private GameObject source;
        private CustomButtonSettings settings;
        private Texture2D texture;
        private Sprite sprite;
        private Sprite replacement;

        [SetUp]
        public void SetUp()
        {
            target = new GameObject("Managed visual", typeof(RectTransform), typeof(Image));
            source = new GameObject("Source visual", typeof(RectTransform), typeof(Image));
            settings = ScriptableObject.CreateInstance<CustomButtonSettings>();
            texture = new Texture2D(4, 4);
            sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);
            replacement = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.zero);
            source.GetComponent<Image>().sprite = sprite;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(settings);
            Object.DestroyImmediate(sprite);
            Object.DestroyImmediate(replacement);
            Object.DestroyImmediate(texture);
        }

        [Test, Combinatorial]
        public void EveryManagedLayerClearsStoredOverridesAndRemainsIdempotent(
            [Values(0, 1, 2)] int layer,
            [Values] CustomPressableVisualStyle style,
            [Values(false, true)] bool overrideMatchesBaseSprite)
        {
            var profile = settings.ResolveVisualStyle(style);
            foreach (string field in new[] { "_socketGhostSprite", "_pressableBackgroundSprite", "_outlineSprite" })
                typeof(CustomPressableVisualStyleProfile).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(profile, sprite);

            Image image = target.GetComponent<Image>();
            Assert.That(Configure(layer, image, style), Is.True);
            Assert.That(Configure(layer, image, style), Is.False);
            image.overrideSprite = overrideMatchesBaseSprite ? sprite : replacement;

            Assert.That(Configure(layer, image, style), Is.True, "Edit mode reports clearing a stored override.");
            using (var serialized = new SerializedObject(image))
                Assert.That(serialized.FindProperty("m_OverrideSprite").objectReferenceValue, Is.Null);
            Assert.That(image.sprite, Is.SameAs(sprite));
            Assert.That(image.overrideSprite, Is.SameAs(sprite), "Unity exposes the base sprite when no override remains.");
            Assert.That(Configure(layer, image, style), Is.False);
            Assert.That(profile.SocketGhostSprite, Is.SameAs(sprite));
            Assert.That(profile.PressableBackgroundSprite, Is.SameAs(sprite));
            Assert.That(profile.OutlineSprite, Is.SameAs(sprite));
        }

        [TestCase(0), TestCase(1), TestCase(2)]
        public void MissingImageRemainsANoOp(int layer)
        {
            Assert.That(Configure(layer, null, CustomPressableVisualStyle.GenericButton), Is.False);
        }

        private bool Configure(int layer, Image image, CustomPressableVisualStyle style)
        {
            switch (layer)
            {
                case 0: return CustomPressableSocketVisual.Configure(image, settings, style);
                case 1: return CustomPressableSocketVisual.ConfigurePressableBackground(image, settings, style, source.GetComponent<Image>());
                default: return CustomPressableSocketVisual.ConfigureOutline(image, settings, style);
            }
        }
    }
}
