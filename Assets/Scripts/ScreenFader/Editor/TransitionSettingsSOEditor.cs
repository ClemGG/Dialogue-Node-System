using UnityEditor;

using Project.Enums;

namespace Project.ScreenFader.Editor
{

    [CustomEditor(typeof(TransitionSettingsSO))]
    [CanEditMultipleObjects]
    public class TransitionSettingsSOEditor : UnityEditor.Editor
    {
        private SerializedProperty _transitionType;
        private SerializedProperty _fadeCurve;
        private SerializedProperty _speed;
        private SerializedProperty _delayAfterTransition;
        private SerializedProperty _fadeColor;
        private SerializedProperty _maskTexture;
        private SerializedProperty _invertMask;
        private SerializedProperty _maskSpread;


        private void OnEnable()
        {
            _transitionType = serializedObject.FindProperty("TransitionType");
            _fadeCurve = serializedObject.FindProperty("FadeCurve");
            _speed = serializedObject.FindProperty("Speed");
            _delayAfterTransition = serializedObject.FindProperty("DelayAfterTransition");
            _fadeColor = serializedObject.FindProperty("FadeColor");
            _maskTexture = serializedObject.FindProperty("MaskTexture");
            _invertMask = serializedObject.FindProperty("InvertMask");
            _maskSpread = serializedObject.FindProperty("MaskSpread");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_transitionType);
            EditorGUILayout.PropertyField(_fadeCurve);
            EditorGUILayout.PropertyField(_speed);
            EditorGUILayout.PropertyField(_delayAfterTransition);

            FaderTransitionType value = (FaderTransitionType)_transitionType.enumValueIndex;
            if (value == FaderTransitionType.ColoredFade || value == FaderTransitionType.Mask)
            {
                EditorGUILayout.PropertyField(_fadeColor);
            }
            if (value == FaderTransitionType.Mask)
            {
                EditorGUILayout.PropertyField(_maskTexture);
                EditorGUILayout.PropertyField(_invertMask);
                EditorGUILayout.PropertyField(_maskSpread);
            }





            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}