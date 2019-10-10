using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face 
{
    public Vector3[] Vertices;

    public bool Canselected;

    public Face(Vector3[] vs, bool Canselected = true)
    {
        this.Vertices = vs;
        this.Canselected = Canselected;
    }
}
