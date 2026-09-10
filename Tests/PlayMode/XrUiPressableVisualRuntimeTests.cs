using System.Reflection;
using Deucarian.XRUI.Controls;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.PlayModeTests
{
    public sealed class XrUiPressableVisualRuntimeTests
    {
        [Test, Combinatorial]
        public void RuntimeClearsOverridesWithoutChangingTheExistingReportedChangeContract(
            [Values(0, 1, 2)] int layer, [Values(false, true)] bool overrideMatchesBaseSprite)
        {
            Assert.That(Application.isPlaying, Is.True);
            var target = new GameObject("Runtime managed visual", typeof(RectTransform), typeof(Image));
            var settings = ScriptableObject.CreateInstance<CustomButtonSettings>();
            var texture = new Texture2D(4, 4);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);
            var replacement = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.zero);
            try
            {
                var style = CustomPressableVisualStyle.GenericButton;
                var profile = settings.ResolveVisualStyle(style);
                foreach (string field in new[] { "_socketGhostSprite", "_pressableBackgroundSprite", "_outlineSprite" })
                    typeof(CustomPressableVisualStyleProfile).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(profile, sprite);
                Image image = target.GetComponent<Image>();
                bool Configure()
                {
                    switch (layer)
                    {
                        case 0: return CustomPressableSocketVisual.Configure(image, settings, style);
                        case 1: return CustomPressableSocketVisual.ConfigurePressableBackground(image, settings, style, null);
                        default: return CustomPressableSocketVisual.ConfigureOutline(image, settings, style);
                    }
                }

                Assert.That(Configure(), Is.True);
                Assert.That(Configure(), Is.False);
                image.overrideSprite = overrideMatchesBaseSprite ? sprite : replacement;
                Assert.That(Configure(), Is.False, "The existing runtime path does not report override-only changes.");
                Assert.That(image.sprite, Is.SameAs(sprite));
                Assert.That(image.overrideSprite, Is.SameAs(sprite));
                Assert.That(Configure(), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(settings);
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(replacement);
                Object.DestroyImmediate(texture);
            }
        }
    }
}
