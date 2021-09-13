using Project.Enums;
using UnityEditor;
using UnityEngine;

using static Project.Utilities.ValueTypes.Arrays;

namespace Project.ScreenFader.Editor
{
    [CustomEditor(typeof(ScreenFaderSO))]
    public class ScreenFaderSOEditor : UnityEditor.Editor
    {
        private SerializedProperty _fadeShader;
        private SerializedProperty _blendShader;
        private SerializedProperty _params;
        private SerializedProperty _maskValue;

        private ScreenFadeRenderer _sfr;
        private GameObject _camEmpty;
        private Camera _renderCam;




        private void OnEnable()
        {
            _fadeShader = serializedObject.FindProperty("_fadeShader");
            _blendShader = serializedObject.FindProperty("_blendShader");
            _params = serializedObject.FindProperty("_params");
            _maskValue = serializedObject.FindProperty("_maskValue");



            if (Application.isPlaying) return;

            Component[] o = FindObjectsOfType<ScreenFadeRenderer>();
            DestroyAllIn(o);


            //Pour visualiser les effets, on crée une caméra par code sur laquelle on appelle OnRenderImage
            _camEmpty = new GameObject("ScreenFader cam test");
            _camEmpty.transform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
            _camEmpty.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;

            _sfr = _camEmpty.AddComponent<ScreenFadeRenderer>();
            _sfr.SetScreenFader(target as ScreenFaderSO, true);
            

            _renderCam = _camEmpty.GetComponent<Camera>();  //La caméra est crée automatiquement par le ScreenFadeRenderer
            _renderCam.depth = 10f;

        }

        private void OnDisable()
        {
            _sfr.SetCanvases();
            DestroyImmediate(_camEmpty);
        }


        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_fadeShader);
            EditorGUILayout.PropertyField(_blendShader);
            EditorGUILayout.PropertyField(_params);

            if (_params.objectReferenceValue)
            {
                EditorGUILayout.PropertyField(_maskValue);

                TransitionSettingsSO settings = _params.objectReferenceValue as TransitionSettingsSO;
                if(settings.TransitionType == FaderTransitionType.TextureBlend)
                {
                    EditorGUILayout.HelpBox("Le Transition Type des paramètres est en TextureBlend ; aucun changement ne sera affiché à l'écran.", MessageType.Warning);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

        }
    }
}