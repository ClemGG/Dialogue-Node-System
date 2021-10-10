
namespace Project.Enums
{

    /// <summary>
    /// The transition type of the ScreenFade (Mask, Fade or Blend)
    /// </summary>
    public enum FaderTransitionType
    {
        Mask = 0,           //Uses a texture to mask the UI
        ColoredFade = 1,    //Creates a colored fade to transition towards the next scene
        TextureBlend = 2,   //Creates a transparent blend towards the next texture
    }
}


