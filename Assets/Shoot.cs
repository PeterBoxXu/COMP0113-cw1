using System.Collections.Generic;
using Ubiq.Spawning;
using UnityEngine;
using Avatar = Ubiq.Avatars.Avatar;
//using UnityEngine.InputSystem;
using UnityEngine.XR;

public class Shoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public bool leftHand = false;
    public Avatar avatar;
    public float speed = 5f;
    public Transform nozzle;
    private NetworkSpawnManager spawnManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //avatar = GetComponentInChildren<Avatar>();
        spawnManager = NetworkSpawnManager.Find(this);
    }
    void Update()
    {
        if (avatar == null)
        {
            return;
        }

        if (!avatar.IsLocal)
        {
            return;
        }

        if (CheckAction())
        {
            TriggerFunction();
        }
    }

    private bool CheckAction()
    {
        // ¼ì²é¼üÅÌ X ¼ü (Check if keyboard X key is pressed)
        if (Input.GetKeyDown(KeyCode.X))
            return true;

        bool isbuttonPressed = false;

        // ¼ì²éÊÖ±úÓÒ²à°â»ú (Check if XR right-hand trigger is pressed)
        List<InputDevice> devices = new List<InputDevice>();
        if (!leftHand)
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        else
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
        if (devices.Count > 0)
        {
            InputDevice device = devices[0];
            bool triggerPressed = false;
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) && triggerPressed)
                isbuttonPressed = true;
        }

        return isbuttonPressed;
    }

    private void TriggerFunction()
    {
        Debug.Log("Right trigger button pressed!");

        var go = spawnManager.SpawnWithPeerScope(bulletPrefab);
        go.transform.position = nozzle.position;
        go.transform.rotation = nozzle.rotation;
        Bullet bullet = go.GetComponent<Bullet>();
        bullet.owner = true;
        Rigidbody rb = go.GetComponent<Rigidbody>();
        rb.linearVelocity = nozzle.forward * speed;
    }

}
