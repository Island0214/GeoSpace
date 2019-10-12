using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryOperation : Operation
{
    GeoController geoController;
    ToolController toolController;
    StateController stateController;

    Tool tool;

    GeometryTool geometryTool;

    Geometry geometry;

    GeometryBehaviour geometryBehaviour;
    public GeometryOperation(GeoController geoController, ToolController toolController, StateController stateController, Tool tool,GeometryBehaviour geometryBehaviour)
    {
        this.geoController = geoController;
        this.toolController = toolController;
        this.stateController = stateController;

        this.tool = tool;
        this.geometryBehaviour = geometryBehaviour;

        Type type = Type.GetType(tool.Name + "GeometryTool");
        if (type != null)
            geometryTool = (GeometryTool)Activator.CreateInstance(type);
    }

    public override void Start()
    {
        if (geometryTool == null)
        {
            Debug.LogWarning(tool.Name + " Error!");
            geoController.EndOperation();
            return;
        }

        geometry = geometryTool.GenerateGeometry();
        geoController.SetGeometry(geometry);

        AddState(geometry);

        geoController.EndOperation();
    }

    public override void End()
    {

    }

    private void AddState(Geometry geometry)
    {
        Type type = Type.GetType(tool.Name + "GeometryState");
        if (type != null)
        {
            GeometryState geometryState = (GeometryState)Activator.CreateInstance(type, tool, geometry);
            geometryState.OnClickDelete = () => geoController.ClearGeometryOperation(geometry);

            stateController.AddGeometryState(geometryState);
        }
    }
}
