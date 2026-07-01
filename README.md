# Deucarian XR UI

Standalone XR-ready uGUI controls for Deucarian Unity projects.

The package centers on press-gated controls: hover and proximity can show feedback, but activation happens only after the configured press depth is reached. It includes custom buttons, toggles, sliders, dropdown/input press targets, world-space canvas event-camera assignment, and XRI poke affordance helpers.

The core package has neutral built-in colors and settings. Install `com.deucarian.xr-ui.theming-integration` when a project wants Deucarian Theming to drive the XR UI palette.

## Runtime Hooks

- `XrUiRuntimeServices.SetCameraProvider(...)` lets a project provide the event camera and head transform.
- `XrUiRuntimeServices.SetSettingsProvider(...)` lets a project provide runtime settings such as keyboard raycast protection.
- `XrUiControlExclusionRegistry` lets a project opt specific controls or subtrees out of automatic pressable feedback installation.

## Samples

Import `Pressable Controls` for a small runtime-created uGUI setup. Import `Spatial Keyboard Adapter` only in projects that also import Unity's XR Interaction Toolkit Spatial Keyboard sample.
