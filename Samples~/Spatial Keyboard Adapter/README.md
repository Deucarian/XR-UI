# Spatial Keyboard Adapter

Runtime code in `com.deucarian.xr-ui` intentionally does not depend on Unity's XR Interaction Toolkit Spatial Keyboard sample assembly.

Use this sample area for project-local adapters after importing Unity's Spatial Keyboard sample. The recommended pattern is to route key/input-field activation through `CustomPressableSurface` and `ICustomPressActivationTarget`, then keep the adapter code in the consuming project or imported sample.
