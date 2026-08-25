# Deucarian XR UI Agent Notes

Package ID: `com.deucarian.xr-ui`
Repository: `Deucarian/XR-UI`

Follow the canonical Deucarian governance docs in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/main/ARCHITECTURE.md), especially capability ownership and dependency rules.

## Ownership

This package owns:

- XR-ready uGUI pressable controls, world canvas helpers, poke affordances, neutral XR UI palettes, and runtime service hooks for XR UI behavior.

Registered capabilities:
- `xr-world-ui-controls`

This package must not own:

- General theming governance, app-specific XR interaction flows, generic input frameworks, diagnostics dashboards, or package installation.

## Dependencies

Allowed dependency shape:

- May depend on Common for approved runtime cleanup, Unity UI/TextMeshPro/Input System/XR packages for XR-ready controls, and optional integration packages for theme mapping.

Required dependencies and why:

- `com.deucarian.common`: approved Unity object lifetime helper and shared runtime primitive owner.
- `com.unity.inputsystem`: input primitives used by XR interaction flows.
- `com.unity.textmeshpro`: TMP dropdown/input support.
- `com.unity.ugui`: uGUI controls and selectable base classes.
- `com.unity.xr.core-utils`: XR origin and utility support.
- `com.unity.xr.interaction.toolkit`: poke interaction and XR affordance support.

Optional/version-defined dependencies:

- None in this package. `com.deucarian.xr-ui.theming-integration` is a separate optional integration package, not a hard dependency.

Architecture exceptions:

- None.

## Policies

- Logging: Do not add diagnostics/logging unless package behavior actually needs it; if needed, use Deucarian Logging and update all metadata together.
- Common: Use Common-owned cleanup helpers instead of local copies for production Unity object cleanup.
- Editor UI: Editor code may support this package's inspectors only; do not become the shared editor shell.
- Diagnostics: Do not become Diagnostics; expose local runtime state only when needed by this package.
- Testing: Test fixture teardown may use `DestroyImmediate` directly.

## Validation

Run the shared validator before committing:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Also run existing repository tests when changing code or asmdefs. Documentation-only updates should still run `git diff --check`.

## Codex Guidance

- Inspect current files before changing anything.
- Work on `develop`; do not edit or merge `main` unless the task is promotion-only.
- Do not edit `Library/PackageCache`.
- Do not guess package versions or dependency versions.
- Do not add package dependencies casually; update asmdefs, `package.json`, `deucarian-package.json`, Package Registry, Package Installer fallback catalog, and Bootstrap fallback catalog together when a dependency is truly required.
- Do not create local copies of shared helpers.
- Keep commits focused and report exactly what changed and what was validated.

## Before Adding Code

- Confirm the change fits this package's ownership boundary.
- Reuse existing local patterns and helpers.
- Avoid broad refactors without audit support.
- Preserve runtime/editor behavior unless the task explicitly asks to change it.

## Before Adding A Dependency

- Is the capability already owned by that package?
- Is it used by production code, editor code, sample code, or tests?
- Does the asmdef reference match `package.json`?
- Does `deucarian-package.json` need updating?
- Does Package Registry need updating?
- Does Package Installer fallback catalog need updating?
- Does Bootstrap fallback catalog need updating?
- Are exact versions propagated without guessing?

## Before Adding A Helper

- Is this package the capability owner?
- Is this behavior repeated in at least three production packages?
- Is there an existing owner package?
- Should this remain local?
- Has the audit been updated?

## Debug And Unity Object Lifetime

- Direct Unity Debug calls are forbidden in production code.
- Production Unity object cleanup must use Common `UnityObjectUtility.DestroySafely`.
- Test fixture teardown may use `DestroyImmediate` directly.
