using System.Collections.Generic;

namespace Deucarian.XRUI.Controls
{
    internal static class CustomSelectableTransientOwners
    {
        private static readonly HashSet<CustomSelectableFeedback> TransientOwners = new();
        private static readonly List<CustomSelectableFeedback> OwnerSnapshot = new();

        public static void Register(CustomSelectableFeedback owner)
        {
            if (owner == null || TransientOwners.Contains(owner))
            {
                return;
            }

            OwnerSnapshot.Clear();
            OwnerSnapshot.AddRange(TransientOwners);

            for (int i = 0; i < OwnerSnapshot.Count; i++)
            {
                CustomSelectableFeedback currentOwner = OwnerSnapshot[i];
                if (currentOwner == null)
                {
                    TransientOwners.Remove(currentOwner);
                    continue;
                }

                if (currentOwner != owner)
                {
                    if (currentOwner.BlocksCompetingTransientOwner)
                    {
                        OwnerSnapshot.Clear();
                        return;
                    }

                    currentOwner.ClearTransientStateFromCompetingOwner();
                }
            }

            OwnerSnapshot.Clear();
            TransientOwners.Add(owner);
        }

        public static void Unregister(CustomSelectableFeedback owner)
        {
            TransientOwners.Remove(owner);
        }
    }
}
