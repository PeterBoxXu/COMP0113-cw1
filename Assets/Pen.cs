using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Messaging;
using System.Collections.Generic;

public class Pen : MonoBehaviour
{
    private NetworkContext context;            // 网络上下文 (Network Context)
    private bool owner;                        // 是否为拥有者 (Owner flag)
    private Transform nib;                     // 笔尖 (Nib)
    private Material drawingMaterial;          // 绘图材质 (Drawing Material)
    private GameObject currentDrawing;         // 当前绘图对象 (Current Drawing)
    private Renderer gripColorRenderer;        // 手柄颜色渲染器 (Grip Color Renderer)

    public Color penColor = Color.white;       // 笔颜色 (Pen Color)

    private LineRenderer lineRenderer;         // 线渲染器 (Line Renderer)
    private MeshCollider meshCollider;         // 网格碰撞器 (Mesh Collider)
    private List<Vector3> linePoints = new List<Vector3>();  // 绘制线段点集合 (Line Points List)

    // 网络消息结构 (Network Message Structure)
    private struct Message
    {
        public Vector3 position;               // 位置 (Position)
        public Quaternion rotation;            // 旋转 (Rotation)
        public bool isDrawing;                 // 是否正在绘制 (Drawing State)

        public Message(Transform transform, bool isDrawing)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.isDrawing = isDrawing;
        }
    }

    private void Start()
    {
        nib = transform.Find("Grip/Nib");

        var shader = Shader.Find("Sprites/Default");
        drawingMaterial = new Material(shader);
        drawingMaterial.color = penColor;

        Transform colorTransform = transform.Find("Grip/color");
        if (colorTransform != null)
        {
            gripColorRenderer = colorTransform.GetComponent<Renderer>();
            if (gripColorRenderer != null)
            {
                gripColorRenderer.material.color = penColor;
            }
        }

        // 使用 XRGrabInteractable 组件 (Use XRGrabInteractable)
        var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.activated.AddListener(XRGrabInteractable_Activated);
        grab.deactivated.AddListener(XRGrabInteractable_Deactivated);
        grab.selectEntered.AddListener(XRGrabInteractable_SelectEntered);
        grab.selectExited.AddListener(XRGrabInteractable_SelectExited);

        context = NetworkScene.Register(this);
    }

    private void FixedUpdate()
    {
        if (owner)
        {
            // 发送网络消息，同步笔的位置、旋转和绘制状态
            context.SendJson(new Message(transform, isDrawing: currentDrawing != null));

            // 如果正在绘制，则不断更新线段 (仅拥有者更新)
            if (currentDrawing != null && lineRenderer != null)
            {
                Vector3 currentPos = nib.position;
                if (linePoints.Count == 0 || Vector3.Distance(currentPos, linePoints[linePoints.Count - 1]) > 0.01f)
                {
                    linePoints.Add(currentPos);
                    lineRenderer.positionCount = linePoints.Count;
                    lineRenderer.SetPositions(linePoints.ToArray());
                    UpdateMeshCollider();
                }
            }
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();
        // 更新笔的位置和旋转 (远程同步)
        transform.position = data.position;
        transform.rotation = data.rotation;

        // 根据消息控制开始或结束绘制
        if (data.isDrawing && currentDrawing == null)
        {
            BeginDrawing();
        }
        if (!data.isDrawing && currentDrawing != null)
        {
            EndDrawing();
        }

        // 非拥有者端：更新绘制轨迹 (远程端更新 LineRenderer)
        if (data.isDrawing && currentDrawing != null && lineRenderer != null)
        {
            Vector3 currentPos = nib.position;
            if (linePoints.Count == 0 || Vector3.Distance(currentPos, linePoints[linePoints.Count - 1]) > 0.01f)
            {
                linePoints.Add(currentPos);
                lineRenderer.positionCount = linePoints.Count;
                lineRenderer.SetPositions(linePoints.ToArray());
                UpdateMeshCollider();
            }
        }
    }

    private void XRGrabInteractable_Activated(ActivateEventArgs eventArgs)
    {
        BeginDrawing();
    }

    private void XRGrabInteractable_Deactivated(DeactivateEventArgs eventArgs)
    {
        EndDrawing();
    }

    private void XRGrabInteractable_SelectEntered(SelectEnterEventArgs arg0)
    {
        owner = true;
    }

    private void XRGrabInteractable_SelectExited(SelectExitEventArgs eventArgs)
    {
        owner = false;
    }

    // 开始绘制 (Begin Drawing)
    private void BeginDrawing()
    {
        currentDrawing = new GameObject("Drawing");

        lineRenderer = currentDrawing.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = penColor;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;

        meshCollider = currentDrawing.AddComponent<MeshCollider>();
        meshCollider.convex = false;
        meshCollider.isTrigger = false;

        linePoints.Clear();
    }

    // 结束绘制 (End Drawing)
    private void EndDrawing()
    {
        if (currentDrawing != null)
        {
            UpdateMeshCollider(); // 最后更新一次碰撞体
            currentDrawing = null;
            lineRenderer = null;
            meshCollider = null;
            linePoints.Clear();
        }
    }

    // 更新网格碰撞器 (Update Mesh Collider)
    private void UpdateMeshCollider()
    {
        if (linePoints.Count < 2 || lineRenderer == null || meshCollider == null)
            return;

        Mesh bakedMesh = new Mesh();
        lineRenderer.BakeMesh(bakedMesh, true);

        // 先清除旧的 Mesh，再赋值新 Mesh
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = bakedMesh;
    }

    // 设置笔颜色 (Set Pen Color)
    public void SetPenColor(Color newColor)
    {
        penColor = newColor;
        if (drawingMaterial != null)
        {
            drawingMaterial.color = penColor;
        }
        if (gripColorRenderer != null)
        {
            gripColorRenderer.material.color = penColor;
        }
    }
}
