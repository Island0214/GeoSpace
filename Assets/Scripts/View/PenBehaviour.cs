﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pen
{
    List<Vector3> points;
    int id;

    public Pen(int id)
    {
        points = new List<Vector3>();
        this.id = id;
    }

    public void AddPoint(Vector3 point)
    {
        points.Add(point);
    }

    public List<Vector3> GetPoints()
    {
        return points;
    }

    public override string ToString()
    {
        return string.Format("pen {0}", this.id);
    }
}

public class Word
{
    List<Pen> pens;
    List<Vector2> points;
    int id;

    public Word(int id, List<Pen> pens)
    {
        this.id = id;
        this.pens = pens;
        points = new List<Vector2>();
        pens.ForEach(pen =>
        {
            pen.GetPoints().ForEach(p =>
            {
                points.Add(new Vector2(p.x, p.y));
            });
        });
    }

    public List<Pen> GetPens()
    {
        return this.pens;
    }

    public List<Vector2> GetPoints()
    {
        return points;
    }

    public override string ToString()
    {
        return string.Format("word {0}", this.id);
    }
}

public class PenBehaviour : ElementBehaviour
{
    const string LAYER = "UI";
    const float MAX_TIME = 1f;
    const float ValidDiff = 0.2f;

    LineRenderer lineRenderer;
    int i;
    Pen pen;
    List<Pen> pens;
    List<Word> words;
    Camera curCamera;
    int PenCount = 0;
    int WordCount = 0;
    RectTransform penWrapper;
    float waitTime;

    float width;
    float height;
    Vector2 xRange;
    Vector2 yRange;
    Vector3 startPoint;

    public Action<FormInput> OnClickSubmit;
    public Action<FormInput> OnClickCancel;
    Button btnSubmit;
    Button btnCancel;

    Dictionary<Pen, GameObject> penMap;
    RecognizePanel recognizePanel;

    List<Vector3> ShapePositions;

    bool Drawing;
    Geometry geometry;
     GeometryBehaviour geometryBehaviour;

    public void Init(GeoUI geoUI)
    {
        recognizePanel = geoUI.recognizePanel;
        geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();
    }


    public void SetDrawing(bool Drawing)
    {
        this.Drawing = Drawing;
        if (Drawing)
        {
            ShapePositions = new List<Vector3>();
        }
    }

    public void SetGeometry(Geometry geometry)
    {
        this.geometry = geometry;
    }

    public void Start()
    {
        penMap = new Dictionary<Pen, GameObject>();
        pens = new List<Pen>();
        words = new List<Word>();

        lineRenderer = transform.GetComponent<LineRenderer>();
        InitRenderer(lineRenderer);
        penWrapper = (RectTransform)transform;
        width = penWrapper.rect.width;
        height = penWrapper.rect.height;
        xRange = new Vector2(0, width);
        yRange = new Vector2(-height, 0);
        Camera[] cameras = Camera.allCameras;
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].name == "UICameraFront")
                curCamera = cameras[i];
        }
        startPoint = curCamera.WorldToScreenPoint(penWrapper.parent.position) - new Vector3(width, 0, 1);

        btnSubmit = transform.parent.Find("ButtonSubmit").GetComponent<Button>();
        btnCancel = transform.parent.Find("ButtonCancel").GetComponent<Button>();
        btnSubmit.onClick.AddListener(ClickSubmit);
        btnCancel.onClick.AddListener(ClickCancel);

        visiable = true;
        waitTime = 0;
    }

    private void InitRenderer(LineRenderer renderer)
    {
        renderer.startWidth = 1f;
        renderer.endWidth = 1f;
        renderer.useWorldSpace = false;
        SetColorIndex(renderer, 0);
        SetStyleIndex(renderer, 0);
    }

    private bool IsValidPoint(Vector3 vector3)
    {
        if (vector3.x < xRange.x || vector3.x > xRange.y || vector3.y < yRange.x || vector3.y > yRange.y)
        {
            return false;
        }
        return true;
    }

    void FixedUpdate()
    {
        if (!Drawing)
        {
            waitTime += Time.deltaTime;
            if (waitTime > MAX_TIME && pens.Count > 0)
            {
                AddWord();
                waitTime = 0;
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            pen = new Pen(PenCount++);
            i = 0;
            Init(pen);
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 point = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0) - startPoint;
            pen.AddPoint(point);
            SetData(i, point);
            i++;
            waitTime = 0;
        }
        if (Input.GetMouseButtonUp(0))
        {
            AddPen(pen);
        }
    }

    private Vector3 ScreenPositionToAxis(Vector3 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Vector3 position = new Vector3(0, 0, 0);
        Plane screenPlane = new Plane(-ray.direction, position);

        float distance = 0;
        if (screenPlane.Raycast(ray, out distance))
        {
            position = ray.GetPoint(distance);
        }

        // Normalize
        if (Mathf.Abs(position.z) <= ValidDiff)
        {
            position.z = 0;
        }
        return position;
    }

    private void AddPen(Pen pen)
    {
        if (penMap.ContainsKey(pen))
            return;
        if (pen.GetPoints().Count <= 1)
            return;
        pens.Add(pen);
        GameObject penObject = new GameObject(pen.ToString());
        penObject.layer = LayerMask.NameToLayer(LAYER); ;
        RectTransform rect = penObject.AddComponent<RectTransform>();
        penObject.transform.SetParent(penWrapper.transform);
        rect.position = new Vector3(0, 0, 1);
        rect.localScale = new Vector3(1, 1, 1);
        rect.pivot = new Vector2(0, 1);
        rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, penWrapper.rect.width);
        rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, penWrapper.rect.height);
        LineRenderer curLineRenderer = penObject.AddComponent<LineRenderer>();
        InitRenderer(curLineRenderer);
        curLineRenderer.positionCount = pen.GetPoints().Count;
        curLineRenderer.SetPositions(pen.GetPoints().ToArray());
        penMap.Add(pen, penObject);

        if (Drawing)
        {
            List<Vector3> points = pen.GetPoints();
            Vector3 start = ScreenPositionToAxis(points[0] + startPoint);
            Vector3 end = ScreenPositionToAxis(points[points.Count - 1] + startPoint);

            foreach (Vector3 position in new Vector3[] { start, end })
            {
                bool IsNew = true;
                foreach (Vector3 point in ShapePositions)
                {
                    IsNew = !IsSamePoint(position, point);
                    if (!IsNew) {
                        break;
                    }
                }
                if (IsNew)
                {
                    ShapePositions.Add(position);
                }
            }
        }
    }

    private bool IsSamePoint(Vector3 point1, Vector3 point2)
    {
        Vector3 diff = point1 - point2;
        if (diff.sqrMagnitude < new Vector3(ValidDiff, ValidDiff, ValidDiff).sqrMagnitude) {
            return true;
        }
        return false;
    }

    private void AddWord()
    {
        Word word = new Word(WordCount++, new List<Pen>(pens));
        words.Add(word);
        pens.Clear();

        // todo 
        recognizePanel.AddWord("字");
    }

    private bool AddShape()
    {
        if (geometry is ResolvedBody)
        {
            ResolvedBody resolvedBody = (ResolvedBody) geometry;
            if (resolvedBody.shapeSetted) {
                return false;
            }
            switch (ShapePositions.Count)
            {
                case 3:
                    resolvedBody.SetTriangle(ShapePositions.ToArray());
                    break;
                case 4:
                    resolvedBody.SetRectangle(ShapePositions.ToArray());
                    break;
                default:
                    return false;
            }
        }
        geometryBehaviour.InitGeometry(geometry);
        return true;
    }

    public void Init(Pen pen)
    {
        lineRenderer.SetPositions(pen.GetPoints().ToArray());
    }

    public void SetData(int i, Vector3 point)
    {
        lineRenderer.positionCount = (i + 1);
        lineRenderer.SetPosition(i, point);
    }

    public void SetColorIndex(LineRenderer renderer, int index)
    {
        base.SetColorIndex(index);
        StyleManager.SetLineProperty(renderer, colorIndex);
    }

    public void SetStyleIndex(LineRenderer renderer, int index)
    {
        base.SetStyleIndex(index);
        renderer.sharedMaterial = ConfigManager.EdgeStyle[styleIndex].Material;
    }

    private void Clear()
    {
        foreach (KeyValuePair<Pen, GameObject> pair in penMap)
            Destroy(pair.Value);
        penMap.Clear();

        StatusButton lockButton = GameObject.Find("LockButton").GetComponent<StatusButton>();
        lockButton.SetStatus(0);
    }

    private void ClickSubmit()
    {
        // if (OnClickSubmit != null)
        //     OnClickSubmit(form);
        transform.parent.gameObject.SetActive(false);
        recognizePanel.Clear();
        Clear();

        if (Drawing) {
            AddShape();
        }
    }

    private void ClickCancel()
    {
        transform.parent.gameObject.SetActive(false);
        recognizePanel.Clear();
        Clear();
    }
}
