# Pressable Controls Sample

Open **PressableControls.unity** and press Play. The camera, world-space canvas,
event system, button, toggle, slider handle/fill and dropdown template are wired.
Hold a button briefly for the desktop press fallback; drag the slider and choose
a dropdown item. The status label reports each action.

Select **Example canvas** for the serialized sample references. Expand its children
to inspect each press surface, feedback component and persistent button event.
The core sample uses XR UI's authored palette; import Themed Pressable Controls
from the theming integration package to try the shared Deucarian visual family.

This scene uses the built-in input module. In an Input-System-only project replace
it with InputSystemUIInputModule. Device hand/poke testing additionally requires
your project's XR rig; this desktop example does not silently install one.
