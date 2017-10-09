using UnityEngine;
using UnityEditor;

namespace LightBallChain
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ChainRenderer))]
    public class ChainRendererEditor : Editor
    {
        SerializedProperty _motionType;
        SerializedProperty _color;
        SerializedProperty _radius;
        SerializedProperty _ballScale;
        SerializedProperty _instanceCount;
        SerializedProperty _frequency;
        SerializedProperty _interval;

        void OnEnable()
        {
            _motionType = serializedObject.FindProperty("_motionType");
            _color = serializedObject.FindProperty("_color");
            _radius = serializedObject.FindProperty("_radius");
            _ballScale = serializedObject.FindProperty("_ballScale");
            _instanceCount = serializedObject.FindProperty("_instanceCount");
            _frequency = serializedObject.FindProperty("_frequency");
            _interval = serializedObject.FindProperty("_interval");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_motionType);
            EditorGUILayout.PropertyField(_color);
            EditorGUILayout.PropertyField(_radius);
            EditorGUILayout.PropertyField(_ballScale);
            EditorGUILayout.PropertyField(_instanceCount);
            EditorGUILayout.PropertyField(_frequency);
            EditorGUILayout.PropertyField(_interval);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
