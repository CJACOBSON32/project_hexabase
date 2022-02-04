using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AreaGizmo : MonoBehaviour
{
    [Min(0)]
    public float radius = 0.5f;

    [Min(0)]
    public float innerRadius = 0.1f;

    private Mesh circle;

    // Start is called before the first frame update
    void Awake()
    {
        circle = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Models/Circle.stl", typeof(Mesh));
    }

    public void setRadius(float radius)
    {
        if (radius < 0)
            Debug.LogError("Radius of areaGizmo in " + gameObject.ToString() + " cannot be set to a negative number.");
        else
            this.radius = Mathf.Max(radius, 0);
    }

    public void setInnerRadius(float radius)
    {
        if (radius < 0)
            Debug.LogError("Radius of areaGizmo in " + gameObject.ToString() + " cannot be set to a negative number.");
        else
            this.innerRadius = Mathf.Max(radius, 0);
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, transform.up, this.innerRadius);

        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, transform.up, this.radius);
    }
}
