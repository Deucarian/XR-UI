# Deucarian XR UI

## What this is

Deucarian XR UI provides standalone XR-ready uGUI controls for Deucarian Unity projects. The package centers on press-gated controls: hover and proximity can show feedback, but activation happens only after the configured press depth is reached.

The core package has neutral built-in colors and settings. Install `com.deucarian.xr-ui.theming-integration` when a project wants Deucarian Theming to drive the XR UI palette.

Package ID: `com.deucarian.xr-ui`

Current package version: `0.1.1`.

## When to use it

- You need XR-ready buttons, toggles, sliders, dropdown/input press targets, and world-space uGUI helpers.
- You want XRI poke affordance helpers without coupling the project to Deucarian Theming.
- You need runtime hooks for event camera, head transform, settings, and automatic feedback installation.

## When not to use it

- You only need theme color role mapping; install `com.deucarian.xr-ui.theming-integration` alongside this package for that.
- You need app-specific XR navigation, command routing, or scene flow.
- You need a generic input system or Diagnostics dashboard.

## Install

Stable:

```json
"com.deucarian.xr-ui": "https://github.com/Deucarian/XR-UI.git#main"
```

Development:

```json
"com.deucarian.xr-ui": "https://github.com/Deucarian/XR-UI.git#develop"
```

## Unity compatibility

Requires Unity `2022.3` or newer.

## 60-second quick start

1. Install the package through Unity Package Manager or the Deucarian Package Installer.
2. Import the `Pressable Controls` sample.
3. Add `XrUiPressableControlsSample` from the imported sample to an empty GameObject in a scene with an XR Origin.
4. Assign a world-space canvas event camera through `XrUiRuntimeServices` if your project does not use `Camera.main`.

```csharp
using Deucarian.XRUI;
using UnityEngine;

public sealed class XrUiCameraProvider : MonoBehaviour, IXrUiCameraProvider
{
    [SerializeField] private Camera eventCamera;
    [SerializeField] private Transform headTransform;

    public Camera EventCamera => eventCamera;
    public Transform HeadTransform => headTransform;

    private void OnEnable()
    {
        XrUiRuntimeServices.SetCameraProvider(this);
    }

    private void OnDisable()
    {
        XrUiRuntimeServices.SetCameraProvider(null);
    }
}
```

## Samples

- `Pressable Controls`: runtime-created uGUI examples for buttons, toggles, sliders, and dropdowns.
- `Spatial Keyboard Adapter`: notes for wiring Unity XR Interaction Toolkit Spatial Keyboard samples to press-gated XR UI controls.

## Public API map

- `CustomButton`, `CustomDropdown`, `CustomTmpDropdown`, `CustomSlider`, `SliderToggle`: press-gated uGUI controls.
- `CustomPressableSurface` and `CustomSelectableFeedback`: visual feedback and press-depth state.
- `CustomTogglePressTarget`, `CustomDropdownPressTarget`, `CustomInputFieldPressTarget`: adapters for existing uGUI controls.
- `WorldCanvasEventCameraAssigner`: world-space canvas event camera assignment.
- `XrUiPokeAffordanceInstaller` and `XrUiPokeFollowAffordance`: XR Interaction Toolkit poke affordance helpers.
- `XrUiRuntimeServices`, `IXrUiCameraProvider`, `IXrUiSettingsProvider`: project-provided runtime hooks.
- `XrUiColorPalette` and `XrUiSemanticColor`: neutral color palette model used by the core package and optional theming integration.

## Runtime hooks

- `XrUiRuntimeServices.SetCameraProvider(...)` lets a project provide the event camera and head transform.
- `XrUiRuntimeServices.SetSettingsProvider(...)` lets a project provide runtime settings such as keyboard raycast protection.
- `XrUiControlExclusionRegistry` lets a project opt specific controls or subtrees out of automatic pressable feedback installation.

## Integrations

Works with:

- Unity uGUI, TextMeshPro, Input System, XR Core Utils, and XR Interaction Toolkit.
- `com.deucarian.common` for approved runtime cleanup.

Optional integrations:

- `com.deucarian.xr-ui.theming-integration` maps Deucarian Theming color roles into XR UI palettes.

Does not own:

- Deucarian Theming's color role source of truth.
- App-specific XR interaction flows.
- Generic input frameworks.
- Diagnostics dashboards.

## Troubleshooting

- If controls hover but do not activate, check the configured press depth and XRI poke interactor setup.
- If a world-space canvas does not receive events, verify the event camera provider or `WorldCanvasEventCameraAssigner`.
- If keyboard rays hit hidden blockers, configure runtime settings through `XrUiRuntimeServices.SetSettingsProvider(...)`.
- If theme colors do not update, confirm that `com.deucarian.xr-ui.theming-integration` is installed.

## Validation

Run the shared package validator:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Run Unity EditMode tests when changing runtime/editor code or asmdefs.

## Architecture / Contributor Notes

See [AGENTS.md](AGENTS.md) for ownership, dependency, and validation guidance.

## License

See [LICENSE.md](LICENSE.md).
