using UnityEngine;
using UnityEditor;

namespace LightBallChain
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ChainRenderer))]
    public class ChainRendererEditor : Editor
    {
        SerializedProperty _motionType;
        SerializedProperty _frequency;
        SerializedProperty _interval;

        SerializedProperty _radius;
        SerializedProperty _ballCount;
        SerializedProperty _ballScale;
        SerializedProperty _color;

        void OnEnable()
        {
            _motionType = serializedObject.FindProperty("_motionType");
            _frequency = serializedObject.FindProperty("_frequency");
            _interval = serializedObject.FindProperty("_interval");

            _radius = serializedObject.FindProperty("_radius");
            _ballCount = serializedObject.FindProperty("_ballCount");
            _ballScale = serializedObject.FindProperty("_ballScale");
            _color = serializedObject.FindProperty("_color");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_motionType);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_frequency);
            EditorGUILayout.PropertyField(_interval);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(_radius);
            EditorGUILayout.PropertyField(_ballCount);
            EditorGUILayout.PropertyField(_ballScale);
            EditorGUILayout.PropertyField(_color);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
