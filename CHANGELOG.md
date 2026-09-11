# Changelog

## [0.3.0] - 2026-09-11

- Direct new visual palette authoring to Theming; keep existing XR palette assets under a clearly labeled compatibility fallback.
- Accept fully resolved interaction state colors so theme-owned pressed and selected colors remain independent and are not multiplied twice.

## [0.2.3] - 2026-09-11

- Use native XR settings and custom button Inspectors with shared controls, isolated preview and existing serialized/animation behavior.
- Require Editor 1.10.6 for the shared native controls, typography, responsive layouts and accessible interaction states.

## [0.2.2] - 2026-09-10

### Changed

- Adopt the shared Editor 1.7 workspace presentation: neutral surfaces, readable typography, consistent actions and aligned controls.
- Preserve package workflows and native serialized editing.
- Separate managed visual sprite-override handling from socket styling, retaining public APIs, serialized settings, and existing Edit Mode/Play Mode behavior. Keep each extracted source file below the 500-line review limit.

## [0.2.1] - 2026-09-09

- Register package tooling and navigation actions as shared Control Center pages. Preserve the domain workflow while using Editor-owned submenus, in-window navigation, and UI scaling.

## 0.2.0 - Unreleased

- Add explicit palette contexts/scopes and owner-aware fallback registration. Resolve local palettes for graphics, buttons and sliders.

## Unreleased

- Registered XR UI settings and palette actions with Deucarian Control Center and removed their global menu entries.

## 0.1.1 - 2026-07-17

- Preserved press-gated TMP input-field activation on Unity 2022.3, where `shouldActivateOnSelect` is not yet available.
- Added importable Pressable Controls and Spatial Keyboard Adapter scenes and aligned the exact Common dependency.

## 0.1.0

- Initial standalone extraction of Deucarian XR UI pressable controls.
- Added neutral palette/settings fallback and generic runtime provider hooks.
- Added XRI poke affordance helpers and automatic world canvas event-camera assignment.
