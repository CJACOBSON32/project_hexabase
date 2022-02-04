using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimpleGizmo))]
public class SimpleGizmoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SimpleGizmo simpleGizmoScript = (SimpleGizmo)target;

        base.OnInspectorGUI();

        if (simpleGizmoScript.shape == SimpleGizmo.Shape.sphere)
            simpleGizmoScript.sphereRadius = EditorGUILayout.FloatField("Radius", simpleGizmoScript.sphereRadius);
        else if (simpleGizmoScript.shape == SimpleGizmo.Shape.cube)
            simpleGizmoScript.cubeDimensions = EditorGUILayout.Vector3Field("Cube Dimensions", simpleGizmoScript.cubeDimensions);
    }
}
