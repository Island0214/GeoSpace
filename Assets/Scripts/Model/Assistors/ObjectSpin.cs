using UnityEngine;
using System;
using System.Collections;

public class ObjectSpin : MonoBehaviour{
	
	float RotateSpeed = 60f;
	float PreAngle = 0;
	float Radius;
	Geometry geometry;
	VertexUnit[] vertices;
	GeometryBehaviour geometryBehaviour;


	void Update(){
		transform.Rotate(Vector3.up * RotateSpeed * Time.deltaTime);
		AddCurFace(transform.localEulerAngles.y);
		if(transform.localEulerAngles.y > 350){
			DestroyGameObject();
		}
	}

	private void AddCurFace(float Angle) {
		float X = Radius * Mathf.Sin(Mathf.Deg2Rad * Angle);
		float Z = Radius * Mathf.Cos(Mathf.Deg2Rad * Angle);

		float preX = Radius * Mathf.Sin(Mathf.Deg2Rad * PreAngle);
		float preZ = Radius * Mathf.Cos(Mathf.Deg2Rad * PreAngle);
		if (vertices.Length == 3)
		{
			VertexSpace v1 = new VertexSpace(vertices[0].Position());
			VertexSpace v2 = new VertexSpace(vertices[1].Position());
			VertexSpace v3 = new VertexSpace(X, vertices[1].Position().y, Z);
			VertexSpace v4 = new VertexSpace(preX, vertices[1].Position().y, preZ);
			geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] {v1, v3, v4}));
			geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] {v2, v3, v4}));
			geometryBehaviour.AddElement(new GeoEdge(v3, v4));
		}
		else if (vertices.Length == 4)
		{
			VertexSpace v1 = new VertexSpace(vertices[0].Position());
			VertexSpace v2 = new VertexSpace(X, vertices[0].Position().y, Z);
			VertexSpace v3 = new VertexSpace(preX, vertices[0].Position().y, preZ);
			VertexSpace v4 = new VertexSpace(vertices[1].Position());
			VertexSpace v5 = new VertexSpace(X, vertices[1].Position().y, Z);
			VertexSpace v6 = new VertexSpace(preX, vertices[1].Position().y, preZ);
			geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] {v1, v2, v3}));
			geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] {v4, v5, v6}));
			geometryBehaviour.AddElement(new GeoFace(new VertexUnit[] {v2, v5, v6, v3}));
			geometryBehaviour.AddElement(new GeoEdge(v2, v3));
			geometryBehaviour.AddElement(new GeoEdge(v5, v6));
		}
		PreAngle = Angle;
	}

	public void GetData(Geometry geometry) {
		this.geometry = geometry;

		GeoVertex[] geoVertices = geometry.GeoVertices();
        vertices = new VertexUnit[geoVertices.Length];
        for (int i = 0; i < geoVertices.Length; i++)
        {
            vertices[i] = geoVertices[i].VertexUnit();
        }
		Radius = vertices[2].Position().z - vertices[1].Position().z;

	    geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();	
	}

	void DestroyGameObject()
    {
		Type type = Type.GetType("SpinAuxiliaryTool");
		SpinAuxiliaryTool auxiliaryTool;
        if (type != null) {
            auxiliaryTool = (SpinAuxiliaryTool)Activator.CreateInstance(type);
			auxiliaryTool.GenerateResolvedBody(geometry);
		}
        Destroy(gameObject);
    }
}