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

        SpinAuxiliary auxiliary = new SpinAuxiliary();
        auxiliary.InitWithGeometry(geometry);
        GeoCamera geoCamera = GameObject.Find("/3D/GeoCamera").GetComponent<GeoCamera>();
        geoCamera.TriggerCenterRAnimation();
        VertexUnit[] vertexUnits = auxiliary.vertices;
        if (vertexUnits.Length != 3 && vertexUnits.Length != 4)
            return null;
        
        SpinCartoon(vertexUnits, geometry);
        return auxiliary;
    }

    public void GenerateResolvedBody(Geometry geometry)
    {
        ResolvedBody resolvedBody;
        if (geometry is ResolvedBody)
            resolvedBody = (ResolvedBody)geometry;
        else
            return;

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

            GeoCircular circular = new GeoCircular(new VertexUnit[] { vertex1, vertex2, vertex3 }, radius, CircularType.Cone);
            geometry.AddGeoCircular(circular);
            geometry.AddGeoCircle(new GeoCircle(vertex2, radius));
            resolvedBody.isSpinned = true;
        }
        geometryBehaviour.InitGeometry(geometry);
    }

    public void SpinCartoon(VertexUnit[] vertexUnits, Geometry geometry)
    {
        VertexUnit vertex1 = vertexUnits[0];
        VertexUnit vertex2 = vertexUnits[1];
        VertexUnit vertex3 = vertexUnits[2];

        GameObject rectangle = new GameObject("CartoonFace");
        MeshFilter filter = rectangle.AddComponent<MeshFilter>();
        MeshCollider meshCollider = rectangle.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();
        Mesh colliderMesh = new Mesh();
        meshCollider.sharedMesh = colliderMesh;
        filter.sharedMesh = mesh;
        // triangle
        if (vertexUnits.Length == 3) {
            mesh.vertices = new Vector3[3] { vertex1.Position(), vertex2.Position(), vertex3.Position() };
            mesh.triangles = new int[3] { 0, 1, 2 };
        }
        // rectangle
        else if (vertexUnits.Length == 4)
        {
            VertexUnit vertex4 = vertexUnits[3];
            mesh.vertices = new Vector3[6] { vertex1.Position(), vertex2.Position(), vertex3.Position(), vertex1.Position(), vertex3.Position(), vertex4.Position() };
            mesh.triangles = new int[6] { 0, 1, 2, 3, 4, 5 };
        }
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // uv
        Vector3 y = mesh.normals[0].normalized;
        Vector3 x = (mesh.vertices[1] - mesh.vertices[0]).normalized;
        Vector3 z = Vector3.Cross(x, y);

        Matrix4x4 matrix = new Matrix4x4(
            new Vector4(x.x, y.x, z.x, 0),
            new Vector4(x.y, y.y, z.y, 0),
            new Vector4(x.z, y.z, z.z, 0),
             new Vector4(0, 0, 0, 1));

        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 position = matrix.MultiplyPoint(mesh.vertices[i]);
            uvs[i] = new Vector2(position.x, position.z);
        }
        mesh.uv = uvs;

        MeshRenderer render = rectangle.AddComponent<MeshRenderer>();
        StyleManager.SetPlaneProperty(render, 0);
        render.sharedMaterial = ConfigManager.FaceStyle[0].Material;

        rectangle.AddComponent<ObjectSpin>().GetData(geometry);
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