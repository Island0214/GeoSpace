using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CircleBehaviour : ElementBehaviour
{

    const float LINE_RADIUS = 0.02f;
    const float LINE_LENGTH = 1.0f;

    const float LINE_COLLIDER_SIZE = 0.1f;
    const float LINE_COLLIDER_OFFSET = 0.2f;

    private GeoCamera geoCamera;
    private GeoController geoController;
    private Mesh mesh; // Because of vertex color use for normal
    private MeshRenderer meshRenderer;
    private BoxCollider meshCollider;
    private GeoCircle geoCircle;
    private Circle circle;
    private Vector3 normal;
    public int Segments = 60;   // Segment count

    public void Init(GeoCircle geoCircle)
    {
        geoController = GameObject.Find("/GeoController").GetComponent<GeoController>();

        this.geoCircle = geoCircle;
        mesh = new Mesh();


        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<BoxCollider>();
        meshFilter.sharedMesh = mesh;
        meshCollider.size = new Vector3(1 - LINE_COLLIDER_OFFSET, LINE_COLLIDER_SIZE, LINE_COLLIDER_SIZE);

        SetColorIndex(0);
        SetStyleIndex(0);

        StyleManager.OnStyleChange += () =>
        {
            SetColorIndex(0);
            SetStyleIndex(0);
        };

        visiable = true;
    }

    public void SetData(Circle c)
    {
        circle = c;
        normal = Vector3.up.normalized;
        Vector3 wrapNormal = normal * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);

        Color color = new Color(wrapNormal.x, wrapNormal.y, wrapNormal.z, 1);
        Color[] colors = new Color[] { color, color, color, color };
        mesh.colors = colors;

        CircleMesh(mesh, circle);
    }

    public override void SetColorIndex(int index)
    {
        base.SetColorIndex(index);
        StyleManager.SetLineProperty(meshRenderer, colorIndex);
    }

    public override void SetStyleIndex(int index)
    {
        base.SetStyleIndex(index);
        meshRenderer.sharedMaterial = ConfigManager.EdgeStyle[styleIndex].Material;
    }

    private void CircleMesh(Mesh mesh, Circle circle)
    {
        float Radius = circle.radius;
        int segments = Segments;
        Vector3 vertice = circle.Vertice;

        int vertices_count = Segments + 1;
        Vector3[] vertices = new Vector3[vertices_count * 2];
        vertices[0] = vertice;
        float angledegree = 360.0f;
        float angleRad = Mathf.Deg2Rad * angledegree;
        float angleCur = angleRad;
        float angledelta = angleRad / Segments;
        float y = vertice.y;

        for (int i = 1; i < vertices_count; i++)
        {
            float cosA = Mathf.Cos(angleCur);
            float sinA = Mathf.Sin(angleCur);

            vertices[i] = new Vector3(Radius * cosA, y, Radius * sinA);
            angleCur -= angledelta;
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        int[] triangles = new int[vertices.Length * 6];
        
    }

    private Mesh LineMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, -1, 0);
        vertices[1] = new Vector3(-0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, 1, 0);
        vertices[2] = new Vector3(0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, 1, 0);
        vertices[3] = new Vector3(0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, -1, 0);

        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(1, 0);

        int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();

        return mesh;
    }

}