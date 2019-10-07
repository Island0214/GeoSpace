using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResolvedBody : Geometry
{

    public bool isSpinned = false;
    public bool isSpread = false;

    public override void Init()
    {
        base.Init();

        Name = "ResolvedBody";
        Type = GeometryType.ResolvedBody;

    }

    public void SetRectangle(Vector2 position)
    {
        Vector3 faceNormal = Vector3.right;

        VertexFace u0 = new VertexFace(0, position.x / 2, 0, faceNormal);
        u0.isFixed = true;
        VertexFace u1 = new VertexFace(0, -position.x / 2, 0, faceNormal);
        u1.isFixed = true;
        VertexFace u2 = new VertexFace(0, -position.x / 2, position.y, faceNormal);
        u2.isFixed = true;
        VertexFace u3 = new VertexFace(0, position.x / 2, position.y, faceNormal);
        u3.isFixed = true;
        AddBaseVertex(u0);
        AddBaseVertex(u1);
        AddBaseVertex(u2);
        AddBaseVertex(u3);

        GeoVertex v0 = new GeoVertex(u0, true);
        GeoVertex v1 = new GeoVertex(u1, true);
        GeoVertex v2 = new GeoVertex(u2, true);
        GeoVertex v3 = new GeoVertex(u3, true);
        AddGeoVertex(v0);
        AddGeoVertex(v1);
        AddGeoVertex(v2);
        AddGeoVertex(v3);

        GeoEdge e0 = new GeoEdge(u0, u1, true);
        GeoEdge e1 = new GeoEdge(u1, u2, true);
        GeoEdge e2 = new GeoEdge(u2, u3, true);
        GeoEdge e3 = new GeoEdge(u0, u3, true);
        AddGeoEdge(e0);
        AddGeoEdge(e1);
        AddGeoEdge(e2);
        AddGeoEdge(e3);

        GeoFace f0 = new GeoFace(new VertexUnit[] { u0, u1, u2, u3 }, true);
        AddGeoFace(f0);

        InitDatas();
    }
	
	public void SetTriangle(Vector2 position) {
        Vector3 faceNormal = Vector3.right;

        VertexFace u0 = new VertexFace(0, position.x / 2, 0, faceNormal);
		u0.isFixed = true;
        VertexFace u1 = new VertexFace(0, -position.x / 2, 0, faceNormal);
		u1.isFixed = true;
        VertexFace u2 = new VertexFace(0, -position.x / 2, position.y, faceNormal);
		u2.isFixed = true;
        AddBaseVertex(u0);
        AddBaseVertex(u1);
        AddBaseVertex(u2);

        GeoVertex v0 = new GeoVertex(u0, true);
        GeoVertex v1 = new GeoVertex(u1, true);
        GeoVertex v2 = new GeoVertex(u2, true);
        AddGeoVertex(v0);
        AddGeoVertex(v1);
        AddGeoVertex(v2);

        GeoEdge e0 = new GeoEdge(u0, u1, true);
        GeoEdge e1 = new GeoEdge(u1, u2, true);
        GeoEdge e2 = new GeoEdge(u2, u0, true);
        AddGeoEdge(e0);
        AddGeoEdge(e1);
        AddGeoEdge(e2);

        GeoFace f0 = new GeoFace(new VertexUnit[] { u0, u1, u2 }, true);
        AddGeoFace(f0);

        InitDatas();
    }
}

public class ResolvedBodyGeometryTool : GeometryTool
{
    public override Geometry GenerateGeometry()
    {
        ResolvedBody geo = new ResolvedBody();
        geo.Constructor = new ResolvedBodyConstructor(geo);
        geo.Assistor = new Assistor(geo);
        geo.Implement = new Implement(geo);
        geo.Init();

        return geo;
    }
}

public class ResolvedBodyGeometryState : GeometryState
{
    new ResolvedBody geometry;

    public ResolvedBodyGeometryState(Tool tool, Geometry geometry) : base(tool, geometry)
    {
        if (geometry is ResolvedBody)
            this.geometry = (ResolvedBody)geometry;
    }

    public override FormInput Title()
    {
        // add state
        FormInput formInput = new FormInput(1);
        formInput.inputs[0] = new FormText("旋转体");

        // transform camera
        GameObject obj = GameObject.Find("X");
        NavAxisBehaviour axis = obj.GetComponent<NavAxisBehaviour>();
        PointerEventData data = new PointerEventData(EventSystem.current);
        axis.OnPointerClick(data);

        return formInput;
    }
}