using System;
using Ubiq.Avatars;
using System.Collections.Generic;
using Ubiq.Spawning;
using UnityEngine;
using UnityEngine.Events;
using Ubiq.Messaging;
using Ubiq.Rooms;
using System.Collections;
using Ubiq.NetworkedBehaviour;

    public class avatarController : MonoBehaviour
    {
        [Header("Avatar Prefab Switching")]
       
        public AvatarManager avatarManager;
        public GameObject newAvatarPrefab;
        public NetworkedRobot networkedRobot;
        public PrefabCatalogue catalogue;
        public Transform spawnPoint;
        public GameObject xrRig;
        private bool shouldTeleport = false;

        [Header("Texture Options (URP BaseMap)")]
        public Texture2D[] baseMaps;

        [Header("Network Synchronization")]
        private RoomClient roomClient;           
        public NetworkScene networkScene;       
        
        [Header("Fixed Room Settings")]
        
        private bool isProcessingAvatarUpdate;  // 标记是否正在处理头像更新
        private string currentUpdateId;         // 当前更新操作的唯一标识符
        private bool isJoiningRoom;             // 标记是否正在加入房间
        private Guid targetRoomId;

        [Serializable]
        public class RobotMaterialData
        {
            public int leftArm;
            public int rightArm;
            public int body;
        }

        [Serializable]
        public class AvatarUpdateMessage
        {
            public string senderId;       // 发送者的唯一ID
            public string updateId;       // 更新操作的唯一标识符
            public string jsonString;     // 材质数据JSON
            public int prefabId;          // 预制体ID
            public string action;         // 操作类型: "update", "lock", "unlock"
        }

        private void Start()
        {
            NetworkedBehaviours.Register(this);
            // 确保找到所有必要组件
            if (networkScene == null)
            {
                networkScene = FindObjectOfType<NetworkScene>();
                Debug.Log("Network scene found: " + (networkScene != null));
            }
            
            if (roomClient == null)
            {
                roomClient = FindObjectOfType<RoomClient>();
                Debug.Log("Room client found: " + (roomClient != null));
            }
            
            if (networkedRobot == null)
            {
                networkedRobot = FindObjectOfType<NetworkedRobot>();
                Debug.Log("Networked robot found: " + (networkedRobot != null));
            }
            
            if (avatarManager == null)
            {
                avatarManager = FindObjectOfType<AvatarManager>();
                Debug.Log("Avatar manager found: " + (avatarManager != null));
            }
            
            targetRoomId = CreateRoom.gameRoomId;
            isProcessingAvatarUpdate = false;
            isJoiningRoom = false;
            
            // 添加此日志以便于调试
            Debug.Log($"Avatar controller initialized. Target room: {targetRoomId}");
        } 

        private void OnEnable()
        {
            var roomClients = FindObjectsByType<RoomClient>(FindObjectsSortMode.None);
            Debug.Log($"Found {roomClients.Length} RoomClient instances");
            
            foreach (var client in roomClients)
            {
                Debug.Log($"Adding OnJoinedRoom listener to RoomClient: {client.GetInstanceID()}");
                client.OnJoinedRoom.AddListener(OnJoinedRoom);
            }
            
            // 确保网络场景已设置
            if (networkScene == null)
            {
                networkScene = FindObjectOfType<NetworkScene>();
                Debug.Log("Network scene found in OnEnable: " + (networkScene != null));
            }
        }
        
        private void OnDisable()
        {
            var roomClients = FindObjectsByType<RoomClient>(FindObjectsSortMode.None);
            foreach (var client in roomClients)
            {
                client.OnJoinedRoom.RemoveListener(OnJoinedRoom);
            }
        }
// 添加此方法，在加入房间时请求所有现有对等方的头像数据
private void RequestExistingAvatars()
{
    Debug.Log("向房间对等方请求现有头像信息");
    
    if (networkScene == null || roomClient == null || !roomClient.JoinedRoom)
    {
        Debug.LogError("无法请求现有头像 - 网络未就绪");
        return;
    }
    
    AvatarUpdateMessage requestMsg = new AvatarUpdateMessage
    {
        senderId = roomClient.Me.uuid,
        updateId = Guid.NewGuid().ToString(),
        action = "request_avatar_info"
    };
    
    try
    {
        string json = JsonUtility.ToJson(requestMsg);
        NetworkId componentId = networkScene.Id;
        
        if (componentId == null)
        {
            Debug.LogError("找不到NetworkId组件！");
            return;
        }
        
        Debug.Log($"发送头像信息请求：{json}");
        networkScene.Send(componentId, json);
    }
    catch (Exception e)
    {
        Debug.LogError($"发送头像信息请求时出错：{e.Message}");
    }
}

// 修改OnJoinedRoom以包含对现有头像的请求
private void OnJoinedRoom(IRoom room) 
{
    Debug.Log($"已加入房间：{room.UUID}，目标房间：{targetRoomId}");
    isJoiningRoom = false;
    Debug.Log("ShouldTeleport: " + shouldTeleport);
    
    // 如果我们刚刚加入目标房间
    if (room.UUID == targetRoomId.ToString())
    {
        Debug.Log("已加入目标房间，正在更新头像");
        
        // 首先，请求房间中现有头像的信息
        RequestExistingAvatars();
        
        // 然后处理我们待处理的更新
        if (currentUpdateId != null)
        {
            Debug.Log("继续使用ID更新头像: " + currentUpdateId);
            ContinueAvatarUpdate();
        }
        else
        {
            Debug.Log("没有待处理的更新，只是刷新当前头像");
            
            // 如果我们有，则广播我们当前的头像信息
            if (networkedRobot != null && !string.IsNullOrEmpty(networkedRobot.jsonString))
            {
                int prefabId = GetPrefabId(avatarManager.avatarPrefab);
                if (prefabId != -1)
                {
                    SendUpdateMessage(networkedRobot.jsonString, prefabId);
                }
            }
        }
        
        // 更新我们的头像
        if (avatarManager != null)
        {
            avatarManager.UpdateAvatar();
            Debug.Log("已调用Avatar manager UpdateAvatar");
        }
        else
        {
            Debug.LogWarning("Avatar manager为null！");
        }
    }
    
    // 处理传送
    if (shouldTeleport && xrRig != null && spawnPoint != null)
    {
        xrRig.transform.position = spawnPoint.position;
        xrRig.transform.rotation = spawnPoint.rotation;
        Debug.Log("玩家已移至生成点");
        shouldTeleport = false;
    }
}

// 修改ProcessMessage以处理新的request_avatar_info操作
public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
{
    try
    {
        string jsonMessage = message.ToString();
        Debug.Log($"收到网络消息：{jsonMessage}");
        
        AvatarUpdateMessage updateMsg = JsonUtility.FromJson<AvatarUpdateMessage>(jsonMessage);
        
        // 不响应我们自己的消息
        if (updateMsg.senderId == roomClient.Me.uuid)
        {
            Debug.Log("忽略自己的消息");
            return;
        }

        switch (updateMsg.action)
        {
            case "update":
                // 收到头像更新消息
                Debug.Log($"收到带有JSON的头像更新消息：{updateMsg.jsonString}");
                ApplyAvatarUpdate(updateMsg.jsonString, updateMsg.prefabId);
                break;
                
            case "lock":
                // 房间因更新而被锁定
                Debug.Log("房间因头像更新而被锁定");
                isProcessingAvatarUpdate = true;
                break;
                
            case "unlock":
                // 更新后房间解锁
                Debug.Log("头像更新后房间解锁");
                isProcessingAvatarUpdate = false;
                break;
                
            case "request_avatar_info":
                // 有人在请求我们的头像信息
                Debug.Log($"收到来自{updateMsg.senderId}的头像信息请求");
                
                // 发送我们当前的头像信息
                if (networkedRobot != null && !string.IsNullOrEmpty(networkedRobot.jsonString) && 
                    avatarManager != null && avatarManager.avatarPrefab != null)
                {
                    int prefabId = GetPrefabId(avatarManager.avatarPrefab);
                    if (prefabId != -1)
                    {
                        Debug.Log("响应请求发送我们的头像信息");
                        SendUpdateMessage(networkedRobot.jsonString, prefabId);
                    }
                    else
                    {
                        Debug.LogWarning("无法发送头像信息 - 预制体不在目录中");
                    }
                }
                else
                {
                    Debug.LogWarning("无法发送头像信息 - 头像数据未就绪");
                }
                break;
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"处理网络消息时出错：{e.Message}\n{e.StackTrace}");
    }
}

        /// <summary>
        /// 按钮点击事件处理 - 切换到新房间并更新头像
        /// </summary>
        public void SwitchAndUpdateAvatarPrefab()
        {
            Debug.Log("SwitchAndUpdateAvatarPrefab called");
            
            if (isProcessingAvatarUpdate || isJoiningRoom)
            {
                Debug.LogWarning("Another action is in progress. Please wait.");
                return;
            }

            if (networkedRobot == null)
            {
                Debug.LogError("NetworkedRobot component is null!");
                return;
            }
            
            if (string.IsNullOrEmpty(networkedRobot.jsonString))
            {
                Debug.LogWarning("NetworkedRobot jsonString is null or empty.");
                return;
            }

            string jsonString = networkedRobot.jsonString;
            Debug.Log("Retrieved JSON: " + jsonString);
            
            try
            {
                RobotMaterialData data = JsonUtility.FromJson<RobotMaterialData>(jsonString);
                Debug.Log($"Parsed JSON data: body={data.body}, leftArm={data.leftArm}, rightArm={data.rightArm}");
                
                if (data.body < 0 || data.body >= baseMaps.Length)
                {
                    Debug.LogError($"Invalid body texture index: {data.body}. Available textures: {baseMaps.Length}");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing JSON: " + e.Message);
                return;
            }

            // 创建唯一更新ID并标记正在处理
            currentUpdateId = Guid.NewGuid().ToString();
            Debug.Log($"Created update ID: {currentUpdateId}");
            
            // 开始加入游戏房间的过程
            JoinGame();
        }
        
        /// <summary>
        /// 加入目标房间
        /// </summary>
        private void JoinGame()
        {
            shouldTeleport = true;
            Debug.Log($"Attempting to join room: {CreateRoom.gameRoomId}");
            
            isJoiningRoom = true;
            
            // 检查 roomClient 是否有效
            if (roomClient == null)
            {
                roomClient = FindObjectOfType<RoomClient>();
                if (roomClient == null)
                {
                    Debug.LogError("No RoomClient found!");
                    isJoiningRoom = false;
                    return;
                }
            }
            
            // 尝试加入房间
            try
            {
                roomClient.Join(CreateRoom.gameRoomId);
                Debug.Log($"Join request sent for room: {CreateRoom.gameRoomId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error joining room: {e.Message}");
                isJoiningRoom = false;
                return;
            }
            
            // 启动超时检查，防止加入房间卡住
            StartCoroutine(CheckRoomJoinTimeout());
        }

        /// <summary>
        /// 检查加入房间是否超时
        /// </summary>
        private IEnumerator CheckRoomJoinTimeout()
        {
            float timeoutSeconds = 10.0f;
            float elapsedTime = 0;
            
            while (isJoiningRoom && elapsedTime < timeoutSeconds)
            {
                yield return new WaitForSeconds(0.5f);
                elapsedTime += 0.5f;
            }
            
            if (isJoiningRoom)
            {
                Debug.LogError($"Joining room timed out after {timeoutSeconds} seconds");
                isJoiningRoom = false;
                currentUpdateId = null;
            }
        }
        
        /// <summary>
        /// 继续处理头像更新（在加入房间后）
        /// </summary>
        private void ContinueAvatarUpdate()
        {
            Debug.Log("ContinueAvatarUpdate called");
            
            if (string.IsNullOrEmpty(currentUpdateId))
            {
                Debug.LogWarning("No update ID available, aborting avatar update.");
                return;
            }
            
            // 通知其他用户锁定房间
            SendLockMessage();
            
            // 获取预制体ID
            int prefabId = GetPrefabId(newAvatarPrefab);
            if (prefabId == -1)
            {
                Debug.LogError("Prefab not found in catalogue. Attempting to add it.");
                AddDynamicPrefab(newAvatarPrefab);
                prefabId = GetPrefabId(newAvatarPrefab);
                
                if (prefabId == -1)
                {
                    Debug.LogError("Failed to add prefab to catalogue.");
                    return;
                }
            }
            
            // 处理头像更新（使用 networkedRobot 的 jsonString）
            ApplyAvatarUpdate(networkedRobot.jsonString, prefabId);
            
            // 发送更新消息给其他用户
            SendUpdateMessage(networkedRobot.jsonString, prefabId);
            
            // 处理完成后，解锁房间
            StartCoroutine(DelayedUnlock(1.0f));
        }
        
        /// <summary>
        /// 发送锁定房间的消息
        /// </summary>
        private void SendLockMessage()
        {
            AvatarUpdateMessage lockMsg = new AvatarUpdateMessage
            {
                senderId = roomClient.Me.uuid,
                updateId = currentUpdateId,
                action = "lock"
            };
            
            SendNetworkMessage(lockMsg);
            isProcessingAvatarUpdate = true;
        }
        
        /// <summary>
        /// 延迟解锁房间
        /// </summary>
        private IEnumerator DelayedUnlock(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            
            AvatarUpdateMessage unlockMsg = new AvatarUpdateMessage
            {
                senderId = roomClient.Me.uuid,
                updateId = currentUpdateId,
                action = "unlock"
            };
            
            SendNetworkMessage(unlockMsg);
            isProcessingAvatarUpdate = false;
            currentUpdateId = null;
        }
        
        /// <summary>
        /// 发送更新消息
        /// </summary>
        private void SendUpdateMessage(string jsonString, int prefabId)
        {
            AvatarUpdateMessage updateMsg = new AvatarUpdateMessage
            {
                senderId = roomClient.Me.uuid,
                updateId = currentUpdateId,
                jsonString = jsonString,
                prefabId = prefabId,
                action = "update"
            };
            
            SendNetworkMessage(updateMsg);
        }

        /// <summary>
        /// 发送网络消息
        /// </summary>
        private void SendNetworkMessage(AvatarUpdateMessage msg)
        {
            if (networkScene == null)
            {
                Debug.LogError("NetworkScene is null when attempting to send message!");
                return;
            }
            
            if (roomClient == null)
            {
                Debug.LogError("RoomClient is null when attempting to send message!");
                return;
            }
            
            if (!roomClient.JoinedRoom)
            {
                Debug.LogError("Not joined to a room when attempting to send message!");
                return;
            }
            
            try
            {
                string json = JsonUtility.ToJson(msg);
                NetworkId componentId = networkScene.Id;
                
                if (componentId == null)
                {
                    Debug.LogError("NetworkId component not found!");
                    return;
                }
                
                Debug.Log($"Sending network message: {json}");
                networkScene.Send(componentId, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sending network message: {e.Message}");
            }
        }
        private void EnsureCatalogueExists()
        {
            if (catalogue == null)
            {
                catalogue = ScriptableObject.CreateInstance<PrefabCatalogue>();
                Debug.Log("创建了新的PrefabCatalogue");
            }
            
            if (catalogue.prefabs == null)
            {
                catalogue.prefabs = new List<GameObject>();
                Debug.Log("为目录创建了新的prefabs列表");
            }
        }
        // 应用头像更新
        private void ApplyAvatarUpdate(string jsonString, int prefabId)
        {
            Debug.Log($"ApplyAvatarUpdate called with prefabId: {prefabId}, JSON: {jsonString}");
            
            if (string.IsNullOrEmpty(jsonString))
            {
                Debug.LogError("ApplyAvatarUpdate: JSON string is null or empty.");
                return;
            }
            
            // 获取预制体
            GameObject prefabToUse = GetPrefabFromId(prefabId);
            if (prefabToUse == null)
            {
                Debug.LogError($"Could not find prefab with ID {prefabId} in catalogue");
                return;
            }

            // 解析 JSON
            RobotMaterialData data;
            try
            {
                data = JsonUtility.FromJson<RobotMaterialData>(jsonString);
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing JSON in ApplyAvatarUpdate: " + e.Message);
                return;
            }

            // 克隆预制体
            GameObject clone = Instantiate(prefabToUse);
            NetworkId cloneNetworkId = networkScene.Id;

            // 查找渲染器
            SkinnedMeshRenderer[] allRenderers = clone.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (allRenderers.Length == 0)
            {
                Debug.LogWarning("No SkinnedMeshRenderer found in cloned prefab!");
                
                // 尝试查找普通 MeshRenderer
                MeshRenderer[] meshRenderers = clone.GetComponentsInChildren<MeshRenderer>(true);
                if (meshRenderers.Length > 0)
                {
                    Debug.Log($"Found {meshRenderers.Length} MeshRenderer components instead.");
                    
                    // 处理 MeshRenderer
                    foreach (var renderer in meshRenderers)
                    {
                        Debug.Log($"MeshRenderer name: {renderer.gameObject.name}");
                        
                        // 根据对象名称应用适当的贴图
                        if (renderer.gameObject.name.Contains("Body") || 
                            renderer.gameObject.name.Contains("Chassis") ||
                            renderer.gameObject.name == "MediumMechStrikerChassis")
                        {
                            if (data.body >= 0 && data.body < baseMaps.Length)
                            {
                                renderer.material.SetTexture("_BaseMap", baseMaps[data.body]);
                                Debug.Log($"Applied body texture {data.body} to {renderer.gameObject.name}");
                            }
                        }
                        else if (renderer.gameObject.name.Contains("LeftArm") || 
                                renderer.gameObject.name.Contains("Left"))
                        {
                            if (data.leftArm >= 0 && data.leftArm < baseMaps.Length)
                            {
                                renderer.material.SetTexture("_BaseMap", baseMaps[data.leftArm]);
                                Debug.Log($"Applied left arm texture {data.leftArm} to {renderer.gameObject.name}");
                            }
                        }
                        else if (renderer.gameObject.name.Contains("RightArm") || 
                                renderer.gameObject.name.Contains("Right"))
                        {
                            if (data.rightArm >= 0 && data.rightArm < baseMaps.Length)
                            {
                                renderer.material.SetTexture("_BaseMap", baseMaps[data.rightArm]);
                                Debug.Log($"Applied right arm texture {data.rightArm} to {renderer.gameObject.name}");
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("No renderers found in cloned prefab!");
                }
            }
            else
            {
                Debug.Log($"Found {allRenderers.Length} SkinnedMeshRenderer components.");
                
                SkinnedMeshRenderer cloneBodyRenderer = null;
                SkinnedMeshRenderer cloneLeftArmRenderer = null;
                SkinnedMeshRenderer cloneRightArmRenderer = null;
                
                // 查找特定的渲染器
                foreach (var renderer in allRenderers)
                {
                    Debug.Log($"Found renderer: {renderer.gameObject.name}");
                    
                    if (renderer.gameObject.name == "MediumMechStrikerChassis" || 
                        renderer.gameObject.name.Contains("Body") || 
                        renderer.gameObject.name.Contains("Chassis"))
                    {
                        cloneBodyRenderer = renderer;
                    }
                    else if (renderer.gameObject.name.Contains("LeftArm") || 
                            renderer.gameObject.name.Contains("Left"))
                    {
                        cloneLeftArmRenderer = renderer;
                    }
                    else if (renderer.gameObject.name.Contains("RightArm") || 
                            renderer.gameObject.name.Contains("Right"))
                    {
                        cloneRightArmRenderer = renderer;
                    }
                }
                
                // 更新身体材质
               if (cloneBodyRenderer != null && IsValidTextureIndex(data.body))
                    {
                        cloneBodyRenderer.material.SetTexture("_BaseMap", baseMaps[data.body]);
                        Debug.Log($"应用了身体纹理{data.body}");
                    }
                    else
                    {
                        Debug.LogWarning($"无效的身体纹理索引：{data.body}或身体渲染器为null。");
                    }
                
                // // 更新左手臂材质
                // if (cloneLeftArmRenderer != null && data.leftArm >= 0 && data.leftArm < baseMaps.Length)
                // {
                //     cloneLeftArmRenderer.material.SetTexture("_BaseMap", baseMaps[data.leftArm]);
                //     Debug.Log($"Clone's left arm BaseMap updated to index {data.leftArm}");
                // }
                
                // // 更新右手臂材质
                // if (cloneRightArmRenderer != null && data.rightArm >= 0 && data.rightArm < baseMaps.Length)
                // {
                //     cloneRightArmRenderer.material.SetTexture("_BaseMap", baseMaps[data.rightArm]);
                //     Debug.Log($"Clone's right arm BaseMap updated to index {data.rightArm}");
                // }
            }

            // 将更新后的预制体添加到目录中
            AddDynamicPrefab(clone);

            // 应用新的预制体
            if (avatarManager != null)
            {
                Debug.Log("Updating avatar with the new prefab");
                avatarManager.avatarCatalogue = catalogue;
                avatarManager.avatarPrefab = clone;
                avatarManager.UpdateAvatar();
                Debug.Log("Avatar prefab switched and updated successfully.");
            }
            else
            {
                Debug.LogError("AvatarManager is not assigned!");
                Destroy(clone);
            }
        }

        // 获取预制体在目录中的ID
        private int GetPrefabId(GameObject prefab)
        {
            if (catalogue == null)
            {
                Debug.LogError("PrefabCatalogue is null!");
                return -1;
            }
            
            if (catalogue.prefabs == null)
            {
                Debug.LogError("PrefabCatalogue.prefabs is null!");
                return -1;
            }
            
            if (prefab == null)
            {
                Debug.LogError("Prefab parameter is null!");
                return -1;
            }
            
            for (int i = 0; i < catalogue.prefabs.Count; i++)
            {
                if (catalogue.prefabs[i] == prefab)
                {
                    Debug.Log($"Found prefab at index {i}");
                    return i;
                }
            }
            
            Debug.LogWarning($"Prefab '{prefab.name}' not found in catalogue");
            return -1;
        }

        // 获取目录中指定索引的预制体
        private GameObject GetPrefabFromId(int id)
        {
            if (catalogue == null)
            {
                Debug.LogError("PrefabCatalogue is null!");
                return null;
            }
            
            if (catalogue.prefabs == null)
            {
                Debug.LogError("PrefabCatalogue.prefabs is null!");
                return null;
            }
            
            if (id >= 0 && id < catalogue.prefabs.Count)
            {
                Debug.Log($"Retrieved prefab at index {id}");
                return catalogue.prefabs[id];
            }
            
            Debug.LogError($"Invalid prefab index: {id}. Catalogue contains {catalogue.prefabs.Count} prefabs.");
            return null;
        }

       public void AddDynamicPrefab(GameObject dynamicPrefab)
        {
            if (dynamicPrefab == null)
            {
                Debug.LogError("不能将null预制体添加到目录中！");
                return;
            }
            
            EnsureCatalogueExists();
            
            // 为预制体创建唯一标识符
            // 可以考虑当前时间戳或其他数据组合作为标识符的一部分
            string prefabIdentifier = $"{dynamicPrefab.name}_{System.DateTime.Now.Ticks}";
            
            // 为了便于调试，可以给预制体一个更具描述性的名称
            dynamicPrefab.name = prefabIdentifier;
            
            // 清除列表中的任何null条目
            for (int i = catalogue.prefabs.Count - 1; i >= 0; i--)
            {
                if (catalogue.prefabs[i] == null)
                {
                    catalogue.prefabs.RemoveAt(i);
                    Debug.Log($"从目录中删除了索引{i}处的null条目");
                }
            }
            
            // 添加到目录（不再检查名称是否重复）
            catalogue.prefabs.Add(dynamicPrefab);
            Debug.Log($"将预制体 {dynamicPrefab.name} 添加到PrefabCatalogue的索引 {catalogue.prefabs.Count - 1} 处");

        
        }

        private bool IsValidTextureIndex(int index)
        {
            if (baseMaps == null)
            {
                Debug.LogError("baseMaps数组为null！");
                return false;
            }
            
            return index >= 0 && index < baseMaps.Length;
        }
        /// <summary>
        /// 测试方法：直接切换到指定的 prefab（未经过材质更新）
        /// </summary>
        public void TestSwitchAvatarPrefab()
        {
            Debug.Log("TestSwitchAvatarPrefab called");
            
            if (avatarManager == null)
            {
                Debug.LogError("AvatarManager is null!");
                return;
            }
            
            if (newAvatarPrefab == null)
            {
                Debug.LogError("newAvatarPrefab is null!");
                return;
            }
            
            avatarManager.avatarPrefab = newAvatarPrefab;
            avatarManager.UpdateAvatar();
            Debug.Log($"Avatar prefab switched to {newAvatarPrefab.name}");
        }
    }