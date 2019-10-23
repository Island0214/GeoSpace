using System;
using System.Collections;
using System.Collections.Generic;
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
        
        ClearGeometry();
    }

    void InitUI()
    {
        geoUI = new GeoUI();

        Transform canvasBack = GameObject.Find("/UI/CanvasBack").transform;
        Transform canvasFront = GameObject.Find("/UI/CanvasFront").transform;

        NavPanel navPanel = canvasBack.Find("NavPanel").GetComponent<NavPanel>();
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
                break;
            case ToolGroupType.Condition:
                AddConditionOperation(tool);
                break;
            case ToolGroupType.Auxiliary:
                AddAuxiliaryOperation(tool);
                break;
            case ToolGroupType.Measure:
                AddMeasureOperation(tool);
                break;
        }
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

        currentOperation = new GeometryOperation(this, toolController, stateController, tool);
        currentOperation.Start();
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
