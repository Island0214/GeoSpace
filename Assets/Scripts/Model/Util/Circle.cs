using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CircleDirection
{
    X,
    Y,
    Z,
}
public class Circle 
{
    public Vector3 Vertice;
    public float radius;
    public CircleDirection direction;
    public bool displayFace;

    public Circle(Vector3 vs, float radius, CircleDirection direction = CircleDirection.Y, bool displayFace = false)
    {
        this.Vertice = vs;
        this.radius = radius;
        this.direction = direction;
        this.displayFace = displayFace;
    }
}
