using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddAuxiliaryOperation : Operation
{
    GeoController geoController;
    StateController stateController;

    GeoCamera geoCamera;

    Geometry geometry;
    GeometryBehaviour geometryBehaviour;
    InputPanel inputPanel;
    Tool tool;

    AuxiliaryTool auxiliaryTool;


    public AddAuxiliaryOperation(GeoController geoController,GeoCamera geoCamera, StateController stateController, Geometry geometry, GeometryBehaviour geometryBehaviour, GeoUI geoUI, Tool tool)
    {
        CanRotateCamera = true;
        CanActiveElement = true;

        this.geoController = geoController;

        this.geoCamera = geoCamera;

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

            AddState(auxiliary,form);

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

    private void AddState(Auxiliary auxiliary,FormInput form)
    {
        Type type = Type.GetType(tool.Name + "AuxiliaryState");
        if (type != null)
        {
            AuxiliaryState auxiliaryState = (AuxiliaryState)Activator.CreateInstance(type, tool, auxiliary, geometry);
            auxiliaryState.OnClickDelete = () => geoController.RemoveAuxiliaryOperation(auxiliary);

            //state单击
            auxiliaryState.DoubleClick = () => this.TurnToFront(auxiliary,form);
            //auxiliaryState.DoubleClick = () => geoCamera.TriggerRotateAnimation(45,10); //动画旋转角度到某个位置

            stateController.AddAuxiliaryState(auxiliaryState);
        }
    }

    public void TurnToFront(Auxiliary auxiliary,FormInput form) {
        Debug.Log(form.inputs[1]);// 辅助面名
        //String faceName = form.inputs[1].ToString();
        //Debug.Log(faceName);
        //Debug.Log(auxiliary.elements[0]);//  face 7 5 0
        //Debug.Log(auxiliary.elements[0].name);//  Face 
        //Debug.Log(auxiliary.dependencies.ToArray()[0].Position());//辅助面第一个点的坐标

        Vector3 rotateAngle = new Vector3(0, 0, 0);
        if (auxiliary.elements[0].name == "Face")
        {
            rotateAngle = GetRotateAngle(auxiliary);
            Debug.Log("change");
        }

        //float rotateX = rotateAngle.x;
        //float rotateY = rotateAngle.y;
        //float rotateZ = rotateAngle.z;
        Debug.Log("rotateX:" + rotateAngle.x);
        Debug.Log("rotateY:" + rotateAngle.y);
        //Debug.Log("rotateZ:" + rotateZ);
        //geoCamera.SetCameraAttributes(rotateY, -90f - rotateX, 0f);  //直接跳转镜头
         //geoCamera.SetCameraAttributes(45f, -90f-10f, 0f);//y ,x,z  向上45度，向右10度
        geoCamera.TriggerRotateAnimation(rotateAngle.x,rotateAngle.y);  //动画旋转镜头
     
    }

    public Vector3 GetRotateAngle(Auxiliary auxiliary)
    {
        VertexUnit[] units = auxiliary.dependencies.ToArray();
        //平面三个不共线点
        Vector3 A = new Vector3();
        Vector3 B = new Vector3();
        Vector3 C = new Vector3();
        //if (auxiliary is PlaneAuxiliary)
        //{
        GeoElement[] elements = auxiliary.elements;
        Vector3[] vertexs = new Vector3[units.Length];
        for (int i = 0; i < units.Length; i++)
        {
            vertexs[i] = units[i].Position();
        }
        for (int i = 0; i < units.Length - 2; i++)
        {
            A = vertexs[i];
            B = vertexs[i + 1];
            C = vertexs[i + 2];
            if (((B.x - A.x) / (C.x - A.x) == (B.y - A.y) / (C.y - A.y)) && ((B.x - A.x) / (C.x - A.x) == (B.z - A.z) / (C.z - A.z)))
            {
                continue;
            }
            else
            {
                break;
            }
        }

        //求平面法线
        Vector3 normalVector;  //平面法向量
        Vector3 AB = new Vector3(B.x - A.x, B.y - A.y, B.z - A.z);
        Vector3 AC = new Vector3(C.x - A.x, C.y - A.y, C.z - A.z);
        normalVector.x = AB.y * AC.z - AC.y * AB.z;
        normalVector.y = AB.z * AC.x - AB.x * AC.z;
        normalVector.z = AB.x * AC.y - AB.y * AC.x;
        if (normalVector.x < 0) {
            normalVector.x = -normalVector.x;
            normalVector.y = -normalVector.y;
            normalVector.z = -normalVector.z;
        }
        if (normalVector.x == 0) {
            if (normalVector.z < 0) {
                normalVector.y = -normalVector.y;
                normalVector.z = -normalVector.z;
            }
        }
        float rotateX = Vector3.Angle(new Vector3(1, 0, 0), new Vector3(normalVector.x,0f,normalVector.z));
        float rotateY = 90f - Vector3.Angle(new Vector3(0, 1, 0), normalVector);
        //float rotateZ = 90f - Vector3.Angle(new Vector3(0, 0, 1), normalVector);
        //float rotateZ = 0f;
        //if (normalVector.z < 0) {
          //  rotateX = -rotateX;
        //}
        
        //return new Vector3(rotateX, rotateY, rotateZ);
        return new Vector3(-rotateX-90,rotateY,0f);

    }

}