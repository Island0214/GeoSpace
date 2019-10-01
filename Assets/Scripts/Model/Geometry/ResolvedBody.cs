using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolvedBody : Geometry
{
    public override void Init()
    {
        base.Init();

        Name = "ResolvedBody";
        Type = GeometryType.ResolvedBody;
    }
}

public class ResolvedBodyGeometryTool : GeometryTool
{
    public override Geometry GenerateGeometry()
    {
        ResolvedBody geo = new ResolvedBody();
        geo.Assistor = new Assistor(geo);
        geo.Implement = new Implement(geo);
        geo.Init();

        return geo;
    }
}

public class ResolvedBodyState : GeometryState
{
    new ResolvedBody geometry;

    public ResolvedBodyState(Tool tool, Geometry geometry) : base(tool, geometry)
    {
        if (geometry is ResolvedBody)
            this.geometry = (ResolvedBody)geometry;
    }

    public override FormInput Title()
    {

        FormInput formInput = new FormInput(1);
        formInput.inputs[0] = new FormText("旋转体");

        return formInput;
    }
}