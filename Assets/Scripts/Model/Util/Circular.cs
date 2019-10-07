using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circular 
{
    // Cylinder has 2 Vertices and Cone has 3 Vertices.
    public Vector3[] Vertices;
    public float radius;
    public CircularType type;

    public Circular(Vector3[] vs, float radius, CircularType type)
    {
        this.Vertices = vs;
        this.radius = radius;
        this.type = type;
    }
}
