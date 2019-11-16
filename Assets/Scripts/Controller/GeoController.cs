using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public enum GeoState
{
    Normal,
    MoveVertex,
    EditVertex,
    EditEdge,
    EditFace,
    Condition,
    Auxiliary,
    Measure,
}

public class GeoUI
{
    public NavPanel navPanel;
    public ToolPanel toolPanel;
    public StatePanel statePanel;
    public InputPanel inputPanel;
    public ElementPanel elementPanel;
    public ActivePanel activePanel;
    public CameraPanel cameraPanel;
    public WritingPanel writingPanel;
}

public class GeoController : MonoBehaviour
{

    GeoUI geoUI;
    Geometry geometry;
    GeoState state;
    GeometryBehaviour geometryBehaviour;
    NavigationBehaviour navigationBehaviour;
    GridBehaviour gridBehaviour;
    CoordinateBehaviour coordinateBehaviour;

    GeoCamera geoCamera;
    NavCamera navCamera;

    StateController stateController;
    ToolController toolController;
    RecognizeController recognizeController;

    bool isCameraRotate;
    bool canCameraRotate;

    bool isSnapToGrid;
    GeometryShadeType shadeType;

    Operation currentOperation;

    void Start()
    {
        geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();
        navigationBehaviour = GameObject.Find("/3D/Navigation").GetComponent<NavigationBehaviour>();
        gridBehaviour = GameObject.Find("/3D/Grid").GetComponent<GridBehaviour>();
        coordinateBehaviour = GameObject.Find("/3D/Coordinate").GetComponent<CoordinateBehaviour>();

        geoCamera = GameObject.Find("/3D/GeoCamera").GetComponent<GeoCamera>();
        navCamera = GameObject.Find("/3D/NavCamera").GetComponent<NavCamera>();

        InitTouchSystem();
        InitView();
        InitUI();

        geoCamera.InitDefault();

        stateController = GetComponent<StateController>();
        stateController.Init(geoUI.statePanel);

        toolController = GetComponent<ToolController>();
        toolController.Init(geoUI.toolPanel);

        recognizeController = GetComponent<RecognizeController>();
        recognizeController.Init(geoUI.writingPanel);
        
        ClearGeometry();
    }

    void InitUI()
    {
        geoUI = new GeoUI();

        Transform canvasBack = GameObject.Find("/UI/CanvasBack").transform;
        Transform canvasFront = GameObject.Find("/UI/CanvasFront").transform;

        NavPanel navPanel = canvasBack.Find("NavPanel").GetComponent<NavPanel>();
        navPanel.OnWritingButtonClick = HandleClickWritingButton;
        navPanel.OnSpeechButtonClick = HandleClickSpeechButton;
        navPanel.OnShadeButtonClick = HandleClickShadeButton;
        navPanel.OnLockButtonClick = HandleClickLockButton;
        navPanel.OnDisplayButtonClick = HandleClickDisplayButton;
        navPanel.OnCoordinateButtonClick = HandleClickCoordinateButton;
        navPanel.OnGridButtonClick = HandleClickGridButton;
        navPanel.OnSnapButtonClick = HandleClickSnapButton;
        geometryBehaviour.OnElementDisplayChange = HandleElementDisplayChange;
        navPanel.Init();

        ToolPanel toolPanel = canvasBack.Find("ToolPanel").GetComponent<ToolPanel>();
        toolPanel.Init();
        toolPanel.OnClickTool = HandleClickTool;

        StatePanel statePanel = canvasBack.Find("StatePanel").GetComponent<StatePanel>();
        statePanel.Init();

        InputPanel inputPanel = canvasBack.Find("InputPanel").GetComponent<InputPanel>();
        inputPanel.Init();

        ElementPanel elementPanel = canvasFront.Find("ElementPanel").GetComponent<ElementPanel>();
        elementPanel.Init();

        ActivePanel activePanel = canvasFront.Find("ActivePanel").GetComponent<ActivePanel>();
        activePanel.Init();

        WritingPanel writingPanel = canvasFront.Find("WritingPanel").GetComponent<WritingPanel>();
        writingPanel.Init();

        CameraPanel cameraPanel = canvasFront.Find("CameraPanel").GetComponent<CameraPanel>();
        cameraPanel.OnCenterButtonClick = HandleClickCenterButton;
        cameraPanel.OnZoomInButtonClick = HandleClickZoomInButton;
        cameraPanel.OnZoomOutButtonClick = HandleClickZoomOutButton;
        cameraPanel.OnUpButtonClick = HandleClickUpButton;
        cameraPanel.OnDownButtonClick = HandleClickDownButton;
        cameraPanel.Init();
        cameraPanel.SetCenterButtonActive(false);

        geoUI.navPanel = navPanel;
        geoUI.toolPanel = toolPanel;
        geoUI.statePanel = statePanel;
        geoUI.inputPanel = inputPanel;
        geoUI.elementPanel = elementPanel;
        geoUI.activePanel = activePanel;
        geoUI.cameraPanel = cameraPanel;
        geoUI.writingPanel = writingPanel;
    }

    void InitTouchSystem()
    {
        // Pan
        PanRecognizer panRecognizer = new PanRecognizer();

        panRecognizer.gestureCanTrigger = () =>
        {
            bool isInViewport = geoCamera.IsInViewport();
            if (!isInViewport)
                return false;

            if (state == GeoState.Normal)
                return canCameraRotate;
            return currentOperation.CanRotateCamera;
        };
        panRecognizer.gestureBeginEvent += (r) =>
        {
            isCameraRotate = true;
            geoUI.activePanel.Clear();
        };

        panRecognizer.gestureRecognizedEvent += (r) =>
        {
            Vector2 delta = -(r.deltaPosition) * 0.1f;
            geoCamera.Rotate(delta);
        };

        panRecognizer.gestureEndEvent += (r) =>
        {
            isCameraRotate = false;
        };

        TouchSystem.addRecognizer(panRecognizer);
    }

    void InitView()
    {
        geoCamera.Init();
        navCamera.Init(geoCamera);
        geometryBehaviour.Init(geoCamera);
        navigationBehaviour.Init(geoCamera);
        gridBehaviour.Init(geoCamera, 10);
        coordinateBehaviour.Init(geoCamera, 10);
    }

    private void SetState(GeoState newState)
    {
        state = newState;
    }

    public GeoState GetState()
    {
        return state;
    }

    public bool IsCameraRotate()
    {
        return isCameraRotate;
    }

    public void ClearGeometry()
    {
        this.geometry = null;

        toolController.SetIsGeometry(false);
        toolController.SetToolType(GeometryType.Common);
        toolController.RefreshToolPanel();

        stateController.ClearStates();
        geometryBehaviour.Clear();
    }

    public void SetGeometry(Geometry geometry)
    {
        ClearGeometry();

        this.geometry = geometry;
        geometryBehaviour.InitGeometry(geometry);
        geometryBehaviour.SetShadeType(shadeType);

        toolController.SetIsGeometry(true);
        toolController.SetToolType(geometry.Type);
        toolController.RefreshToolPanel();

        geoUI.navPanel.SetDisplayButtonStatus(0);
    }

    public void RefreshGeometryBehaviour()
    {
        geometryBehaviour.UpdateElements();
        geometryBehaviour.UpdateSignsPosition();
    }

    public void RemoveAuxiliary(Auxiliary auxiliary)
    {

        bool result = geometry.Assistor.RemoveAuxiliary(auxiliary);

        if (result)
        {
            stateController.RemoveAuxiliaryState(auxiliary);

            VertexUnit[] units = auxiliary.units;
            GeoElement[] elements = auxiliary.elements;

            foreach (GeoElement element in elements)
                geometry.RemoveElement(element);

            foreach (VertexUnit unit in units)
                geometry.RemoveVertexUnit(unit.id);

            foreach (GeoElement element in elements)
                geometryBehaviour.RemoveElement(element);

            foreach (VertexUnit unit in units)
                geometryBehaviour.RemoveSign(unit.id);

            Gizmo[] gizmos = auxiliary.gizmos;
            if (gizmos != null)
            {
                foreach (Gizmo gizmo in gizmos)
                {
                    geometry.RemoveGizmo(gizmo);
                    geometryBehaviour.RemoveGizmo(gizmo);
                }
            }

        }
    }

    public void RemoveMeasure(Measure measure)
    {
        bool result = geometry.Implement.RemoveMeasure(measure);

        if (result)
        {
            stateController.RemoveMeasureState(measure);

            Gizmo[] gizmos = measure.gizmos;
            if (gizmos != null)
            {
                foreach (Gizmo gizmo in gizmos)
                {
                    geometry.RemoveGizmo(gizmo);
                    geometryBehaviour.RemoveGizmo(gizmo);
                }
            }
        }
    }

    public void RemoveCondition(Condition condition)
    {
        bool result = geometry.Constructor.RemoveCondition(condition);

        if (result)
        {
            stateController.RemoveConditionState(condition);

            Gizmo[] gizmos = condition.gizmos;
            if (gizmos != null)
            {
                foreach (Gizmo gizmo in gizmos)
                {
                    geometry.RemoveGizmo(gizmo);
                    geometryBehaviour.RemoveGizmo(gizmo);
                }
            }
        }
    }

    #region EventHandle

    public void HandleClickCenterButton()
    {
        geoUI.cameraPanel.SetCenterButtonActive(false);
        geoCamera.TriggerCenterZMAnimation();
    }

    public void HandleClickZoomInButton()
    {
        geoUI.cameraPanel.SetCenterButtonActive(true);
        geoCamera.TriggerZoomAnimation(-0.5f);
    }

    public void HandleClickZoomOutButton()
    {
        geoUI.cameraPanel.SetCenterButtonActive(true);
        geoCamera.TriggerZoomAnimation(0.5f);
    }

    public void HandleClickUpButton()
    {
        geoUI.cameraPanel.SetCenterButtonActive(true);
        geoCamera.TriggerMoveYAnimation(0.5f);
    }

    public void HandleClickDownButton()
    {
        geoUI.cameraPanel.SetCenterButtonActive(true);
        geoCamera.TriggerMoveYAnimation(-0.5f);
    }

    public void HandleClickTool(ToolGroupType type, Tool tool)
    {
        switch (type)
        {
            case ToolGroupType.Geometry:
                GeometryOperation(tool);
                //Classify("正方体ABCD-A1B1C1D1");
                //Classify("三棱锥P-ABC");
                //Classify("三角形旋转体");
                break;
            case ToolGroupType.Condition:
                AddConditionOperation(tool);
                //Classify("旋转");
                break;
            case ToolGroupType.Auxiliary:
                AddAuxiliaryOperation(tool);
                //Classify("连接AD1");
                //Classify("面ABCD的面积");
                //Classify("作AB中点P");
                //Classify("连接A1BC1作平面");
                break;
            case ToolGroupType.Measure:
                AddMeasureOperation(tool);
                //Classify("过点B1作平面A1BC1的垂线交于点P");
                //Classify("角AD1D的角度");
                //Classify("过点P作平面A1B1C1D1的垂线交于点O");
                //Classify("连接AD1");
                break;
        }
    }
    public void HandleClickWritingButton(int i)
    {
        if (geoUI.writingPanel != null)
            geoUI.writingPanel.OpenWritingPanel();
    }

    public void HandleClickSpeechButton(int i)
    {
    }

    public void HandleClickShadeButton(int i)
    {
        shadeType = (GeometryShadeType)i;
        geometryBehaviour.SetShadeType(shadeType);
    }

    public void HandleClickLockButton(int i)
    {
        canCameraRotate = i == 0;
    }

    public void HandleClickDisplayButton(int i)
    {
        if (i == 0)
            geometryBehaviour.GeometryShowAllElements();
    }

    public void HandleClickCoordinateButton(int i)
    {
        coordinateBehaviour.gameObject.SetActive(i == 0);
    }

    public void HandleClickGridButton(int i)
    {
        gridBehaviour.gameObject.SetActive(i == 0);
    }

    public void HandleClickSnapButton(int i)
    {
        isSnapToGrid = i == 0;
    }

    public void HandleElementDisplayChange(int i)
    {
        geoUI.navPanel.SetDisplayButtonStatus(i > 0 ? 1 : 0);
    }

    public void ClickVertex(GeoVertex vertex)
    {
        if (currentOperation == null)
            EditVertexOperation(vertex);
        else
            currentOperation.OnClickElement(vertex);
    }

    public void ClickEdge(GeoEdge edge)
    {
        if (currentOperation == null)
            EditEdgeOperation(edge);
        else
            currentOperation.OnClickElement(edge);
    }

    public void ClickFace(GeoFace face)
    {
        if (currentOperation == null)
            EditFaceOperation(face);
        else
            currentOperation.OnClickElement(face);
    }

    public void ClickSpreadFace(GeoFace face)
    {
        int color = face.color == 0 ? 1 : 0;
        geometry.SetSpreadFaceStyle(face, color);
        geometryBehaviour.GeometryElementColorChange(face, color);
    }

    public void HoverVertex(GeoVertex vertex, bool isHover)
    {
        if (isCameraRotate)
            return;
        if (currentOperation != null && !currentOperation.CanActiveElement)
            return;
        if (isHover)
        {
            FormElement formElement = VertexForm(vertex);
            geoUI.activePanel.SetVertex(formElement);
        }
        else
            geoUI.activePanel.Clear();

        geometryBehaviour.HighlightVertex(vertex, isHover);
    }

    public void HoverEdge(GeoEdge edge, bool isHover)
    {
        if (isCameraRotate)
            return;
        if (currentOperation != null && !currentOperation.CanActiveElement)
            return;
        if (isHover)
        {
            FormElement formElement = EdgeForm(edge);
            geoUI.activePanel.SetEdge(formElement);
        }
        else
            geoUI.activePanel.Clear();

        geometryBehaviour.HighlightEdge(edge, isHover);
    }

    public void HoverFace(GeoFace face, bool isHover)
    {
        if (isCameraRotate)
            return;
        if (currentOperation != null && !currentOperation.CanActiveElement)
            return;
        if (isHover && face.faceType == FaceType.Normal)
        {
            FormElement formElement = FaceForm(face);
            geoUI.activePanel.SetFace(formElement);
        }
        else
            geoUI.activePanel.Clear();

        geometryBehaviour.HighlightFace(face, isHover);
    }

    #endregion

    #region Common

    public FormElement VertexForm(GeoVertex vertex)
    {
        string s = geometry.VertexSign(vertex.Id);
        FormElement formElement = new FormElement(1, new string[] { s });
        return formElement;
    }

    public FormElement EdgeForm(GeoEdge edge)
    {
        string s1 = geometry.VertexSign(edge.Id1);
        string s2 = geometry.VertexSign(edge.Id2);
        FormElement formElement = new FormElement(2, new string[] { s1, s2 });
        return formElement;
    }

    public FormElement FaceForm(GeoFace face)
    {
        int count = face.Ids.Length;
        string[] signs = new string[count];

        for (int i = 0; i < count; i++)
        {
            signs[i] = geometry.VertexSign(face.Ids[i]);
        }

        FormElement formElement = new FormElement(count, signs);
        return formElement;
    }

    #endregion


    #region Operation

    public void EndOperation()
    {
        if (currentOperation == null)
            return;
        currentOperation.End();
        currentOperation = null;
        SetState(GeoState.Normal);
    }

    public void GeometryOperation(Tool tool)
    {
        if (state != GeoState.Normal)
            return;

        currentOperation = new GeometryOperation(this, toolController, stateController, tool,geometryBehaviour);
        currentOperation.Start();
    }

    public void Classify(String str)
    {
        /*
        正方体/立方体ABCD-A1B1C1D1
        三棱锥P-ABC
        旋转体  矩形旋转体、三角形旋转体
        长宽高  
            
        作空间中一点A(1,1,1)
    	作两点、线段中点              作AB中点C
		线段描点                     取AB上一点C
	    作平面重心                   作面ABC..重心M
	    连接两点                     连接MN 
        过点作线段垂线               过点A作线段MN垂线交线段于点P
	    过点作平面垂线               过点A作面ABC..垂线交面于点Q
        连接点作平面                 连接ABC..作平面
        测量长度、角度、面积
         */
        if (state != GeoState.Normal)
            return;
        if (str == "旋转")
        {
            Debug.Log("旋转");
            Tool tool = geoUI.toolPanel.toolGroups[2].Tools[0];
            //Debug.Log(tool.Type);  //reslovedBody
            SetState(GeoState.Auxiliary);

            currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
            currentOperation.Start();
        }
        else if (str == "展开")
        {
            Debug.Log("展开");
            Tool tool = geoUI.toolPanel.toolGroups[2].Tools[1];
            //Debug.Log(tool.Type);  //reslovedBody
            SetState(GeoState.Auxiliary);

            currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
            currentOperation.Start();
        }
        else if (str.Length < 3)
        {
            Debug.Log("input error");
        }
        else if (str.Substring(0, 3) == "正方体" || str.Substring(0, 3) == "立方体")
        {
            Debug.Log("正方体");
            Tool tool = geoUI.toolPanel.toolGroups[0].Tools[0];
            currentOperation = new GeometryOperation(this, toolController, stateController, tool, geometryBehaviour);
            String list = "";

            if (str.Length > 3)
            {
                //Debug.Log(str.Substring(3));
                String cuboidName = str.Substring(3);

                if (cuboidName.Length >= 9)
                {

                    for (int i = 0; i < cuboidName.Length; i++)
                    {
                        String item = cuboidName.Substring(i, 1);
                        if (Regex.IsMatch(cuboidName.Substring(i, 1), @"^[A-Za-z]+$"))
                        {
                            if (i == cuboidName.Length - 1)
                            {
                                list += item;
                            }
                            else
                            {
                                if (Regex.IsMatch(cuboidName.Substring(i + 1, 1), @"^[0-9]*$"))
                                {
                                    list += cuboidName.Substring(i, 2);
                                    if (i != cuboidName.Length - 2)
                                    {
                                        list += " ";
                                    }
                                    i++;
                                }
                                else
                                {
                                    list += item + " ";
                                }
                            }
                        }
                    }
                    //Debug.Log(list);
                    GeometryOperation opt = (GeometryOperation)currentOperation;
                    opt.ReSetSign(list);
                }
                else
                {
                    // default name
                }
            }

            currentOperation.Start();


        }
        else if (str.Substring(0, 3) == "三棱锥")
        {
            Debug.Log("三棱锥");
            Tool tool = geoUI.toolPanel.toolGroups[0].Tools[1];
            currentOperation = new GeometryOperation(this, toolController, stateController, tool, geometryBehaviour);
            String list = "";

            if (str.Length > 3)
            {
                Debug.Log(str.Substring(3));
                String triName = str.Substring(3);

                if (triName.Length >= 5)
                {

                    for (int i = 0; i < triName.Length; i++)
                    {
                        String item = triName.Substring(i, 1);
                        if (Regex.IsMatch(triName.Substring(i, 1), @"^[A-Za-z]+$"))
                        {
                            if (i == triName.Length - 1)
                            {
                                list += item;
                            }
                            else
                            {
                                if (Regex.IsMatch(triName.Substring(i + 1, 1), @"^[0-9]*$"))
                                {
                                    list += triName.Substring(i, 2);
                                    if (i != triName.Length - 2)
                                    {
                                        list += " ";
                                    }
                                    i++;
                                }
                                else
                                {
                                    list += item + " ";
                                }
                            }
                        }
                    }
                    Debug.Log(list);
                    GeometryOperation opt = (GeometryOperation)currentOperation;
                    opt.ReSetSign(list);
                }
                else
                {
                    // default name
                }
            }

            currentOperation.Start();

        }
        else if (str.IndexOf("旋转体") != -1)
        {
            Debug.Log("旋转体");
            Tool tool = geoUI.toolPanel.toolGroups[0].Tools[2];
            currentOperation = new GeometryOperation(this, toolController, stateController, tool, geometryBehaviour);
            currentOperation.Start();
            if (str == "矩形旋转体")
            {
                Tool tool1 = geoUI.toolPanel.toolGroups[1].Tools[0];
                Debug.Log(tool1.Description);
                //Debug.Log(tool1.Type);

                SetState(GeoState.Condition);

                currentOperation = new AddConditionOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool1);
                currentOperation.Start();

            }
            else if (str == "三角形旋转体")
            {
                Tool tool1 = geoUI.toolPanel.toolGroups[1].Tools[1];
                Debug.Log(tool1.Description);
                //Debug.Log(tool1.Type);

                SetState(GeoState.Condition);

                currentOperation = new AddConditionOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool1);
                currentOperation.Start();
            }
        }
        else if (str.IndexOf("空间一点") != -1)
        {
            Debug.Log("取空间一点");

        }
        else if (str.IndexOf("中点") != -1)
        {
            Debug.Log("中点");
            String line = "";
            for (int i = 1; i < str.Length - 2; i++)
            {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        line += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                    }
                    else
                    {
                        line += str.Substring(i, 1) + " ";
                    }
                }
            }

            FormText text1 = new FormText("作");
            String[] eles = line.Split(' ');
            FormElement ele1 = new FormElement(eles.Length - 1);
            for (int i = 0; i < eles.Length - 1; i++)
            {
                ele1.fields[i] = eles[i];
            }
            FormText text2 = new FormText("中点");
            FormElement ele2 = new FormElement(1);
            if (Regex.IsMatch(str.Substring(str.Length - 2), @"^[A-Za-z0-9]+$"))
            {
                ele2.fields[0] = str.Substring(str.Length - 2);
            }
            else
            {
                ele2.fields[0] = str.Substring(str.Length - 1);
            }

            FormInput writeInput = new FormInput(4);
            writeInput.inputs[0] = text1;
            writeInput.inputs[1] = ele1;
            writeInput.inputs[2] = text2;
            writeInput.inputs[3] = ele2;

            Tool tool = geoUI.toolPanel.toolGroups[2].Tools[1];
            SetState(GeoState.Auxiliary);


            currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
            AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
            opt.SetWriteInput(writeInput);
            currentOperation.Start();
        }
        else if ((str.IndexOf("一点") != -1 && str.IndexOf("线段") != -1) || (str.IndexOf("作线段") != -1 && str.IndexOf("的点") != -1))
        {
            Debug.Log("取线段一点");

            String line = "";
            for (int i = 1; i < str.Length - 2; i++)
            {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        line += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                    }
                    else
                    {
                        line += str.Substring(i, 1) + " ";
                    }
                }
            }
            //Debug.Log(line);
            FormText text1 = new FormText("作线段");
            String[] eles = line.Split(' ');
            FormElement ele1 = new FormElement(eles.Length - 1);
            for (int i = 0; i < eles.Length - 1; i++)
            {
                ele1.fields[i] = eles[i];
            }
            FormText text2 = new FormText("的点");
            FormElement ele2 = new FormElement(1);
            if (Regex.IsMatch(str.Substring(str.Length - 2), @"^[A-Za-z0-9]+$"))
            {
                ele2.fields[0] = str.Substring(str.Length - 2);
            }
            else
            {
                ele2.fields[0] = str.Substring(str.Length - 1);
            }
            //Debug.Log(ele2.fields[0]);
            FormInput writeInput = new FormInput(4);
            writeInput.inputs[0] = text1;
            writeInput.inputs[1] = ele1;
            writeInput.inputs[2] = text2;
            writeInput.inputs[3] = ele2;

            Tool tool = geoUI.toolPanel.toolGroups[2].Tools[2];
            SetState(GeoState.Auxiliary);


            currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
            AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
            opt.SetWriteInput(writeInput);
            currentOperation.Start();
        }
        else if (str.IndexOf("重心") != -1)
        {
            Debug.Log("重心");
            String face = "";
            for (int i = 3; i < str.Length - 3; i++)
            {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        face += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                    }
                    else
                    {
                        face += str.Substring(i, 1) + " ";
                    }
                }
            }

            FormText text1 = new FormText("作平面");
            String[] eles = face.Split(' ');
            FormElement ele1 = new FormElement(eles.Length - 1);
            for (int i = 0; i < eles.Length - 1; i++)
            {
                ele1.fields[i] = eles[i];
            }
            FormText text2 = new FormText("的重心");
            FormElement ele2 = new FormElement(1);
            if (Regex.IsMatch(str.Substring(str.Length - 2), @"^[A-Za-z0-9]+$"))
            {
                ele2.fields[0] = str.Substring(str.Length - 2);
            }
            else
            {
                ele2.fields[0] = str.Substring(str.Length - 1);
            }
            FormInput writeInput = new FormInput(4);
            writeInput.inputs[0] = text1;
            writeInput.inputs[1] = ele1;
            writeInput.inputs[2] = text2;
            writeInput.inputs[3] = ele2;

            Tool tool = geoUI.toolPanel.toolGroups[2].Tools[3];
            SetState(GeoState.Auxiliary);


            currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
            AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
            opt.SetWriteInput(writeInput);
            currentOperation.Start();
        }
        else if (str.IndexOf("连接") != -1 && Regex.IsMatch(str.Substring(2), @"^[A-Za-z0-9]+$"))
        {
            Debug.Log("连接两点");

            String line = "";
            for (int i = 2; i < str.Length - 1; i++)
            {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        line += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                    }
                    else
                    {
                        line += str.Substring(i, 1) + " ";
                        if (i == str.Length - 2)
                        {
                            line += str.Substring(i + 1);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            FormText text1 = new FormText("连接");
            String[] eles = line.Split(' ');
            FormElement ele1 = new FormElement(eles.Length);
            for (int i = 0; i < eles.Length; i++)
            {
                ele1.fields[i] = eles[i];
            }
            FormText text2 = new FormText("作线段");

            FormInput writeInput = new FormInput(3);
            writeInput.inputs[0] = text1;
            writeInput.inputs[1] = ele1;
            writeInput.inputs[2] = text2;

            Tool tool = geoUI.toolPanel.toolGroups[2].Tools[4];
            SetState(GeoState.Auxiliary);


            currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
            AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
            opt.SetWriteInput(writeInput);
            currentOperation.Start();
        }
        else if (str.IndexOf("垂线") != -1 && str.IndexOf("线段") != -1)
        {
            Debug.Log("线段垂线");
            String point1 = "";
            for (int i = 1; i < 4; i++) {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        point1 += str.Substring(i, 2);
                        //Debug.Log(point1 + "---");
                        break;
                    }
                    else
                    {
                        point1 += str.Substring(i, 1);
                        break;
                    }
                }
            }
            
            String face = "";
            for (int i = 5; i < str.Length - 2; i++)
            {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        face += str.Substring(i, 2) + " ";
                        i++;
                    }
                    else
                    {
                        face += str.Substring(i, 1) + " ";
                    }
                }
            }

            String point2 = "";
            if (Regex.IsMatch(str.Substring(str.Length - 2), @"^[A-Za-z0-9]+$"))
            {
                point2 = str.Substring(str.Length - 2);
            }
            else if (Regex.IsMatch(str.Substring(str.Length-1), @"^[A-Za-z]+$"))
            {
                point2 = str.Substring(str.Length - 1);
            }
            else {
                Debug.Log("point lost error");
                return;
            }

            FormText text1 = new FormText("过点");
            //Debug.Log(text1);
            FormElement ele1 = new FormElement(1);
            ele1.fields[0] = point1;
            //Debug.Log(ele1);
            FormText text2 = new FormText("作平面");
            //Debug.Log(text2);
            String[] eles = face.Split(' ');
            FormElement ele2 = new FormElement(eles.Length-1);
            for (int i = 0; i < eles.Length-1; i++)
            {
                ele2.fields[i] = eles[i];
                //Debug.Log(eles[i] +"*");
            }
            FormText text3 = new FormText("的垂线");
            //Debug.Log(text3);
            FormText text4 = new FormText("交于点");
            //Debug.Log(text4);
            FormElement ele3 = new FormElement(1);
            ele3.fields[0] = point2;
            //Debug.Log(point2);

            FormInput writeInput = new FormInput(7);
            writeInput.inputs[0] = text1;
            writeInput.inputs[1] = ele1;
            writeInput.inputs[2] = text2;
            writeInput.inputs[3] = ele2;
            writeInput.inputs[4] = text3;
            writeInput.inputs[5] = text4;
            writeInput.inputs[6] = ele3;

            Tool tool = geoUI.toolPanel.toolGroups[2].Tools[5];
            SetState(GeoState.Auxiliary);


            currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
            AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
            opt.SetWriteInput(writeInput);
            currentOperation.Start();

        }
        else if (str.IndexOf("垂线") != -1 && str.IndexOf("平面") != -1)
        {
            Debug.Log("面垂线");

            String point1 = "";
            for (int i = 1; i < 4; i++)
            {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        point1 += str.Substring(i, 2);
                        //Debug.Log(point1 + "---");
                        break;
                    }
                    else
                    {
                        point1 += str.Substring(i, 1);
                        break;
                    }
                }
            }

            String line = "";
            for (int i = 5; i < str.Length - 2; i++)
            {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        line += str.Substring(i, 2) + " ";
                        i++;
                    }
                    else
                    {
                        line += str.Substring(i, 1) + " ";
                    }
                }
            }

            String point2 = "";
            if (Regex.IsMatch(str.Substring(str.Length - 2), @"^[A-Za-z0-9]+$"))
            {
                point2 = str.Substring(str.Length - 2);
            }
            else if (Regex.IsMatch(str.Substring(str.Length - 1), @"^[A-Za-z]+$"))
            {
                point2 = str.Substring(str.Length - 1);
            }
            else
            {
                Debug.Log("point lost error");
                return;
            }

            FormText text1 = new FormText("过点");
            //Debug.Log(text1);
            FormElement ele1 = new FormElement(1);
            ele1.fields[0] = point1;
            //Debug.Log(ele1);
            FormText text2 = new FormText("作线段");
            //Debug.Log(text2);
            String[] eles = line.Split(' ');
            FormElement ele2 = new FormElement(eles.Length - 1);
            for (int i = 0; i < eles.Length - 1; i++)
            {
                ele2.fields[i] = eles[i];
                //Debug.Log(eles[i] +"*");
            }
            FormText text3 = new FormText("的垂线");
            //Debug.Log(text3);
            FormText text4 = new FormText("交于点");
            //Debug.Log(text4);
            FormElement ele3 = new FormElement(1);
            ele3.fields[0] = point2;
            //Debug.Log(point2);

            FormInput writeInput = new FormInput(7);
            writeInput.inputs[0] = text1;
            writeInput.inputs[1] = ele1;
            writeInput.inputs[2] = text2;
            writeInput.inputs[3] = ele2;
            writeInput.inputs[4] = text3;
            writeInput.inputs[5] = text4;
            writeInput.inputs[6] = ele3;

            Tool tool = geoUI.toolPanel.toolGroups[2].Tools[6];
            SetState(GeoState.Auxiliary);


            currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
            AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
            opt.SetWriteInput(writeInput);
            currentOperation.Start();
        }
        else if (str.IndexOf("连接") != -1 && str.IndexOf("作平面") != -1)
        {
            Debug.Log("平面");

            String face = "";
            for (int i = 2; i < str.Length - 3; i++)
            {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        face += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                    }
                    else
                    {
                        face += str.Substring(i, 1) + " ";
                    }
                }
            }

            FormText text1 = new FormText("连接");
            String[] eles = face.Split(' ');
            FormElement ele1 = new FormElement(eles.Length - 1);
            for (int i = 0; i < eles.Length - 1; i++)
            {
                ele1.fields[i] = eles[i];
            }
            FormText text2 = new FormText("作平面");
            FormInput writeInput = new FormInput(3);
            writeInput.inputs[0] = text1;
            writeInput.inputs[1] = ele1;
            writeInput.inputs[2] = text2;

            Tool tool = geoUI.toolPanel.toolGroups[2].Tools[7];
            SetState(GeoState.Auxiliary);


            currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
            AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
            opt.SetWriteInput(writeInput);
            currentOperation.Start();
        }
        else if (str.IndexOf("长度") != -1)
        {
            Debug.Log("测量长度");

            String line = "";
            for (int i = 0; i < str.Length - 1; i++)
            {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        line += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                    }
                    else
                    {
                        line += str.Substring(i, 1) + " ";
                    }
                }
            }

            FormText text1 = new FormText("线段");
            String[] eles = line.Split(' ');
            FormElement ele1 = new FormElement(eles.Length-1);
            for (int i = 0; i < eles.Length - 1; i++)
            {
                ele1.fields[i] = eles[i];
            }
            FormText text2 = new FormText("的长度");
            FormInput writeInput = new FormInput(3);
            writeInput.inputs[0] = text1;
            writeInput.inputs[1] = ele1;
            writeInput.inputs[2] = text2;

            Tool tool = geoUI.toolPanel.toolGroups[3].Tools[0];
            SetState(GeoState.Measure);

            currentOperation = new AddMeasureOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool);
            AddMeasureOperation opt = (AddMeasureOperation)currentOperation;
            opt.SetWriteInput(writeInput);
            currentOperation.Start();
        }
        else if (str.IndexOf("角度") != -1)
        {
            Debug.Log("测量角度");

            String angle = "";
            for (int i = 0; i < str.Length - 1; i++)
            {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        angle += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                    }
                    else
                    {
                        angle += str.Substring(i, 1) + " ";
                    }
                }
            }

            FormText text1 = new FormText("∠");
            String[] eles = angle.Split(' ');
            if (eles.Length - 1 != 3) {
                Debug.Log("angle input error");
                return;
            } 
            FormElement ele1 = new FormElement(eles.Length - 1);
            for (int i = 0; i < eles.Length - 1; i++)
            {
                ele1.fields[i] = eles[i];
            }
            FormText text2 = new FormText("的角度");
            FormInput writeInput = new FormInput(3);
            writeInput.inputs[0] = text1;
            writeInput.inputs[1] = ele1;
            writeInput.inputs[2] = text2;

            Tool tool = geoUI.toolPanel.toolGroups[3].Tools[1];
            SetState(GeoState.Measure);

            currentOperation = new AddMeasureOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool);
            AddMeasureOperation opt = (AddMeasureOperation)currentOperation;
            opt.SetWriteInput(writeInput);
            currentOperation.Start();
        }
        else if (str.IndexOf("面积") != -1)
        {
            Debug.Log("测量面积");

            String face = "";
            for (int i = 1; i < str.Length - 1; i++)
            {
                if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                {
                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                    {
                        face += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                    }
                    else
                    {
                        face += str.Substring(i, 1) + " ";
                    }
                }
            }

            FormText text1 = new FormText("平面");
            String[] eles = face.Split(' ');
            FormElement ele1 = new FormElement(eles.Length - 1);
            for (int i = 0; i < eles.Length - 1; i++)
            {
                ele1.fields[i] = eles[i];
            }
            FormText text2 = new FormText("的面积");
            FormInput writeInput = new FormInput(3);
            writeInput.inputs[0] = text1;
            writeInput.inputs[1] = ele1;
            writeInput.inputs[2] = text2;

            Tool tool = geoUI.toolPanel.toolGroups[3].Tools[2];
            SetState(GeoState.Measure);

            currentOperation = new AddMeasureOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool);
            AddMeasureOperation opt = (AddMeasureOperation)currentOperation;
            opt.SetWriteInput(writeInput);
            currentOperation.Start();
        }
        else
        {
            Debug.Log("analysis error");
        }
    }


    public void MoveVertexOperation(GeoVertex geoVertex, VertexBehaviour vertexBehaviour)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.MoveVertex);

        VertexUnit unit = geometry.VertexUnit(geoVertex.Id);

        currentOperation = new MoveVertexOperation(stateController, geoCamera, geometry, geometryBehaviour, geoUI, unit, vertexBehaviour, isSnapToGrid);
        currentOperation.Start();
    }

    public void EditVertexOperation(GeoVertex geoVertex)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditVertex);

        currentOperation = new EditVertexOperation(this, stateController, geoCamera, geometry, geometryBehaviour, geoUI, geoVertex);
        currentOperation.Start();
    }

    public void EditEdgeOperation(GeoEdge geoEdge)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditEdge);

        currentOperation = new EditEdgeOperation(this, geoCamera, geometry, geometryBehaviour, geoUI, geoEdge);
        currentOperation.Start();
    }

    public void EditFaceOperation(GeoFace geoFace)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditFace);

        currentOperation = new EditFaceOperation(this, geoCamera, geometry, geometryBehaviour, geoUI, geoFace);
        currentOperation.Start();
    }


    public void VertexCoordinateOperation(GeoVertex geoVertex)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditVertex);

        VertexUnit vertexUnit = geometry.VertexUnit(geoVertex.Id);

        currentOperation = new VertexCoordinateOperation(this, stateController, geometry, geometryBehaviour, geoUI, vertexUnit);
        currentOperation.Start();
    }


    public void DeleteVertexOperation(GeoVertex geoVertex)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditVertex);

        currentOperation = new DeleteVertexOperation(this, geometry, geoVertex);
        currentOperation.Start();
    }

    public void DeleteEdgeOperation(GeoEdge geoEdge)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditEdge);

        currentOperation = new DeleteEdgeOperation(this, geometry, geoEdge);
        currentOperation.Start();
    }

    public void DeleteFaceOperation(GeoFace geoFace)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditFace);

        currentOperation = new DeleteFaceOperation(this, geometry, geoFace);
        currentOperation.Start();
    }

    public void AddConditionOperation(Tool tool)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.Condition);

        currentOperation = new AddConditionOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool);
        currentOperation.Start();
    }

    public void AddAuxiliaryOperation(Tool tool)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.Auxiliary);

        currentOperation = new AddAuxiliaryOperation(this,geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
        currentOperation.Start();
    }

    public void AddMeasureOperation(Tool tool)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.Measure);

        currentOperation = new AddMeasureOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool);
        currentOperation.Start();
    }

    public void ClearGeometryOperation(Geometry geometry)
    {
        if (state != GeoState.Normal)
            return;

        currentOperation = new ClearGeometryOperation(this);
        currentOperation.Start();
    }

    public void RemoveConditionOperation(Condition condition)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.Condition);

        currentOperation = new RemoveConditionOperation(this, geometry, condition);
        currentOperation.Start();
    }

    public void RemoveAuxiliaryOperation(Auxiliary auxiliary)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.Auxiliary);

        currentOperation = new RemoveAuxiliaryOperation(this, geometry, auxiliary);
        currentOperation.Start();
    }

    public void RemoveMeasureOperation(Measure measure)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.Measure);

        currentOperation = new RemoveMeasureOperation(this, geometry, measure);
        currentOperation.Start();
    }

    #endregion
}
