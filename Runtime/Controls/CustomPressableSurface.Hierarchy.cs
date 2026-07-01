using System.Collections.Generic;
using Deucarian.Common;
using Deucarian.XRUI.Controls;
using Deucarian.XRUI.Scrollbars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Deucarian.XRUI.Controls
{
    internal sealed class CustomPressableSurfaceHierarchy
    {
        public void Ensure(CustomPressableSurface owner)
        {
            if (!owner._autoCreateHierarchy || owner._rectTransform == null)
            {
                return;
            }

            bool canModifyHierarchy = CanAutoModifyHierarchy(owner);
            bool createdVisualRoot = false;

            owner._pressableVisualRoot ??= owner.transform.Find(CustomPressableSurface.PressableVisualRootName) as RectTransform;
            if (owner._pressableVisualRoot == null)
            {
                if (!canModifyHierarchy)
                {
                    return;
                }

                owner._pressableVisualRoot = CreateStretchedChild(owner, CustomPressableSurface.PressableVisualRootName);
                SetRestDepth(owner, owner._pressableVisualRoot);
                createdVisualRoot = true;
            }

            if (canModifyHierarchy)
            {
                owner._pressableVisualRoot = EnsureSinglePressableVisualRoot(owner, owner._pressableVisualRoot);
            }

            if (canModifyHierarchy)
            {
                EnsureSocketAndMovingRootGraphic(owner);
            }
            else
            {
                Graphic socketGraphic = FindSocketGraphic(owner);
                if (socketGraphic != null)
                {
                    owner.SocketGraphic = socketGraphic;
                    ConfigureSocketGraphic(owner, socketGraphic);
                }
            }

            if (canModifyHierarchy && owner._moveExistingChildrenToVisualRoot && createdVisualRoot)
            {
                MoveExistingChildrenToVisualRoot(owner);
            }

            if (canModifyHierarchy)
            {
                NormalizePressableVisualRoot(owner, owner._pressableVisualRoot);
                SuppressMaskGraphics(owner);
                ConfigureGraphicRaycastTargets(owner);
            }
        }

        public bool IsOwnedByNestedSelectable(CustomPressableSurface owner, Transform candidate)
        {
            Transform current = candidate;
            while (current != null && current != owner.transform)
            {
                if (current.TryGetComponent(out Selectable selectable) && selectable != owner._selectable)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static RectTransform CreateStretchedChild(CustomPressableSurface owner, string childName)
        {
            var child = new GameObject(childName, typeof(RectTransform));
            child.layer = owner.gameObject.layer;

            var childTransform = (RectTransform)child.transform;
            childTransform.SetParent(owner.transform, false);
            childTransform.anchorMin = Vector2.zero;
            childTransform.anchorMax = Vector2.one;
            childTransform.offsetMin = Vector2.zero;
            childTransform.offsetMax = Vector2.zero;
            childTransform.pivot = new Vector2(0.5f, 0.5f);
            return childTransform;
        }

        private static void SetRestDepth(CustomPressableSurface owner, RectTransform rectTransform)
        {
            Vector3 localPosition = rectTransform.localPosition;
            localPosition.z = -owner.PressDepthDistance;
            rectTransform.localPosition = localPosition;
        }

        private static RectTransform EnsureSinglePressableVisualRoot(CustomPressableSurface owner, RectTransform keep)
        {
            if (owner == null)
            {
                return keep;
            }

            if (keep == null || keep.parent != owner.transform || !IsNamed(keep, CustomPressableSurface.PressableVisualRootName))
            {
                keep = owner.transform.Find(CustomPressableSurface.PressableVisualRootName) as RectTransform;
            }

            if (keep == null)
            {
                return null;
            }

            for (int i = owner.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = owner.transform.GetChild(i);
                if (child == null || child == keep)
                {
                    continue;
                }

                if (IsNamed(child, CustomPressableSurface.PressableVisualRootName))
                {
                    UnwrapAndDestroy(child as RectTransform, keep);
                    continue;
                }

                if (!owner.IsOwnedByNestedSelectable(child))
                {
                    RemoveNestedPressableVisualRoots(owner, child, keep);
                }
            }

            ConfigureStretchedVisualRoot(keep);
            SetRestDepth(owner, keep);
            keep.SetAsLastSibling();
            return keep;
        }

        private static void RemoveNestedPressableVisualRoots(CustomPressableSurface owner, Transform parent, RectTransform keep)
        {
            if (owner == null || parent == null || keep == null)
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child == null || child == keep)
                {
                    continue;
                }

                if (IsNamed(child, CustomPressableSurface.PressableVisualRootName))
                {
                    UnwrapAndDestroy(child as RectTransform, keep);
                    continue;
                }

                if (!owner.IsOwnedByNestedSelectable(child))
                {
                    RemoveNestedPressableVisualRoots(owner, child, keep);
                }
            }
        }

        private static void UnwrapAndDestroy(RectTransform container, RectTransform destination)
        {
            if (container == null || destination == null || container == destination)
            {
                return;
            }

            var children = new List<Transform>();
            for (int i = 0; i < container.childCount; i++)
            {
                children.Add(container.GetChild(i));
            }

            for (int i = 0; i < children.Count; i++)
            {
                Transform child = children[i];
                if (child == null || child == destination)
                {
                    continue;
                }

                child.SetParent(destination, false);
                if (child is RectTransform childRect)
                {
                    Vector3 localPosition = childRect.localPosition;
                    localPosition.z = 0f;
                    childRect.localPosition = localPosition;
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(container.gameObject) &&
                    PrefabUtility.IsAddedGameObjectOverride(container.gameObject))
                {
                    PrefabUtility.RevertAddedGameObject(container.gameObject, InteractionMode.AutomatedAction);
                    return;
                }

                UnityObjectUtility.DestroySafely(container.gameObject);
                return;
            }
#endif

            UnityObjectUtility.DestroySafely(container.gameObject);
        }

        private void EnsureSocketAndMovingRootGraphic(CustomPressableSurface owner)
        {
            bool preserveSelectableGeometry = ShouldPreserveSelectableGeometry(owner);
            Graphic rootGraphic = GetOrCreateRootHitGraphic(owner);
            Image movingImage = preserveSelectableGeometry
                                        ? null
                                        : EnsureMovingBackground(owner, rootGraphic as Image);
            if (!preserveSelectableGeometry)
            {
                EnsureOutline(owner);
            }

            Graphic socketGraphic = FindSocketGraphic(owner);
            if (socketGraphic == null)
            {
                socketGraphic = CreateSocketGhost(owner);
            }

            if (socketGraphic == null)
            {
                return;
            }

            ConfigureSocketGraphic(owner, socketGraphic);
            RemoveExtraSocketGhosts(owner, socketGraphic.transform as RectTransform);
            owner.SocketGraphic = socketGraphic;
            if (preserveSelectableGeometry)
            {
                RestorePreservedSelectableTargetGraphic(owner);
            }
            else if (owner._selectable != null && movingImage != null && owner._selectable.targetGraphic != movingImage)
            {
                owner._selectable.targetGraphic = movingImage;
            }

            RedirectSelectableTargetAwayFromSocket(owner);
            ConfigureRootGraphicAsHitTarget(owner, rootGraphic, socketGraphic);
        }

        private static Graphic GetOrCreateRootHitGraphic(CustomPressableSurface owner)
        {
            Graphic rootGraphic = owner.GetComponent<Graphic>();
            if (rootGraphic != null)
            {
                return rootGraphic;
            }

            Image rootImage = owner.gameObject.AddComponent<Image>();
            rootImage.color = ColorPalette.TransparentColor;
            rootImage.raycastTarget = true;
            return rootImage;
        }

        private Image EnsureMovingBackground(CustomPressableSurface owner, Image rootImage)
        {
            Transform existing = FindPreferredDirectManagedChild(owner._pressableVisualRoot, CustomPressableSurface.MovingRootGraphicName);
            Image movingImage = existing != null ? existing.GetComponent<Image>() : null;
            if (movingImage == null)
            {
                RectTransform movingRect = CreateStretchedChild(owner, CustomPressableSurface.MovingRootGraphicName);
                movingRect.SetParent(owner._pressableVisualRoot, false);
                movingImage = movingRect.gameObject.AddComponent<Image>();
            }

            if (movingImage.transform is RectTransform movingTransform)
            {
                ConfigureIgnoredStretchedLayout(movingTransform);
                movingTransform.SetAsFirstSibling();
            }

            CustomPressableSocketVisual.ConfigurePressableBackground(movingImage,
                                                                     owner.ResolvedSettings,
                                                                     owner.ResolvedVisualStyle,
                                                                     rootImage);
            NormalizePressableBackgrounds(owner, owner._pressableVisualRoot);
            return movingImage;
        }

        private Image EnsureOutline(CustomPressableSurface owner)
        {
            if (owner == null || owner._pressableVisualRoot == null)
            {
                return null;
            }

            Transform existing = FindPreferredDirectManagedChild(owner._pressableVisualRoot, CustomPressableSurface.OutlineGraphicName);
            Image outlineImage = existing != null ? existing.GetComponent<Image>() : null;
            if (outlineImage == null)
            {
                RectTransform outlineRect = existing as RectTransform;
                if (outlineRect == null)
                {
                    outlineRect = CreateStretchedChild(owner, CustomPressableSurface.OutlineGraphicName);
                    outlineRect.SetParent(owner._pressableVisualRoot, false);
                }

                outlineImage = outlineRect.gameObject.AddComponent<Image>();
            }

            if (outlineImage.transform is RectTransform outlineTransform)
            {
                ConfigureIgnoredStretchedLayout(outlineTransform);
            }

            CustomPressableSocketVisual.ConfigureOutline(outlineImage, owner.ResolvedSettings, owner.ResolvedVisualStyle);
            NormalizeOutlines(owner, owner._pressableVisualRoot, outlineImage.transform as RectTransform);
            return outlineImage;
        }

        private static void NormalizePressableBackgrounds(CustomPressableSurface owner, RectTransform visualRoot)
        {
            if (visualRoot == null)
            {
                return;
            }

            var backgrounds = new List<RectTransform>();
            for (int i = 0; i < visualRoot.childCount; i++)
            {
                Transform child = visualRoot.GetChild(i);
                if (IsNamed(child, CustomPressableSurface.MovingRootGraphicName) && child is RectTransform rectTransform)
                {
                    backgrounds.Add(rectTransform);
                }
            }

            RectTransform keep = SelectManagedLayerKeeper(backgrounds, null);
            if (keep != null)
            {
                HoistChildrenOutOfBackground(keep, visualRoot);
            }

            for (int i = backgrounds.Count - 1; i >= 0; i--)
            {
                RectTransform duplicate = backgrounds[i];
                if (duplicate == null || duplicate == keep)
                {
                    continue;
                }

                UnwrapAndDestroy(duplicate, visualRoot);
            }

            RemoveNestedPressableBackgrounds(owner, visualRoot, keep, visualRoot);
            RemoveLegacyBackgroundWrappers(owner, visualRoot);
        }

        private static void NormalizeOutlines(CustomPressableSurface owner, RectTransform visualRoot, RectTransform preferred)
        {
            if (owner == null || visualRoot == null)
            {
                return;
            }

            var outlines = new List<RectTransform>();
            for (int i = 0; i < visualRoot.childCount; i++)
            {
                Transform child = visualRoot.GetChild(i);
                if (IsNamed(child, CustomPressableSurface.OutlineGraphicName) && child is RectTransform rectTransform)
                {
                    outlines.Add(rectTransform);
                }
            }

            RectTransform keep = SelectManagedLayerKeeper(outlines, preferred);
            for (int i = outlines.Count - 1; i >= 0; i--)
            {
                RectTransform duplicate = outlines[i];
                if (duplicate == null || duplicate == keep)
                {
                    continue;
                }

                UnwrapAndDestroy(duplicate, visualRoot);
            }

            RemoveNestedOutlines(owner, visualRoot, keep, visualRoot);
        }

        private static RectTransform FindPreferredDirectManagedChild(RectTransform parent, string childName)
        {
            var children = new List<RectTransform>();
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (IsNamed(child, childName) && child is RectTransform rectTransform)
                {
                    children.Add(rectTransform);
                }
            }

            return SelectManagedLayerKeeper(children, null);
        }

        private static RectTransform SelectManagedLayerKeeper(List<RectTransform> candidates, RectTransform preferred)
        {
            RectTransform validPreferred = candidates != null &&
                                           preferred != null &&
                                           candidates.Contains(preferred)
                                                   ? preferred
                                                   : null;

            RectTransform inheritedWithImage = null;
            RectTransform inheritedAny = null;
            RectTransform addedWithImage = null;
            RectTransform addedAny = null;

            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    RectTransform candidate = candidates[i];
                    if (candidate == null)
                    {
                        continue;
                    }

                    bool hasImage = candidate.GetComponent<Image>() != null;
                    if (IsAddedPrefabOverride(candidate))
                    {
                        if (hasImage)
                        {
                            addedWithImage ??= candidate;
                        }

                        addedAny ??= candidate;
                        continue;
                    }

                    if (hasImage)
                    {
                        inheritedWithImage ??= candidate;
                    }

                    inheritedAny ??= candidate;
                }
            }

            return inheritedWithImage ??
                   inheritedAny ??
                   validPreferred ??
                   addedWithImage ??
                   addedAny;
        }

        private static bool IsAddedPrefabOverride(RectTransform rectTransform)
        {
#if UNITY_EDITOR
            return rectTransform != null &&
                   PrefabUtility.IsPartOfPrefabInstance(rectTransform.gameObject) &&
                   PrefabUtility.IsAddedGameObjectOverride(rectTransform.gameObject);
#else
            return false;
#endif
        }

        private static void HoistChildrenOutOfBackground(RectTransform background, RectTransform visualRoot)
        {
            if (background == null || visualRoot == null || background.childCount == 0)
            {
                return;
            }

            var children = new List<Transform>();
            for (int i = 0; i < background.childCount; i++)
            {
                children.Add(background.GetChild(i));
            }

            int insertIndex = Mathf.Min(background.GetSiblingIndex() + 1, visualRoot.childCount);
            for (int i = 0; i < children.Count; i++)
            {
                Transform child = children[i];
                if (child == null)
                {
                    continue;
                }

                if (IsNamed(child, CustomPressableSurface.PressableVisualRootName) && child is RectTransform nestedRoot)
                {
                    UnwrapAndDestroy(nestedRoot, visualRoot);
                    continue;
                }

                child.SetParent(visualRoot, false);
                child.SetSiblingIndex(Mathf.Min(insertIndex, visualRoot.childCount - 1));
                insertIndex++;
                if (child is RectTransform childRect)
                {
                    Vector3 localPosition = childRect.localPosition;
                    localPosition.z = 0f;
                    childRect.localPosition = localPosition;
                }
            }
        }

        private static void RemoveLegacyBackgroundWrappers(CustomPressableSurface owner, RectTransform visualRoot)
        {
            if (owner == null || visualRoot == null)
            {
                return;
            }

            for (int i = visualRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = visualRoot.GetChild(i);
                if (child == null ||
                    IsNamed(child, CustomPressableSurface.MovingRootGraphicName) ||
                    !IsNamed(child, "Background") ||
                    owner.IsOwnedByNestedSelectable(child))
                {
                    continue;
                }

                UnwrapAndDestroy(child as RectTransform, visualRoot);
            }
        }

        private static void RemoveNestedPressableBackgrounds(CustomPressableSurface owner,
                                                            Transform parent,
                                                            RectTransform keep,
                                                            RectTransform visualRoot)
        {
            if (parent == null || visualRoot == null)
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child == null || child == keep)
                {
                    continue;
                }

                if (IsNamed(child, CustomPressableSurface.MovingRootGraphicName))
                {
                    UnwrapAndDestroy(child as RectTransform, visualRoot);
                    continue;
                }

                if (owner != null && owner.IsOwnedByNestedSelectable(child))
                {
                    continue;
                }

                RemoveNestedPressableBackgrounds(owner, child, keep, visualRoot);
            }
        }

        private static void RemoveNestedOutlines(CustomPressableSurface owner,
                                                 Transform parent,
                                                 RectTransform keep,
                                                 RectTransform visualRoot)
        {
            if (parent == null || visualRoot == null)
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child == null || child == keep)
                {
                    continue;
                }

                if (IsNamed(child, CustomPressableSurface.OutlineGraphicName))
                {
                    UnwrapAndDestroy(child as RectTransform, visualRoot);
                    continue;
                }

                if (owner != null && owner.IsOwnedByNestedSelectable(child))
                {
                    continue;
                }

                RemoveNestedOutlines(owner, child, keep, visualRoot);
            }
        }

        private static Graphic FindSocketGraphic(CustomPressableSurface owner)
        {
            Transform existing = owner.transform.Find(CustomPressableSurface.SocketGhostName);
            return existing != null && existing.TryGetComponent(out Graphic socket) ? socket : null;
        }

        private static void RedirectSelectableTargetAwayFromSocket(CustomPressableSurface owner)
        {
            if (owner._selectable == null ||
                owner._socketGraphic == null ||
                owner._selectable.targetGraphic != owner._socketGraphic ||
                owner._pressableVisualRoot == null)
            {
                return;
            }

            Transform movingBackground = owner._pressableVisualRoot.Find(CustomPressableSurface.MovingRootGraphicName);
            Graphic movingGraphic = movingBackground != null
                                            ? movingBackground.GetComponent<Graphic>()
                                            : owner._pressableVisualRoot.GetComponentInChildren<Graphic>(true);
            if (movingGraphic != null && movingGraphic != owner._socketGraphic)
            {
                owner._selectable.targetGraphic = movingGraphic;
            }
        }

        private static Graphic CreateSocketGhost(CustomPressableSurface owner)
        {
            Transform existing = owner.transform.Find(CustomPressableSurface.SocketGhostName);
            RectTransform socketRect = existing as RectTransform;
            if (socketRect == null)
            {
                socketRect = CreateStretchedChild(owner, CustomPressableSurface.SocketGhostName);
            }

            socketRect.SetAsFirstSibling();

            Graphic socketGraphic = socketRect.GetComponent<Graphic>();
            if (socketGraphic == null)
            {
                Image socketImage = socketRect.gameObject.AddComponent<Image>();
                socketGraphic = socketImage;
            }

            socketGraphic.raycastTarget = false;
            return socketGraphic;
        }

        private static void ConfigureSocketGraphic(CustomPressableSurface owner, Graphic socketGraphic)
        {
            if (socketGraphic != null && socketGraphic.transform is RectTransform socketRect)
            {
                ConfigureIgnoredStretchedLayout(socketRect);
                socketRect.SetAsFirstSibling();
            }

            if (socketGraphic is Image socketImage)
            {
                CustomPressableSocketVisual.Configure(socketImage, owner.ResolvedSettings, owner.ResolvedVisualStyle);
                return;
            }

            if (socketGraphic != null)
            {
                socketGraphic.raycastTarget = false;
                socketGraphic.color = CustomPressableSocketVisual.ResolveColor(owner.ResolvedSettings, owner.ResolvedVisualStyle);
            }
        }

        private static void RemoveExtraSocketGhosts(CustomPressableSurface owner, RectTransform keep)
        {
            for (int i = owner.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = owner.transform.GetChild(i);
                if (child == keep)
                {
                    continue;
                }

                if (IsNamed(child, CustomPressableSurface.SocketGhostName))
                {
                    DestroySocketGhost(child);
                    continue;
                }

                if (!owner.IsOwnedByNestedSelectable(child))
                {
                    RemoveNestedSocketGhosts(owner, child, keep);
                }
            }
        }

        private static void RemoveNestedSocketGhosts(CustomPressableSurface owner, Transform parent, RectTransform keep)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child == keep)
                {
                    continue;
                }

                if (IsNamed(child, CustomPressableSurface.SocketGhostName))
                {
                    DestroySocketGhost(child);
                    continue;
                }

                if (!owner.IsOwnedByNestedSelectable(child))
                {
                    RemoveNestedSocketGhosts(owner, child, keep);
                }
            }
        }

        private static void DestroySocketGhost(Transform socketGhost)
        {
            if (socketGhost == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityObjectUtility.DestroySafely(socketGhost.gameObject);
                return;
            }
#endif

            UnityObjectUtility.DestroySafely(socketGhost.gameObject);
        }

        private static void ConfigureRootGraphicAsHitTarget(CustomPressableSurface owner, Graphic rootGraphic, Graphic socketGraphic)
        {
            if (rootGraphic == null)
            {
                return;
            }

            rootGraphic.raycastTarget = true;
            rootGraphic.raycastPadding = owner != null
                                             ? CustomButtonSettings.ToUnityRaycastPadding(owner.ResolvedHitRaycastPadding)
                                             : Vector4.zero;

            if (rootGraphic == socketGraphic)
            {
                return;
            }

            if (rootGraphic is not Image rootImage || rootImage.color.a <= 0f)
            {
                return;
            }

            Color hitOnlyColor = rootImage.color;
            hitOnlyColor.a = 0f;
            rootImage.color = hitOnlyColor;
        }

        private static void MoveExistingChildrenToVisualRoot(CustomPressableSurface owner)
        {
            for (int i = owner.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = owner.transform.GetChild(i);
                if (child == owner._pressableVisualRoot ||
                    child.name == CustomPressableSurface.SocketGhostName ||
                    child.name == CustomPressableSurface.MovingRootGraphicName)
                {
                    continue;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying && PrefabUtility.IsPartOfPrefabInstance(child.gameObject))
                {
                    continue;
                }
#endif

                child.SetParent(owner._pressableVisualRoot, false);
            }

            owner._pressableVisualRoot.SetAsLastSibling();
        }

        private static void NormalizePressableVisualRoot(CustomPressableSurface owner, RectTransform visualRoot)
        {
            if (visualRoot == null)
            {
                return;
            }

            bool preserveSelectableGeometry = ShouldPreserveSelectableGeometry(owner);
            ConfigureStretchedVisualRoot(visualRoot);
            if (preserveSelectableGeometry)
            {
                RemoveVisualRootLayoutGroups(visualRoot);
                RemoveManagedButtonLayers(owner, visualRoot);
                RemoveNestedPressableVisualRoots(owner, visualRoot, visualRoot);
                return;
            }

            ConfigureVisualRootLayoutGroup(visualRoot);
            RemoveNestedPressableVisualRoots(owner, visualRoot, visualRoot);
            NormalizePressableBackgrounds(owner, visualRoot);

            var backgrounds = new List<Transform>();
            var content = new List<Transform>();
            var outlines = new List<Transform>();

            for (int i = 0; i < visualRoot.childCount; i++)
            {
                Transform child = visualRoot.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (IsNamed(child, CustomPressableSurface.MovingRootGraphicName))
                {
                    backgrounds.Add(child);
                    ConfigureIgnoredStretchedLayout(child as RectTransform);
                    continue;
                }

                if (IsNamed(child, CustomPressableSurface.OutlineGraphicName))
                {
                    outlines.Add(child);
                    ConfigureIgnoredStretchedLayout(child as RectTransform);
                    continue;
                }

                if (ShouldPreserveSliderGeometry(owner, child))
                {
                    content.Add(child);
                    ConfigureIgnoredStretchedLayout(child as RectTransform);
                    continue;
                }

                content.Add(child);
            }

            int siblingIndex = 0;
            SetSiblingOrder(backgrounds, ref siblingIndex);
            SetSiblingOrder(content, ref siblingIndex);
            SetSiblingOrder(outlines, ref siblingIndex);
        }

        private static void ConfigureStretchedVisualRoot(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void ConfigureIgnoredStretchedLayout(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            LayoutElement layoutElement = rectTransform.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.ignoreLayout = true;
        }

        private static void ConfigureVisualRootLayoutGroup(RectTransform visualRoot)
        {
            if (visualRoot == null)
            {
                return;
            }

            HorizontalOrVerticalLayoutGroup layoutGroup = visualRoot.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = visualRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            if (layoutGroup == null)
            {
                return;
            }

            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
        }

        private static void RemoveVisualRootLayoutGroups(RectTransform visualRoot)
        {
            if (visualRoot == null)
            {
                return;
            }

            LayoutGroup[] layoutGroups = visualRoot.GetComponents<LayoutGroup>();
            for (int i = layoutGroups.Length - 1; i >= 0; i--)
            {
                LayoutGroup layoutGroup = layoutGroups[i];
                if (layoutGroup == null)
                {
                    continue;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityObjectUtility.DestroySafely(layoutGroup);
                    continue;
                }
#endif

                UnityObjectUtility.DestroySafely(layoutGroup);
            }
        }

        private static void RemoveManagedButtonLayers(CustomPressableSurface owner, RectTransform visualRoot)
        {
            if (visualRoot == null)
            {
                return;
            }

            // Slider geometry can legitimately contain children named "Outline" inside
            // its own visuals; only the direct managed button layers are removed here.
            for (int i = visualRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = visualRoot.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (IsNamed(child, CustomPressableSurface.MovingRootGraphicName) ||
                    IsNamed(child, CustomPressableSurface.OutlineGraphicName))
                {
                    if (IsNamed(child, CustomPressableSurface.MovingRootGraphicName) && child is RectTransform background)
                    {
                        RestoreSliderBackground(owner, background);
                    }
                    else
                    {
                        UnwrapAndDestroy(child as RectTransform, visualRoot);
                    }

                    continue;
                }
            }
        }

        private static void RestoreSliderBackground(CustomPressableSurface owner, RectTransform background)
        {
            if (owner == null || owner._selectable is not Slider slider || background == null)
            {
                UnwrapAndDestroy(background, background != null ? background.parent as RectTransform : null);
                return;
            }

            background.name = "Background";
            ConfigureSliderBackgroundRect(slider, background);
            RemoveLayoutElements(background);

            Image image = background.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
                image.type = Image.Type.Sliced;
                image.fillCenter = true;
                image.preserveAspect = false;
            }

            if (slider.GetComponent<SliderToggle>() != null)
            {
                RestoreSliderToggleHandleArea(slider, background);
                ConfigureSliderToggleBackgroundLayout(background);
            }
        }

        private static void ConfigureSliderBackgroundRect(Slider slider, RectTransform background)
        {
            if (slider.GetComponent<SliderToggle>() != null)
            {
                background.anchorMin = Vector2.zero;
                background.anchorMax = Vector2.one;
                background.offsetMin = Vector2.zero;
                background.offsetMax = Vector2.zero;
                background.pivot = new Vector2(0.5f, 0.5f);
                return;
            }

            if (ContainsName(slider.transform, "timeline"))
            {
                background.anchorMin = new Vector2(0f, 0.5f);
                background.anchorMax = new Vector2(1f, 0.5f);
                background.anchoredPosition = Vector2.zero;
                background.sizeDelta = new Vector2(0f, 8f);
                background.pivot = new Vector2(0.5f, 0.5f);
                return;
            }

            background.anchorMin = new Vector2(0f, 0.35f);
            background.anchorMax = new Vector2(1f, 0.65f);
            background.anchoredPosition = Vector2.zero;
            background.sizeDelta = Vector2.zero;
            background.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void RemoveLayoutElements(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            LayoutElement[] layoutElements = rectTransform.GetComponents<LayoutElement>();
            for (int i = layoutElements.Length - 1; i >= 0; i--)
            {
                LayoutElement layoutElement = layoutElements[i];
                if (layoutElement == null)
                {
                    continue;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityObjectUtility.DestroySafely(layoutElement);
                    continue;
                }
#endif

                UnityObjectUtility.DestroySafely(layoutElement);
            }
        }

        private static void RestoreSliderToggleHandleArea(Slider slider, RectTransform background)
        {
            RectTransform handleArea = slider.handleRect != null ? slider.handleRect.parent as RectTransform : null;
            if (handleArea == null || handleArea == background || handleArea.parent == background)
            {
                return;
            }

            handleArea.SetParent(background, false);
        }

        private static void ConfigureSliderToggleBackgroundLayout(RectTransform background)
        {
            HorizontalLayoutGroup layoutGroup = background.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = background.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            RectOffset padding = layoutGroup.padding;
            padding.left = 7;
            padding.right = 7;
            padding.top = 2;
            padding.bottom = 2;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
        }

        private static bool ContainsName(Transform transform, string value)
        {
            return transform != null &&
                   !string.IsNullOrEmpty(value) &&
                   transform.name.IndexOf(value, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void SetSiblingOrder(List<Transform> children, ref int siblingIndex)
        {
            for (int i = 0; i < children.Count; i++)
            {
                Transform child = children[i];
                if (child == null)
                {
                    continue;
                }

                child.SetSiblingIndex(siblingIndex);
                siblingIndex++;
            }
        }

        private static bool IsNamed(Transform child, string name)
        {
            return string.Equals(child.name, name, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldPreserveSliderGeometry(CustomPressableSurface owner, Transform child)
        {
            if (owner == null || owner._selectable is not Slider slider || child == null)
            {
                return false;
            }

            return IsNamed(child, "Fill Area") ||
                   IsNamed(child, "Handle Slide Area") ||
                   ContainsSliderRect(child, slider.fillRect) ||
                   ContainsSliderRect(child, slider.handleRect);
        }

        private static bool ShouldPreserveSelectableGeometry(CustomPressableSurface owner)
        {
            return owner != null && owner._selectable is Slider && !IsScrollbarSlider(owner._selectable);
        }

        private static bool IsScrollbarSlider(Selectable selectable)
        {
            return selectable != null &&
                   (ContainsName(selectable.transform, "scrollbar") ||
                    ContainsName(selectable.transform, "scroll rect") ||
                    ContainsName(selectable.transform, "scrollrect") ||
                    selectable.GetComponent<ScrollRectSliderAdapter>() != null);
        }

        private static void RestorePreservedSelectableTargetGraphic(CustomPressableSurface owner)
        {
            if (owner == null || owner._selectable is not Slider slider)
            {
                return;
            }

            Graphic targetGraphic = ResolvePreservedSliderTargetGraphic(slider);
            if (targetGraphic != null && slider.targetGraphic != targetGraphic)
            {
                slider.targetGraphic = targetGraphic;
            }
        }

        private static Graphic ResolvePreservedSliderTargetGraphic(Slider slider)
        {
            if (slider == null)
            {
                return null;
            }

            Graphic handleGraphic = slider.handleRect != null ? slider.handleRect.GetComponent<Graphic>() : null;
            if (handleGraphic != null)
            {
                return handleGraphic;
            }

            Graphic fillGraphic = slider.fillRect != null ? slider.fillRect.GetComponent<Graphic>() : null;
            if (fillGraphic != null)
            {
                return fillGraphic;
            }

            Graphic existingTarget = slider.targetGraphic;
            if (existingTarget != null &&
                !IsNamed(existingTarget.transform, CustomPressableSurface.MovingRootGraphicName) &&
                !IsNamed(existingTarget.transform, CustomPressableSurface.OutlineGraphicName) &&
                !IsNamed(existingTarget.transform, CustomPressableSurface.SocketGhostName))
            {
                return existingTarget;
            }

            Graphic[] graphics = slider.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic == null ||
                    graphic == slider.GetComponent<Graphic>() ||
                    IsNamed(graphic.transform, CustomPressableSurface.MovingRootGraphicName) ||
                    IsNamed(graphic.transform, CustomPressableSurface.OutlineGraphicName) ||
                    IsNamed(graphic.transform, CustomPressableSurface.SocketGhostName))
                {
                    continue;
                }

                return graphic;
            }

            return null;
        }

        private static bool ContainsSliderRect(Transform candidate, RectTransform sliderRect)
        {
            return candidate != null &&
                   sliderRect != null &&
                   (sliderRect == candidate || sliderRect.IsChildOf(candidate));
        }

        private void ConfigureGraphicRaycastTargets(CustomPressableSurface owner)
        {
            Graphic rootGraphic = owner.GetComponent<Graphic>();
            Graphic hitGraphic = rootGraphic != null ? rootGraphic : owner._socketGraphic;

            if (hitGraphic != null)
            {
                hitGraphic.raycastTarget = true;
                hitGraphic.raycastPadding = CustomButtonSettings.ToUnityRaycastPadding(owner.ResolvedRaycastPadding);
            }

            Graphic[] graphics = owner.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic == null ||
                    graphic == rootGraphic ||
                    graphic == hitGraphic ||
                    IsOwnedByNestedSelectable(owner, graphic.transform))
                {
                    continue;
                }

                graphic.raycastTarget = false;
            }
        }

        private void SuppressMaskGraphics(CustomPressableSurface owner)
        {
            if (owner == null)
            {
                return;
            }

            Mask[] masks = owner.GetComponentsInChildren<Mask>(true);
            for (int i = 0; i < masks.Length; i++)
            {
                Mask mask = masks[i];
                if (mask == null || IsOwnedByNestedSelectable(owner, mask.transform))
                {
                    continue;
                }

                mask.showMaskGraphic = false;
            }

            RectMask2D[] rectMasks = owner.GetComponentsInChildren<RectMask2D>(true);
            for (int i = 0; i < rectMasks.Length; i++)
            {
                RectMask2D rectMask = rectMasks[i];
                if (rectMask == null ||
                    IsOwnedByNestedSelectable(owner, rectMask.transform) ||
                    rectMask.GetComponent<Mask>() != null)
                {
                    continue;
                }

                if (!rectMask.TryGetComponent(out Graphic graphic) || graphic == null)
                {
                    continue;
                }

                Color color = graphic.color;
                color.a = 0f;
                graphic.color = color;
                graphic.raycastTarget = false;
            }
        }

        private static bool CanAutoModifyHierarchy(CustomPressableSurface owner)
        {
#if UNITY_EDITOR
            if (!EditorUtility.IsPersistent(owner.gameObject))
            {
                return true;
            }

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            return prefabStage != null &&
                   prefabStage.prefabContentsRoot != null &&
                   owner.transform.IsChildOf(prefabStage.prefabContentsRoot.transform);
#else
            return true;
#endif
        }

    }
}
