using System;
using UnityEngine;
using UnityEngine.UI;
using Ubiq.Rooms;
using Ubiq.Messaging;

public class NetworkedRobot : MonoBehaviour
{
    private MeshRenderer m_MeshRenderer;
    public GameObject cubeArms;
    public GameObject cylinderArms;
    public GameObject capsuleArms;
    public GameObject cubeLegs;
    public GameObject cylinderLegs;
    public GameObject capsuleLegs;
    public Color originalColor;
    public String jsonString;
    public Text logText;
    public int robotCode;


    private string networkIdString;
    private RoomClient roomClient;

    private void Awake()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();
        robotCode = 0;
    }

    private void Start()
    {
        // Initialize networking
        networkIdString = NetworkId.Create(this).ToString();
        roomClient = GetComponent<RoomClient>();
        roomClient.OnRoomUpdated.AddListener(RoomClient_OnRoomUpdated);
    }

    private void RoomClient_OnRoomUpdated(IRoom room)
    {
        var robotProperty = room[networkIdString];
        if (!string.IsNullOrEmpty(robotProperty))
        {
            // Parse the JSON data from the room property
            RobotData data = JsonUtility.FromJson<RobotData>(robotProperty);
            
            // Apply the robot configuration based on the parsed data
            ApplyRobotData(data);
        }
    }

    // Apply robot data without broadcasting changes
    private void ApplyRobotData(RobotData data)
    {
        // Set arms
        switch(data.arm)
        {
            case 0:
                SetCubeArmsInternal();
                break;
            case 1:
                SetCylinderArmsInternal();
                break;
            case 2:
                SetCapsuleArmsInternal();
                break;
        }

        // Set legs
        switch(data.leg)
        {
            case 0:
                SetCubeLegsInternal();
                break;
            case 1:
                SetCylinderLegsInternal();
                break;
            case 2:
                SetCapsuleLegsInternal();
                break;
        }

        // Set color
        SetColorInternal(new Color(data.color.r/255f, data.color.g/255f, data.color.b/255f));
    }


    private void SetColorInternal(Color color)
    {
        m_MeshRenderer.material.color = color;
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = color;
        }
    }

    private void SetCubeArmsInternal()
    {
        cylinderArms.SetActive(false);
        capsuleArms.SetActive(false);
        Color mainColor = m_MeshRenderer.material.color;
        foreach (Renderer renderer in cubeArms.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = mainColor;
        }
        cubeArms.SetActive(true);
    }

    private void SetCylinderArmsInternal()
    {
        cubeArms.SetActive(false);
        capsuleArms.SetActive(false);
        Color mainColor = m_MeshRenderer.material.color;
        foreach (Renderer renderer in cylinderArms.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = mainColor;
        }
        cylinderArms.SetActive(true);
    }

    private void SetCapsuleArmsInternal()
    {
        cubeArms.SetActive(false);
        cylinderArms.SetActive(false);
        Color mainColor = m_MeshRenderer.material.color;
        foreach (Renderer renderer in capsuleArms.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = mainColor;
        }
        capsuleArms.SetActive(true);
    }

    private void SetCubeLegsInternal()
    {
        cylinderLegs.SetActive(false);
        capsuleLegs.SetActive(false);
        Color mainColor = m_MeshRenderer.material.color;
        foreach (Renderer renderer in cubeLegs.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = mainColor;
        }
        cubeLegs.SetActive(true);
    }

    private void SetCylinderLegsInternal()
    {
        capsuleLegs.SetActive(false);
        cubeLegs.SetActive(false);
        Color mainColor = m_MeshRenderer.material.color;
        foreach (Renderer renderer in cylinderLegs.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = mainColor;
        }
        cylinderLegs.SetActive(true);
    }

    private void SetCapsuleLegsInternal()
    {
        cubeLegs.SetActive(false);
        cylinderLegs.SetActive(false);
        Color mainColor = m_MeshRenderer.material.color;
        foreach (Renderer renderer in capsuleLegs.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = mainColor;
        }
        capsuleLegs.SetActive(true);
    }

    // Public methods that update both local state and network state
    public void ChangColorRed()
    {
        Debug.Log("Changing color red");
        SetColorInternal(Color.red);
        SyncRobotState();
    }

    public void ChangColorGreen()
    {
        SetColorInternal(Color.green);
        SyncRobotState();
    }

    public void ChangColorBlue()
    {
        SetColorInternal(Color.blue);
        SyncRobotState();
    }

    public void ShowCubeArms()
    {
        SetCubeArmsInternal();
        SyncRobotState();
    }

    public void ShowCylinderArms()
    {
        SetCylinderArmsInternal();
        SyncRobotState();
    }

    public void ShowCapsuleArms()
    {
        SetCapsuleArmsInternal();
        SyncRobotState();
    }

    public void ShowCubeLegs()
    {
        SetCubeLegsInternal();
        SyncRobotState();
    }

    public void ShowCylinderLegs()
    {
        SetCylinderLegsInternal();
        SyncRobotState();
    }

    public void ShowCapsuleLegs()
    {
        SetCapsuleLegsInternal();
        SyncRobotState();
    }

    public void ResetRobot()
    {
        cubeArms.SetActive(false);
        cylinderArms.SetActive(false);
        capsuleArms.SetActive(false);
        cubeLegs.SetActive(false);
        cylinderLegs.SetActive(false);
        capsuleLegs.SetActive(false);
        Color mainColor = originalColor;
        m_MeshRenderer.material.color = mainColor;
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = mainColor;
        }
        logText.text = "";
        
        // Clear the robot state on the network
        if (roomClient && roomClient.Room != null)
        {
            roomClient.Room[networkIdString] = "";
        }
    }

    [Serializable]
    public class RobotData
    {
        public int arm;
        public int leg;
        public Color32 color;
    }

    // This method now collects the robot data and syncs it across the network
    public void GetRobotData()
    {
        RobotData data = new RobotData();
        bool isComplete = true;

        if (cubeArms.activeInHierarchy)
        {
            data.arm = 0;
        }
        else if (cylinderArms.activeInHierarchy)
        {
            data.arm = 1;
        }
        else if (capsuleArms.activeInHierarchy)
        {
            data.arm = 2;
        }
        else
        {
            isComplete = false;
        }

        if (cubeLegs.activeInHierarchy)
        {
            data.leg = 0;
        }
        else if (cylinderLegs.activeInHierarchy)
        {
            data.leg = 1;
        }
        else if (capsuleLegs.activeInHierarchy)
        {
            data.leg = 2;
        }
        else
        {
            isComplete = false;
        }

        if (!isComplete)
        {
            ShowErrorUI();
            return;
        }

        data.color = m_MeshRenderer.material.color;
        
        jsonString = JsonUtility.ToJson(data);
        Debug.Log("Robot Data in JSON: " + jsonString);
        robotCode = robotCode + 1;
        
        if (robotCode < 10)
        {
            logText.text = "Robot Build-Up Complete! Robot Code: 00" + robotCode;
        }
        else if (robotCode < 100)
        {
            logText.text = "Robot Build-Up Complete! Robot Code: 0" + robotCode;
        }
        else
        {
            logText.text = "Robot Build-Up Complete! Robot Code: " + robotCode;
        }

        // Sync the robot state to all clients
        SyncRobotState();
    }

    // Synchronize the current robot state to all clients
    private void SyncRobotState()
    {
        if (roomClient && roomClient.Room != null)
        {
            RobotData data = new RobotData();
            
            // Get arm type
            if (cubeArms.activeInHierarchy)
                data.arm = 0;
            else if (cylinderArms.activeInHierarchy)
                data.arm = 1;
            else if (capsuleArms.activeInHierarchy)
                data.arm = 2;
            else
                data.arm = -1;

            // Get leg type
            if (cubeLegs.activeInHierarchy)
                data.leg = 0;
            else if (cylinderLegs.activeInHierarchy)
                data.leg = 1;
            else if (capsuleLegs.activeInHierarchy)
                data.leg = 2;
            else
                data.leg = -1;

            // Get color
            data.color = m_MeshRenderer.material.color;

            // Convert to JSON and set as room property
            string json = JsonUtility.ToJson(data);
            roomClient.Room[networkIdString] = json;
        }
    }

    public void ShowErrorUI()
    {
        logText.text = "Robot incomplete! Please try again.";
    }
}