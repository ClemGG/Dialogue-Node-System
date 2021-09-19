using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Project.Utilities.Tween.Editor
{
    [CustomEditor(typeof(TweenSettings))]
    [CanEditMultipleObjects]
    public class TweenSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _animationType;
        private SerializedProperty _curveType;
        private SerializedProperty _curve;
        private SerializedProperty _duration;
        private SerializedProperty _delay;
        private SerializedProperty _loop;
        private SerializedProperty _pingPong;
        private SerializedProperty _useFromAsStart;
        private SerializedProperty _relativeToTransform;
        private SerializedProperty _from;
        private SerializedProperty _to;
        private SerializedProperty _fromColor;
        private SerializedProperty _toColor;
        private SerializedProperty _onComplete;


        private void OnEnable()
        {
            _animationType = serializedObject.FindProperty("AnimationType");
            _curveType = serializedObject.FindProperty("CurveType");
            _curve = serializedObject.FindProperty("Curve");
            _duration = serializedObject.FindProperty("Duration");
            _delay = serializedObject.FindProperty("Delay");
            _loop = serializedObject.FindProperty("Loop");
            _pingPong = serializedObject.FindProperty("PingPong");
            _useFromAsStart = serializedObject.FindProperty("UseFromAsStart");
            _relativeToTransform = serializedObject.FindProperty("RelativeToTransform");
            _from = serializedObject.FindProperty("From");
            _to = serializedObject.FindProperty("To");
            _fromColor = serializedObject.FindProperty("FromColor");
            _toColor = serializedObject.FindProperty("ToColor");
            _onComplete = serializedObject.FindProperty("OnColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_animationType);
            EditorGUILayout.PropertyField(_curveType);
            EditorGUILayout.PropertyField(_curve);
            EditorGUILayout.PropertyField(_duration);
            EditorGUILayout.PropertyField(_delay);
            EditorGUILayout.PropertyField(_loop);
            EditorGUILayout.PropertyField(_pingPong);
            EditorGUILayout.PropertyField(_useFromAsStart);
            EditorGUILayout.PropertyField(_relativeToTransform);


            switch (_animationType.intValue)
            {
                case (int)TweenAnimationType.Color:
                    if (_useFromAsStart.boolValue)
                    {
                        EditorGUILayout.PropertyField(_fromColor);
                    }
                    EditorGUILayout.PropertyField(_toColor);
                    break;
                default:
                    if (_useFromAsStart.boolValue)
                    {
                        EditorGUILayout.PropertyField(_from);
                    }
                    EditorGUILayout.PropertyField(_to);
                    break;
            }


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Undo Save Tween Settings");
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}