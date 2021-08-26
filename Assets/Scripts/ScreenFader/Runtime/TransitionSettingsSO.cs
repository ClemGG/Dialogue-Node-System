using UnityEngine;
using Project.Enums;

namespace Project.ScreenFader
{

    [CreateAssetMenu(fileName = "New Transition Settings", menuName = "Scriptable Objects/Screen Fader/New Transition Settings")]
    public class TransitionSettingsSO : ScriptableObject
    {
        public FaderTransitionType TransitionType = FaderTransitionType.Mask;
        public AnimationCurve FadeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public float Speed = 1f;
        public float DelayAfterTransition = 0f;


        //Mask & Fade settings
        public Color FadeColor = Color.black;
        public Texture2D MaskTexture;
        public bool InvertMask;
        [Range(0f, 1f)] public float MaskSpread; // Si TransitionType = Mask, indique le niveau d'adoucissement du masque (0 : Brut ; 1 : Flou)


    }
}