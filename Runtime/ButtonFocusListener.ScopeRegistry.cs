using System;
using System.Collections.Generic;

namespace Deucarian.XRUI
{
    internal static class ButtonFocusScopeRegistry
    {
        private static readonly Dictionary<Type, ButtonFocusListener> SelectedByType = new();
        private static readonly Dictionary<Type, HashSet<ButtonFocusListener>> ListenersByType = new();

        public static void EnsureScope(Type scopeType)
        {
            if (scopeType != null)
            {
                SelectedByType.TryAdd(scopeType, null);
            }
        }

        public static ButtonFocusListener GetSelected(Type scopeType)
        {
            return scopeType != null && SelectedByType.TryGetValue(scopeType, out ButtonFocusListener selected)
                           ? selected
                           : null;
        }

        public static void SetSelected(Type scopeType, ButtonFocusListener listener)
        {
            if (scopeType != null)
            {
                SelectedByType[scopeType] = listener;
            }
        }

        public static void ClearSelected(Type scopeType, ButtonFocusListener listener)
        {
            if (scopeType != null &&
                SelectedByType.TryGetValue(scopeType, out ButtonFocusListener selected) &&
                selected == listener)
            {
                SelectedByType[scopeType] = null;
            }
        }

        public static void Register(Type scopeType, ButtonFocusListener listener)
        {
            if (scopeType == null || listener == null)
            {
                return;
            }

            if (!ListenersByType.TryGetValue(scopeType, out HashSet<ButtonFocusListener> listeners))
            {
                listeners = new HashSet<ButtonFocusListener>();
                ListenersByType[scopeType] = listeners;
            }

            listeners.Add(listener);
        }

        public static void Unregister(Type scopeType, ButtonFocusListener listener)
        {
            if (scopeType == null || listener == null)
            {
                return;
            }

            if (!ListenersByType.TryGetValue(scopeType, out HashSet<ButtonFocusListener> listeners))
            {
                return;
            }

            listeners.Remove(listener);
            if (listeners.Count == 0)
            {
                ListenersByType.Remove(scopeType);
            }
        }

        public static void DeselectExcept(Type scopeType, ButtonFocusListener selected, bool invokeEvents)
        {
            if (scopeType == null ||
                !ListenersByType.TryGetValue(scopeType, out HashSet<ButtonFocusListener> listeners))
            {
                return;
            }

            ButtonFocusListener[] snapshot = new ButtonFocusListener[listeners.Count];
            listeners.CopyTo(snapshot);

            for (int i = 0; i < snapshot.Length; i++)
            {
                ButtonFocusListener listener = snapshot[i];
                if (listener == null || listener == selected)
                {
                    continue;
                }

                if (listener.IsSelected)
                {
                    listener.InternalDeselect(invokeEvents);
                }
            }
        }
    }
}
