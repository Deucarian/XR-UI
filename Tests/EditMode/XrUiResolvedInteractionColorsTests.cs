using Deucarian.XRUI.Controls;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.XRUI.Tests
{
    public sealed class XrUiResolvedInteractionColorsTests
    {
        [Test]
        public void ResolvedStatesDoNotRewriteAuthoredPaletteOrMultiplyTheThemeAgain()
        {
            var palette = ScriptableObject.CreateInstance<XrUiColorPalette>();
            try
            {
                palette.Primary = new Color(.2f, .3f, .4f, 1);
                palette.SelectedMultiplier = 1.5f;
                string before = JsonUtility.ToJson(palette);
                palette.SetResolvedInteractionColors(Color.gray, Color.yellow, Color.green, Color.blue, Color.black);
                Assert.That(JsonUtility.ToJson(palette), Is.EqualTo(before));
                Assert.That(palette.GetInteractionColor(CustomButtonVisualState.Pressed), Is.EqualTo(Color.green));
                Assert.That(palette.GetInteractionColor(CustomButtonVisualState.Selected), Is.EqualTo(Color.blue));
                Assert.That(palette.GetAuthoredInteractionColor(CustomButtonVisualState.Selected),
                    Is.EqualTo(new Color(.3f, .45000002f, .6f, 1)));
            }
            finally { Object.DestroyImmediate(palette); }
        }
    }
}
