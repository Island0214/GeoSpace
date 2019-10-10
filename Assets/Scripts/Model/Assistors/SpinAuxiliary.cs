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
        return auxiliary;
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