# puppets (v1, Nov 26)

Built by Claude from the design team's PSD (`Art sketch game.psd`, pages Big Body and Thin Body).
Each character uses the team's own line art: head, torso and front arm are separate layers.

| Key | From | Skin / hair (placeholder colours) |
|---|---|---|
| big_f | Big Body, left (long hair) | #C68A5E / #54341E |
| big_m | Big Body, right (curly) | #E8B086 / #2A1E18 |
| thin_f | Thin Body, left (wavy) | #B98256 / #7A3A20 |
| thin_m | Thin Body, right (short) | #8D5A3B / #221A16 |

What Claude did: filled skin, hair and eye whites inside the line art; turned the black tops into
the teal Silong jumpsuit (the seat number and zip are added in Unity); scaled each character to
640 px tall. Game files: `Assets/_Project/Resources/Puppets/<key>_<head|torso|arm>.png`.

The rig (Puppet.cs): torso breathes, head nods from the neck and tilts toward the pointer,
eyelids blink (and stay shut at Night), the front arm swings from the shoulder now and then.
Big seats use big bodies, others thin; the two characters of each body alternate.

Known rough spots for the design team to repaint: the long hair (big_f) has a straight cut at the
shoulders; ears and sideburn shapes were filled automatically. Expressions (thinking, shocked,
happy, angry) still need swappable eye, brow and mouth drawings (checklist C03, C15).
