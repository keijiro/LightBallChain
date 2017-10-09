using UnityEngine;
using UnityEditor;

namespace LightBallChain
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ChainRenderer))]
    public class ChainRendererEditor : Editor
    {
        SerializedProperty _motionType;
        SerializedProperty _speed;
        SerializedProperty _interval;
        SerializedProperty _multiplier;
        SerializedProperty _randomSeed;

        SerializedProperty _radius;
        SerializedProperty _ballCount;
        SerializedProperty _ballScale;
        SerializedProperty _color;

        void OnEnable()
        {
            _motionType = serializedObject.FindProperty("_motionType");
            _speed = serializedObject.FindProperty("_speed");
            _interval = serializedObject.FindProperty("_interval");
            _multiplier = serializedObject.FindProperty("_multiplier");
            _randomSeed = serializedObject.FindProperty("_randomSeed");

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
            EditorGUILayout.PropertyField(_speed);
            EditorGUILayout.PropertyField(_interval);
            EditorGUILayout.PropertyField(_multiplier);
            EditorGUILayout.PropertyField(_randomSeed);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(_radius);
            EditorGUILayout.PropertyField(_ballCount);
            EditorGUILayout.PropertyField(_ballScale);
            EditorGUILayout.PropertyField(_color);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
