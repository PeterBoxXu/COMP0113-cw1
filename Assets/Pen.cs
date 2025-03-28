using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Messaging;

public class Pen : MonoBehaviour
{
    private NetworkContext context;
    private bool owner;
    private Transform nib;
    private Material drawingMaterial;
    private GameObject currentDrawing;

    // 1. Amend message to also store current drawing state
    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isDrawing; // new

        public Message(Transform transform, bool isDrawing) // new
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.isDrawing = isDrawing; // new
        }
    }

    private void Start()
    {
        nib = transform.Find("Grip/Nib");

        var shader = Shader.Find("Sprites/Default");
        drawingMaterial = new Material(shader);

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
            // new
            // 2. Send current drawing state if owner
            context.SendJson(new Message(transform,isDrawing:currentDrawing));
        }
    }

    public void ProcessMessage (ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;

        // new
        // 3. Start drawing locally when a remote user starts
        if (data.isDrawing && !currentDrawing)
        {
            BeginDrawing();
        }
        if (!data.isDrawing && currentDrawing)
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
        var trail = currentDrawing.AddComponent<TrailRenderer>();
        trail.time = Mathf.Infinity;
        trail.material = drawingMaterial;
        trail.startWidth = .05f;
        trail.endWidth = .05f;
        trail.minVertexDistance = .02f;

        currentDrawing.transform.parent = nib.transform;
        currentDrawing.transform.localPosition = Vector3.zero;
        currentDrawing.transform.localRotation = Quaternion.identity;
    }

    private void EndDrawing()
    {
        currentDrawing.transform.parent = null;
        currentDrawing.GetComponent<TrailRenderer>().emitting = false;
        currentDrawing = null;
    }
}