using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Messaging;
using System.Collections.Generic;

public class Pen : MonoBehaviour
{
    private NetworkContext context;
    private bool owner;
    private Transform nib;
    private Material drawingMaterial;
    private GameObject currentDrawing;
    private Renderer gripColorRenderer;

    public Color penColor = Color.white;

    private LineRenderer lineRenderer;
    private MeshCollider meshCollider;
    private List<Vector3> linePoints = new List<Vector3>();

    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isDrawing;

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
            context.SendJson(new Message(transform, isDrawing: currentDrawing != null));

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
        transform.position = data.position;
        transform.rotation = data.rotation;

        if (data.isDrawing && currentDrawing == null)
        {
            BeginDrawing();
        }
        if (!data.isDrawing && currentDrawing != null)
        {
            EndDrawing();
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
        meshCollider.convex = true;
        meshCollider.isTrigger = false;

        linePoints.Clear();
    }

    private void EndDrawing()
    {
        if (currentDrawing != null)
        {
            UpdateMeshCollider(); // 最后更新一次碰撞体
            currentDrawing = null;
            lineRenderer = null;
            meshCollider = null;
        }
    }

    private void UpdateMeshCollider()
    {
        if (linePoints.Count < 2 || lineRenderer == null || meshCollider == null)
            return;

        Mesh bakedMesh = new Mesh();
        lineRenderer.BakeMesh(bakedMesh, true);

        meshCollider.sharedMesh = null; // 清除旧 mesh
        meshCollider.sharedMesh = bakedMesh;
    }

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
