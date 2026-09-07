# Core combatant sprite sources

These source sheets establish the first production art standard for combatants.
The runtime atlases in `Resources/Sprites/Characters/Animated` use a fixed 4 by 4
grid with 128 by 128 pixel cells.

Rows have a stable meaning:

- row 1: idle
- row 2: attack
- row 3: hit reaction
- row 4: defeat

Every row contains four frames. Standing frames share a ground baseline and all
textures use nearest-neighbor filtering. The intended visual density is roughly
48 by 64 logical pixels per standing fighter, with a limited palette, two or
three tones per material, and no antialiasing or texture noise.

Altarion casts with empty hands. A staff, wand, or other focus is not part of
his design. Aldric always carries his sword in his right hand and his round
shield on his left arm. Enemy sheets face toward the player party throughout
every animation, including defeat.

The source images were generated with the built-in image generation tool and
then normalized to the runtime grid with nearest-neighbor resampling. They are
kept at their original resolution for manual correction in Aseprite.
