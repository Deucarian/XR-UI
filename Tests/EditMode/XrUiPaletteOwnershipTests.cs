using NUnit.Framework;
using UnityEngine;

namespace Deucarian.XRUI.Tests
{
    public sealed class XrUiPaletteOwnershipTests
    {
        [Test]
        public void RemovingAnOlderRegistrationDoesNotClearTheCurrentOwner()
        {
            var first = ScriptableObject.CreateInstance<XrUiColorPalette>();
            var second = ScriptableObject.CreateInstance<XrUiColorPalette>();
            var context = new XrUiPaletteContext();
            try
            {
                using (var older = context.Register(first))
                using (var newer = context.Register(second))
                {
                    older.Dispose();
                    older.Dispose();
                    Assert.That(context.Current, Is.SameAs(second));
                }
                Assert.That(context.Current, Is.Null);
            }
            finally { Object.DestroyImmediate(first); Object.DestroyImmediate(second); }
        }

        [Test]
        public void RemovingTheCurrentOwnerRestoresPreviousScopeWithoutChangingOtherScopes()
        {
            var first = ScriptableObject.CreateInstance<XrUiColorPalette>();
            var second = ScriptableObject.CreateInstance<XrUiColorPalette>();
            var context = new XrUiPaletteContext(first);
            var independent = new XrUiPaletteContext(first);
            try
            {
                using (context.Register(second))
                {
                    Assert.That(context.Current, Is.SameAs(second));
                    Assert.That(independent.Current, Is.SameAs(first));
                }
                Assert.That(context.Current, Is.SameAs(first));
            }
            finally { Object.DestroyImmediate(first); Object.DestroyImmediate(second); }
        }
    }
}
