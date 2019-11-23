using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpin : MonoBehaviour
{

    float RotateSpeed = 120f;
    float PreAngle = 0;
    float Radius1;
    float Radius2;
    Geometry geometry;
    VertexUnit[] vertices;
    GeometryBehaviour geometryBehaviour;
    List<GeoEdge> geoEdges;

    void Update()
    {
        foreach (GeoEdge edge in geoEdges)
        {
            if (geometryBehaviour.ContainsEdge(edge))
            {
                geometryBehaviour.RemoveElement(edge);
            }
        }
        geoEdges.Clear();
        transform.Rotate(Vector3.up * RotateSpeed * Time.deltaTime);
        AddCurFace(transform.localEulerAngles.y);
        if (transform.localEulerAngles.y >= 360 - RotateSpeed / 10)
        {
            DestroyGameObject();
        }
    }

    private void AddCurFace(float Angle)
    {
        float X1 = Radius1 * Mathf.Sin(Mathf.Deg2Rad * Angle);
        float Z1 = Radius1 * Mathf.Cos(Mathf.Deg2Rad * Angle);
        float preX1 = Radius1 * Mathf.Sin(Mathf.Deg2Rad * PreAngle);
        float preZ1 = Radius1 * Mathf.Cos(Mathf.Deg2Rad * PreAngle);

        float X2 = Radius2 * Mathf.Sin(Mathf.Deg2Rad * Angle);
        float Z2 = Radius2 * Mathf.Cos(Mathf.Deg2Rad * Angle);
        float preX2 = Radius2 * Mathf.Sin(Mathf.Deg2Rad * PreAngle);
        float preZ2 = Radius2 * Mathf.Cos(Mathf.Deg2Rad * PreAngle);
        if (vertices.Length == 3)
        {
            VertexSpace v1 = new VertexSpace(vertices[0].Position());
            VertexSpace v2 = new VertexSpace(vertices[1].Position());
            VertexSpace v3 = new VertexSpace(X1, vertices[2].Position().y, Z1);
            VertexSpace v4 = new VertexSpace(preX1, vertices[2].Position().y, preZ1);
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v1, v3, v4 }));
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v2, v3, v4 }));
            geometryBehaviour.AddElement(new GeoEdge(v3, v4));
			addBorderLine(v1.Position(), v3.Position());
			addBorderLine(v2.Position(), v3.Position());
        }
        else if (vertices.Length == 4)
        {
            VertexSpace v1 = new VertexSpace(vertices[0].Position());
            VertexSpace v2 = new VertexSpace(X2, vertices[3].Position().y, Z2);
            VertexSpace v3 = new VertexSpace(preX2, vertices[3].Position().y, preZ2);
            VertexSpace v4 = new VertexSpace(vertices[1].Position());
            VertexSpace v5 = new VertexSpace(X1, vertices[2].Position().y, Z1);
            VertexSpace v6 = new VertexSpace(preX1, vertices[2].Position().y, preZ1);
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v1, v2, v3 }));
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v4, v5, v6 }));
            geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] { v2, v5, v6, v3 }));
            geometryBehaviour.AddElement(new GeoEdge(v2, v3));
            geometryBehaviour.AddElement(new GeoEdge(v5, v6));
			addBorderLine(v1.Position(), v2.Position());
			addBorderLine(v2.Position(), v5.Position());
			addBorderLine(v4.Position(), v5.Position());
        }
        PreAngle = Angle;
    }

    private void addBorderLine(Vector3 vertice1, Vector3 vertice2)
    {
        GeoEdge geoEdge = new GeoEdge(new VertexSpace(vertice1), new VertexSpace(vertice2));
        geoEdges.Add(geoEdge);
        geometryBehaviour.AddElement(geoEdge);
    }

    public void GetData(Geometry geometry)
    {
        this.geometry = geometry;

        GeoVertex[] geoVertices = geometry.GeoVertices();
        vertices = new VertexUnit[geoVertices.Length];
        for (int i = 0; i < geoVertices.Length; i++)
        {
            vertices[i] = geoVertices[i].VertexUnit();
        }
        Radius1 = vertices[2].Position().z - vertices[1].Position().z;
        if (vertices.Length == 4)
        {
            Radius2 = vertices[3].Position().z - vertices[0].Position().z;
        } else
        {
            Radius2 = Radius1;
        }
        geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();

        geoEdges = new List<GeoEdge>();
    }

    void DestroyGameObject()
    {
        Type type = Type.GetType("SpinAuxiliaryTool");
        SpinAuxiliaryTool auxiliaryTool;
        if (type != null)
        {
            auxiliaryTool = (SpinAuxiliaryTool)Activator.CreateInstance(type);
            auxiliaryTool.GenerateResolvedBody(geometry);
        }
        Destroy(gameObject);
    }
}