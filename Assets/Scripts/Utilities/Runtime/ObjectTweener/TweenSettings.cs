using UnityEngine;


namespace Project.Utilities.Tween
{

    /// <summary>
    /// Indique la fonction de tween à utiliser pour cette transition.
    /// </summary>
    public enum TweenAnimationType
    {
        Move = 0,
        Rot = 4,
        Scale = 5,
        Fade = 6,
        Value = 7,
        Sequence = 8,
        MoveX = 9,
        MoveY = 10,
        MoveZ = 11,
        RotX = 12,
        RotY = 13,
        RotZ = 14,
        ScaleX = 15,
        ScaleY = 16,
        ScaleZ = 17,
        Color = 18,
        Width = 19,
        Height = 20,
    }


    /// <summary>
    /// Indique les paramètres d'une transition tween.
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(menuName = "Tween/New Tween Settings", fileName = "New Tween Settings")]
    public class TweenSettings : ScriptableObject
    {
        [Tooltip("Indique la fonction de tween à utiliser pour cette transition.")]
        public TweenAnimationType animationType = TweenAnimationType.Move;

        [Tooltip("Indique l'AnimationCurve à utiliser pour cette transition.")]
        public LeanTweenType curveType = LeanTweenType.animationCurve;

        [Tooltip("Utilisée uniquement si curveType = LeanTweenType.animationCurve")]
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);


        [Tooltip("La durée d'une transition.")]
        public float duration = .2f;
        [Tooltip("Le délai avant que la transition ne se joue.")]
        public float delay = 0f;

        [Tooltip("Rejoue le tween depuis le début.")]
        public bool loop = false;

        [Tooltip("Rejoue le tween à l'envers et en boucle une fois terminé.")]
        public bool pingPong = false;


        [Tooltip("Si à true, le script utilisera from comme variable de départ (absolue) au lieu de la valeur actuelle de l'objet.")]
        public bool useFromAsStart = false;
        [Tooltip("Si à true, les transformations seront en relatives à l'objet au lieu d'être absolues.")]
        public bool relativeToTransform = false;
        public Vector3 from;
        public Vector3 to;
        public Color fromColor;
        public Color toColor;

        [Tooltip("Pour lancer des tweens en succession une fois ce tween terminé.")]
        public TweenSettings[] OnComplete;



        /// <summary>
        /// Convertit "from" de l'espace global à local.
        /// </summary>
        /// <param name="t">La transform utilisée pour la conversion.</param>
        /// <returns></returns>
        public Vector3 RelativeFrom(Transform t)
        {
            return relativeToTransform ? t.TransformPoint(from) : from;
        }

        /// <summary>
        /// Convertit "to" de l'espace global à local.
        /// </summary>
        /// <param name="t">La transform utilisée pour la conversion.</param>
        /// <returns></returns>
        public Vector3 RelativeTo(Transform t)
        {
            return relativeToTransform ? t.TransformPoint(to) : to;
        }

    }
}