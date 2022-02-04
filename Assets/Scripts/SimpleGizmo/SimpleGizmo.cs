using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGizmo : MonoBehaviour
{
    public enum Shape { cube, sphere }


    public bool solid = false;
    public Color color = Color.cyan;
    public Shape shape = Shape.sphere;

    [HideInInspector] [Min(0)]
    public float sphereRadius = 0.5f;
    [HideInInspector]
    public Vector3 cubeDimensions = new Vector3(0.5f, 0.5f, 0.5f);

    private void OnDrawGizmos()
    {
        Gizmos.color = this.color;

        if (solid)
        {
            switch(shape)
            {
                case Shape.sphere:
                    Gizmos.DrawSphere(transform.position, sphereRadius);
                    break;
                case Shape.cube:
                    Gizmos.DrawCube(transform.position, cubeDimensions);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (shape)
            {
                case Shape.sphere:
                    Gizmos.DrawWireSphere(transform.position, sphereRadius);
                    break;
                case Shape.cube:
                    Gizmos.DrawWireCube(transform.position, cubeDimensions);
                    break;
                default:
                    break;
            }
        }
    }
}