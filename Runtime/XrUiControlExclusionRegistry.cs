using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.XRUI
{
    public static class XrUiControlExclusionRegistry
    {
        private static readonly List<Func<Selectable, bool>> SelectablePredicates = new();
        private static readonly List<Func<Transform, bool>> TransformPredicates = new();

        public static void RegisterSelectablePredicate(Func<Selectable, bool> predicate)
        {
            if (predicate != null && !SelectablePredicates.Contains(predicate))
            {
                SelectablePredicates.Add(predicate);
            }
        }

        public static void UnregisterSelectablePredicate(Func<Selectable, bool> predicate)
        {
            SelectablePredicates.Remove(predicate);
        }

        public static void RegisterTransformPredicate(Func<Transform, bool> predicate)
        {
            if (predicate != null && !TransformPredicates.Contains(predicate))
            {
                TransformPredicates.Add(predicate);
            }
        }

        public static void UnregisterTransformPredicate(Func<Transform, bool> predicate)
        {
            TransformPredicates.Remove(predicate);
        }

        public static bool IsExcluded(Selectable selectable)
        {
            if (selectable == null)
            {
                return false;
            }

            if (IsExcluded(selectable.transform))
            {
                return true;
            }

            for (int i = 0; i < SelectablePredicates.Count; i++)
            {
                Func<Selectable, bool> predicate = SelectablePredicates[i];
                if (predicate != null && predicate(selectable))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsExcluded(Transform transform)
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.name == "XR Device Simulator UI" ||
                    HasComponentNamed(current, "XRDeviceSimulatorUI"))
                {
                    return true;
                }

                for (int i = 0; i < TransformPredicates.Count; i++)
                {
                    Func<Transform, bool> predicate = TransformPredicates[i];
                    if (predicate != null && predicate(current))
                    {
                        return true;
                    }
                }

                current = current.parent;
            }

            return false;
        }

        private static bool HasComponentNamed(Transform transform, string typeName)
        {
            MonoBehaviour[] behaviours = transform.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour != null && behaviour.GetType().Name == typeName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
