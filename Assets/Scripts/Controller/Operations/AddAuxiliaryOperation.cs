using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddAuxiliaryOperation : Operation
{
    GeoController geoController;
    StateController stateController;

    Geometry geometry;
    GeometryBehaviour geometryBehaviour;
    InputPanel inputPanel;
    Tool tool;

    AuxiliaryTool auxiliaryTool;


    public AddAuxiliaryOperation(GeoController geoController, StateController stateController, Geometry geometry, GeometryBehaviour geometryBehaviour, GeoUI geoUI, Tool tool)
    {
        CanRotateCamera = true;
        CanActiveElement = true;

        this.geoController = geoController;
        this.stateController = stateController;
        this.geometry = geometry;
        this.geometryBehaviour = geometryBehaviour;
        this.inputPanel = geoUI.inputPanel;
        this.tool = tool;

        Type type = Type.GetType(tool.Name + "AuxiliaryTool");
        if (type != null)
            auxiliaryTool = (AuxiliaryTool)Activator.CreateInstance(type);
    }

    public override void Start()
    {
        if (auxiliaryTool == null)
        {
            Debug.LogWarning(tool.Name + " Error!");
            geoController.EndOperation();
            return;
        }
        FormInput formInput = auxiliaryTool.FormInput();

        if (formInput != null)
        {
            inputPanel.SetFormForInput(formInput);

            inputPanel.OnValidate = (form) =>
            {
                return auxiliaryTool.ValidateInput(geometry, form);
            };

            inputPanel.OnClickSubmit = (form) =>
            {
                addAuxiliary(geometry, form);
            };

            inputPanel.OnClickCancel = (form) =>
            {
                geoController.EndOperation();
            };
        }
        else
        {
            addAuxiliary(geometry, null);
        }

    }

    public void addAuxiliary(Geometry geometry, FormInput form)
    {
        Auxiliary auxiliary = auxiliaryTool.GenerateAuxiliary(geometry, form);
        if (auxiliary == null) {
            geoController.EndOperation();
            return;
        }
        auxiliary.InitWithGeometry(geometry);

        VertexUnit[] units = auxiliary.units;
        GeoElement[] elements = auxiliary.elements;

        bool result = geometry.Assistor.AddAuxiliary(auxiliary);

        if (result)
        {
            foreach (VertexUnit unit in units)
                geometry.AddVertexUnit(unit);

            foreach (GeoElement element in elements)
                geometry.AddElement(element);

            AddState(auxiliary);

            geometryBehaviour.UpdateElements();
            foreach (GeoElement element in elements)
                geometryBehaviour.AddElement(element);

            geometryBehaviour.UpdateSignsPosition();
            foreach (VertexUnit unit in units)
                geometryBehaviour.AddSign(unit.id);


            Gizmo[] gizmos = auxiliary.gizmos;
            if (gizmos != null)
            {
                foreach (Gizmo gizmo in gizmos)
                {
                    geometry.AddGizmo(gizmo);
                    geometryBehaviour.AddGizmo(gizmo);
                }
            }

            geometryBehaviour.UpdateGeometryShade();

        }
        else
        {
            // TODO
        }

        geoController.EndOperation();
    }

    public override void End()
    {
        inputPanel.Clear();
    }

    public override void OnClickElement(GeoElement element)
    {
        FormElement form = null;
        if (element is GeoVertex)
            form = geoController.VertexForm((GeoVertex)element);
        else if (element is GeoEdge)
            form = geoController.EdgeForm((GeoEdge)element);
        else if (element is GeoFace)
            form = geoController.FaceForm((GeoFace)element);
        if (form != null)
            inputPanel.InputFields(form.fields);
    }

    private void AddState(Auxiliary auxiliary)
    {
        Type type = Type.GetType(tool.Name + "AuxiliaryState");
        if (type != null)
        {
            AuxiliaryState auxiliaryState = (AuxiliaryState)Activator.CreateInstance(type, tool, auxiliary, geometry);
            auxiliaryState.OnClickDelete = () => geoController.RemoveAuxiliaryOperation(auxiliary);

            stateController.AddAuxiliaryState(auxiliaryState);
        }
    }

}