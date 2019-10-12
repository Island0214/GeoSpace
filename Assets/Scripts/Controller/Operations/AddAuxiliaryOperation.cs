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

    GeoUI geoUI;


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
        this.geoUI = geoUI;
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

        inputPanel.SetFormForInput(auxiliaryTool.FormInput());

        inputPanel.OnValidate = (form) =>
        {
            return auxiliaryTool.ValidateInput(geometry, form);
        };

        inputPanel.OnClickSubmit = (form) =>
        {
            Auxiliary auxiliary = auxiliaryTool.GenerateAuxiliary(geometry, form);
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
        };

        inputPanel.OnClickCancel = (form) =>
        {
            geoController.EndOperation();
        };

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

            //Action OnElementHighLight实现  0925需求一
            auxiliaryState.OnElementHighlight = () =>   
            {
            //改变颜色
             geometry.SetElementColor(auxiliary.elements[0], 1);  
             geometryBehaviour.GeometryElementColorChange(auxiliary.elements[0], 1);
            
            //隐藏坐标轴
            geoUI.navPanel.OnCoordinateButtonClick(1);
            geoUI.navPanel.SetCoordinateButtonStatus(1); //改变BUTTON状态

            //隐藏grid
            geoUI.navPanel.OnGridButtonClick(1);
            geoUI.navPanel.SetGridButtonStatus(1);  

            
            //显示各边长
            if(auxiliary.elements[0] is GeoFace) 
            {
                GeoFace geoFace = (GeoFace)auxiliary.elements[0];
                for(int i = 0;i < geoFace.Ids.Length; i++)
                {

                int vertex1 = i;
                int vertex2 = (i+1) % geoFace.Ids.Length;

                LineLengthMeasure measure = new LineLengthMeasure(geoFace.Ids[vertex1], geoFace.Ids[vertex2]);

                measure.InitWithGeometry(geometry);
                bool result = geometry.Implement.AddMeasure(measure);
                if (result)
                {
                    List<ToolGroup> toolGroups = geoUI.toolPanel.ToolGroups();
                    Tool lineLengthMeasureTool = toolGroups[3].Tools[0];
                    AddState_Measure(measure,lineLengthMeasureTool);

                    Gizmo[] gizmos = measure.gizmos;
                    if (gizmos != null)
                    {
                        foreach (Gizmo gizmo in gizmos)
                        {
                            geometry.AddGizmo(gizmo);
                            geometryBehaviour.AddGizmo(gizmo);
                        }
                    }
                }
                else
                {
                    // TODO
                }

                }
            }
            //显示各角度
            if(auxiliary.elements[0] is GeoFace) 
            {
                GeoFace geoFace = (GeoFace)auxiliary.elements[0];
                for(int i = 0;i < geoFace.Ids.Length; i++) 
                {
                int vertex1 = i;
                int vertex2 = (i+1)%geoFace.Ids.Length;
                int vertex3 = (i+2)%geoFace.Ids.Length;

                CornerAngleMeasure measure = new CornerAngleMeasure(geoFace.Ids[vertex1], geoFace.Ids[vertex2], geoFace.Ids[vertex3]);

                measure.InitWithGeometry(geometry);


                bool result = geometry.Implement.AddMeasure(measure);

                if (result)
                {
                    List<ToolGroup> toolGroups = geoUI.toolPanel.ToolGroups();
                    Tool cornerAngleMeasureTool = toolGroups[3].Tools[1];
                    AddState_Measure(measure,cornerAngleMeasureTool);

                    Gizmo[] gizmos = measure.gizmos;
                    if (gizmos != null)
                    {
                        foreach (Gizmo gizmo in gizmos)
                        {
                            geometry.AddGizmo(gizmo);
                            geometryBehaviour.AddGizmo(gizmo);
                        }
                    }
                }
                else
                {
                    // TODO
                } 
                }  
            }        
            //显示面积
            if(auxiliary.elements[0] is GeoFace) 
            {
                GeoFace geoFace = (GeoFace)auxiliary.elements[0];

                Measure measure = new PlaneAreaMeasure(geoFace.Ids);
                measure.InitWithGeometry(geometry);


                bool result = geometry.Implement.AddMeasure(measure);

                if (result)
                {
                    List<ToolGroup> toolGroups = geoUI.toolPanel.ToolGroups();
                    Tool planeAreaMeasureTool = toolGroups[3].Tools[2];
                    AddState_Measure(measure,planeAreaMeasureTool);

                    Gizmo[] gizmos = measure.gizmos;
                    if (gizmos != null)
                    {
                        foreach (Gizmo gizmo in gizmos)
                        {
                            geometry.AddGizmo(gizmo);
                            geometryBehaviour.AddGizmo(gizmo);
                        }
                    }
                }
                else
                {
                    // TODO
                }
            }
            };

            stateController.AddAuxiliaryState(auxiliaryState);
        }
    }

        private void AddState_Measure(Measure measure,Tool tool)
    {
        Type type = Type.GetType(tool.Name + "MeasureState");
        if (type != null)
        {
            MeasureState measureState = (MeasureState)Activator.CreateInstance(type, tool, measure, geometry);
            measureState.OnClickDelete = () => geoController.RemoveMeasureOperation(measure);

            stateController.AddMeasureState(measureState);
        }
    }

}