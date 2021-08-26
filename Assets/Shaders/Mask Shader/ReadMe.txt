This project is related to an answer I provided in the Unity forums at:
http://forum.unity3d.com/threads/circular-fade-in-out-shader.344816/

I hope it proves useful!

Peter
http://www.console-dev.de

---

The "Screen Transition" is implemented as an Image Effect.
See http://docs.unity3d.com/Manual/comp-ImageEffects.html for details about image effects in Unity.


Open "Assets/Scene.unity" and select the "Main Camera" GameObject in the Hierarchy Window. Notice it has the "Screen Transition Image Effect" Component attached. Use this Component to modify the progress of the transition.

The transition effect is implemented using a texture mask. This provides a way to author lots of different effects just by changing the texture and allows to create new transitons without code, so it's a perfect fit for artists.

The texture mask has to be a grayscale texture. The pixel intensity represents the progression of the transitioning effect. For example, encoding a vertical gradient from white to black creates a vertical swipe transition. Encoding a glow provides the circle like effect you are looking for. This only gives a range of 256 steps for the transition, but this is often enough. Black pixels blend in first, white pixels last.

The mask texture has to use special settings. If you add a new mask, make sure to apply the following settings in the Texture Inspector window:

Texture Type = Advanced
Alpha from Grayscale = True
Bypass sRGB Sample = True
Wrap = Clamp
Format = Alpha8