using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circular 
{
    // Cylinder has 4 Vertices and Cone has 3 Vertices.
    public Vector3[] Vertices;
    public float radius1;
    public float radius2;
    public CircularType type;

    public Circular(Vector3[] vs, float radius1, float radius2, CircularType type)
    {
        this.Vertices = vs;
        this.radius1 = radius1;
        this.radius2 = radius2;
        this.type = type;
    }
}
