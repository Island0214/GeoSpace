using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpinAuxiliary : Auxiliary
{
    public VertexUnit[] vertices;
    public FaceRefer face;

    public SpinAuxiliary() : base()
    {
    }
    public override void InitWithGeometry(Geometry geometry)
    {
        units = new VertexUnit[] { };

        GeoVertex[] geoVertices = geometry.GeoVertices();
        vertices = new VertexUnit[geoVertices.Length];
        int[] ids = new int[geoVertices.Length];
        for (int i = 0; i < geoVertices.Length; i++)
        {
            vertices[i] = geoVertices[i].VertexUnit();
            ids[i] = vertices[i].id;
        }
        face = new FaceRefer(ids);

        elements = new GeoElement[] { };

        dependencies.AddRange(units);
    }
}

public class SpinAuxiliaryTool : AuxiliaryTool
{
    private GeometryBehaviour geometryBehaviour;

    public override FormInput FormInput()
    {
        return null;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        return true;
    }

    public override Auxiliary GenerateAuxiliary(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        ResolvedBody resolvedBody;
        if (geometry is ResolvedBody)
            resolvedBody = (ResolvedBody)geometry;
        else
            return null;

        SpinAuxiliary auxiliary = new SpinAuxiliary();
        auxiliary.InitWithGeometry(geometry);
        geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();
        VertexUnit[] vertexUnits = auxiliary.vertices;
        // Cylinder
        if (vertexUnits.Length == 4)
        {
            VertexUnit vertex1 = vertexUnits[0];
            VertexUnit vertex2 = vertexUnits[1];
            VertexUnit vertex3 = vertexUnits[2];
            VertexUnit vertex4 = vertexUnits[3];
            float radius = vertexUnits[2].Position().z;

            //RectangleSpinCartoon
            RectangleSpinCartoon(vertexUnits);

            GeoCircular circular = new GeoCircular(new VertexUnit[] { vertex1, vertex2 }, radius, CircularType.Cylinder);
            geometry.AddGeoCircular(circular);
            geometry.AddGeoCircle(new GeoCircle(vertex1, radius));
            geometry.AddGeoCircle(new GeoCircle(vertex2, radius));
            resolvedBody.isSpinned = true;
        }
        // Cone
        else if (vertexUnits.Length == 3)
        {
        
            VertexUnit vertex1 = vertexUnits[0];
            VertexUnit vertex2 = vertexUnits[1];
            VertexUnit vertex3 = vertexUnits[2];
            float radius = vertexUnits[2].Position().z;

            //TriangleSpinCartoon
            TriangleSpinCartoon(vertexUnits);
            
            GeoCircular circular = new GeoCircular(new VertexUnit[] { vertex1, vertex2, vertex3 }, radius, CircularType.Cone);
            geometry.AddGeoCircular(circular);
            geometry.AddGeoCircle(new GeoCircle(vertex2, radius));
            resolvedBody.isSpinned = true;
        }
        geometryBehaviour.InitGeometry(geometry);
        return auxiliary;
    }

    public void RectangleSpinCartoon(VertexUnit[] vertexUnits){
            VertexUnit vertex1 = vertexUnits[0];
            VertexUnit vertex2 = vertexUnits[1];
            VertexUnit vertex3 = vertexUnits[2];
            VertexUnit vertex4 = vertexUnits[3];

            GameObject rectangle = new GameObject("Rectangle");
            MeshFilter filter = rectangle.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            filter.sharedMesh = mesh;
            mesh.vertices = new Vector3[6] {vertex2.Position(),vertex3.Position(),vertex1.Position(),vertex4.Position(),vertex1.Position(),vertex3.Position()};
            mesh.colors = new Color[6] { Color.gray, Color.gray, Color.gray, Color.gray, Color.gray, Color.gray};
            mesh.triangles = new int[6] { 0, 1, 2, 3, 4, 5};
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        
            MeshRenderer render = rectangle.AddComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Diffuse"));
            material.SetColor("_Color", Color.gray);
            render.sharedMaterial = material;

            rectangle.AddComponent<ObjectSpin>();
    }

    public void TriangleSpinCartoon(VertexUnit[] vertexUnits){
            VertexUnit vertex1 = vertexUnits[0];
            VertexUnit vertex2 = vertexUnits[1];
            VertexUnit vertex3 = vertexUnits[2];

            GameObject triangle = new GameObject("Triangle");
            MeshFilter filter = triangle.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            filter.sharedMesh = mesh;
            mesh.vertices = new Vector3[3] { vertex1.Position(),vertex2.Position(),vertex3.Position()};
            mesh.triangles = new int[3] { 0, 1, 2 };
            mesh.colors = new Color[3] { Color.gray, Color.gray, Color.gray };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            MeshRenderer render = triangle.AddComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Diffuse"));
            material.SetColor("_Color", Color.gray);
            render.sharedMaterial = material;
            triangle.AddComponent<ObjectSpin>();
    }
}

public class SpinAuxiliaryState : AuxiliaryState
{
    new SpinAuxiliary auxiliary;
    Geometry geometry;

    public SpinAuxiliaryState(Tool tool, Auxiliary auxiliary, Geometry geometry) : base(tool, auxiliary)
    {
        if (auxiliary is SpinAuxiliary)
            this.auxiliary = (SpinAuxiliary)auxiliary;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        return auxiliary.face.ids;
    }

    public override FormInput Title()
    {
        FormInput formInput = new FormInput(1);

        formInput.inputs[0] = new FormText("旋转");

        return formInput;
    }


}