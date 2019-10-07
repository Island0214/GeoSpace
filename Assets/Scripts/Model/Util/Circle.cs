using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle 
{
    public Vector3 Vertice;
    public float radius;

    public Circle(Vector3 vs, float radius)
    {
        this.Vertice = vs;
        this.radius = radius;
    }
}
