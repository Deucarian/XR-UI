using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI.Dropdowns
{
    internal static class DropdownInteractionCoordinator
    {
        private const string DropdownListName = "Dropdown List";

        private static readonly Dictionary<Selectable, bool> BlockedSelectables = new();
        private static Component _activeOwner;

        public static bool IsBlocked(Selectable selectable)
        {
            return selectable != null && BlockedSelectables.ContainsKey(selectable);
        }

        public static void Begin(Component owner, Selectable ownerSelectable)
        {
            if (owner == null)
            {
                return;
            }

            if (_activeOwner != owner)
            {
                RestoreBlockedSelectables();
                _activeOwner = owner;
            }

            BlockUnityDropdowns(owner, ownerSelectable);
            BlockTmpDropdowns(owner, ownerSelectable);
        }

        public static void End(Component owner)
        {
            if (owner != null && _activeOwner != null && _activeOwner != owner)
            {
                return;
            }

            _activeOwner = null;
            RestoreBlockedSelectables();
        }

        public static bool HasOpenDropdownList(Component owner)
        {
            Canvas rootCanvas = GetRootCanvas(owner);
            if (rootCanvas == null)
            {
                return false;
            }

            Transform[] transforms = rootCanvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform transform in transforms)
            {
                if (transform != null &&
                    transform.name == DropdownListName &&
                    transform.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }

            return false;
        }

        public static void ForceRebuildLayout(RectTransform root)
        {
            if (root == null || !root.gameObject.activeInHierarchy)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();

            RectTransform[] rectTransforms = root.GetComponentsInChildren<RectTransform>(false);
            for (int i = rectTransforms.Length - 1; i >= 0; i--)
            {
                if (rectTransforms[i] != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransforms[i]);
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            Canvas.ForceUpdateCanvases();
        }

        private static void BlockUnityDropdowns(Component owner, Selectable ownerSelectable)
        {
            foreach (Dropdown dropdown in Object.FindObjectsByType<Dropdown>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (dropdown == null || IsOwnerSelectable(dropdown, owner, ownerSelectable))
                {
                    continue;
                }

                BlockSelectable(dropdown);
            }
        }

        private static void BlockTmpDropdowns(Component owner, Selectable ownerSelectable)
        {
            foreach (TMP_Dropdown dropdown in Object.FindObjectsByType<TMP_Dropdown>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (dropdown == null || IsOwnerSelectable(dropdown, owner, ownerSelectable))
                {
                    continue;
                }

                BlockSelectable(dropdown);
            }
        }

        private static bool IsOwnerSelectable(Selectable selectable, Component owner, Selectable ownerSelectable)
        {
            return selectable == ownerSelectable ||
                   selectable == owner ||
                   selectable != null &&
                   owner != null &&
                   selectable.gameObject == owner.gameObject;
        }

        private static void BlockSelectable(Selectable selectable, Selectable ownerSelectable = null)
        {
            if (selectable == null || selectable == ownerSelectable)
            {
                return;
            }

            if (!BlockedSelectables.ContainsKey(selectable))
            {
                BlockedSelectables.Add(selectable, selectable.interactable);
            }

            selectable.interactable = false;
        }

        private static void RestoreBlockedSelectables()
        {
            foreach (KeyValuePair<Selectable, bool> entry in BlockedSelectables)
            {
                if (entry.Key != null)
                {
                    entry.Key.interactable = entry.Value;
                }
            }

            BlockedSelectables.Clear();
        }

        private static Canvas GetRootCanvas(Component owner)
        {
            if (owner == null)
            {
                return null;
            }

            Canvas canvas = owner.GetComponentInParent<Canvas>();
            return canvas != null ? canvas.rootCanvas : null;
        }
    }
}
