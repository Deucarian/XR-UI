using System;
using System.Collections.Generic;

namespace Deucarian.XRUI
{
    /// <summary>A explicitly owned palette selection scope. Registrations never own the palette assets.</summary>
    public sealed class XrUiPaletteContext
    {
        private readonly List<Registration> registrations = new List<Registration>();
        private readonly Func<XrUiColorPalette> fallback;

        public XrUiPaletteContext(XrUiColorPalette fallback = null) : this(() => fallback) { }
        internal XrUiPaletteContext(Func<XrUiColorPalette> fallback) => this.fallback = fallback;

        public event Action<XrUiColorPalette> Changed;

        public XrUiColorPalette Current
        {
            get
            {
                for (int index = registrations.Count - 1; index >= 0; index--)
                    if (registrations[index].Palette != null) return registrations[index].Palette;
                return fallback();
            }
        }

        /// <summary>Overrides this scope until disposed. Removing a newer registration reveals the previous owner.</summary>
        public IDisposable Register(XrUiColorPalette palette)
        {
            if (palette == null) throw new ArgumentNullException(nameof(palette));
            var previous = Current;
            var registration = new Registration(this, palette);
            registrations.Add(registration);
            if (previous != Current) Changed?.Invoke(Current);
            return registration;
        }

        private void Remove(Registration registration)
        {
            var previous = Current;
            if (registrations.Remove(registration) && previous != Current) Changed?.Invoke(Current);
        }

        private sealed class Registration : IDisposable
        {
            private XrUiPaletteContext owner;
            public readonly XrUiColorPalette Palette;
            public Registration(XrUiPaletteContext owner, XrUiColorPalette palette)
            {
                this.owner = owner;
                Palette = palette;
            }
            public void Dispose()
            {
                var context = owner;
                owner = null;
                context?.Remove(this);
            }
        }
    }
}
