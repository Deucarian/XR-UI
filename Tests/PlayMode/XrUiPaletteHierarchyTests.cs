using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Deucarian.XRUI.PlayModeTests
{
    public sealed class XrUiPaletteHierarchyTests
    {
        [UnityTest]
        public IEnumerator MovingAnAncestorRebindsDescendantGraphicsToTheirNewScope()
        {
            var first = new GameObject("First scope");
            var second = new GameObject("Second scope");
            var container = new GameObject("Movable container");
            var visual = new GameObject("Scoped graphic", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var red = ScriptableObject.CreateInstance<XrUiColorPalette>();
            var blue = ScriptableObject.CreateInstance<XrUiColorPalette>();
            try
            {
                red.BodyText = Color.red;
                blue.BodyText = Color.blue;
                first.AddComponent<XrUiPaletteScope>().SetPalette(red);
                second.AddComponent<XrUiPaletteScope>().SetPalette(blue);
                container.transform.SetParent(first.transform, false);
                visual.transform.SetParent(container.transform, false);
                var image = visual.GetComponent<Image>();
                visual.AddComponent<PaletteGraphicColor>().Configure(image, XrUiSemanticColor.BodyText, false);
                yield return null;
                Assert.That(image.color, Is.EqualTo(Color.red));

                container.transform.SetParent(second.transform, false);
                yield return null;
                yield return null;
                Assert.That(image.color, Is.EqualTo(Color.blue),
                    "Reparenting an ancestor does not invoke OnTransformParentChanged on the graphic itself.");

                container.transform.SetParent(first.transform, false);
                yield return null;
                yield return null;
                Assert.That(image.color, Is.EqualTo(Color.red));
            }
            finally
            {
                Object.DestroyImmediate(visual);
                Object.DestroyImmediate(container);
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
                Object.DestroyImmediate(red);
                Object.DestroyImmediate(blue);
            }
        }
    }
}
