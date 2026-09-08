using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.Tests
{
    public sealed class XrUiPaletteScopeTests
    {
        [Test]
        public void ScopedGraphicsIgnoreOtherPalettesAndRebindWhenReparented()
        {
            var first = new GameObject("First scope");
            var second = new GameObject("Second scope");
            var visual = new GameObject("Scoped text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var red = ScriptableObject.CreateInstance<XrUiColorPalette>();
            var blue = ScriptableObject.CreateInstance<XrUiColorPalette>();
            var green = ScriptableObject.CreateInstance<XrUiColorPalette>();
            try
            {
                red.BodyText = Color.red;
                blue.BodyText = Color.blue;
                green.BodyText = Color.green;
                var firstScope = first.AddComponent<XrUiPaletteScope>();
                var secondScope = second.AddComponent<XrUiPaletteScope>();
                firstScope.SetPalette(red);
                secondScope.SetPalette(blue);
                visual.transform.SetParent(first.transform);
                var image = visual.GetComponent<Image>();
                visual.AddComponent<PaletteGraphicColor>().Configure(image, XrUiSemanticColor.BodyText, false);
                Assert.That(image.color, Is.EqualTo(Color.red));
                using (XrUiColorPalette.RegisterRuntimePalette(green))
                {
                    Assert.That(image.color, Is.EqualTo(Color.red));
                    using (firstScope.Context.Register(blue))
                    {
                        Assert.That(image.color, Is.EqualTo(Color.blue));
                        secondScope.SetPalette(green);
                        Assert.That(image.color, Is.EqualTo(Color.blue));
                    }
                    Assert.That(image.color, Is.EqualTo(Color.red));
                    visual.transform.SetParent(second.transform);
                    Assert.That(XrUiPaletteScope.Resolve(image), Is.SameAs(green));
                    visual.GetComponent<PaletteGraphicColor>().ApplyPaletteColor();
                    Assert.That(image.color, Is.EqualTo(Color.green));
                    secondScope.SetPalette(blue);
                    Assert.That(image.color, Is.EqualTo(Color.blue));
                    secondScope.enabled = false;
                    Assert.That(image.color, Is.EqualTo(Color.green));
                }
            }
            finally
            {
                Object.DestroyImmediate(visual);
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
                Object.DestroyImmediate(red);
                Object.DestroyImmediate(blue);
                Object.DestroyImmediate(green);
            }
        }
    }
}
