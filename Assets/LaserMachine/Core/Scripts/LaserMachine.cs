using UnityEngine;                                           // 引入 Unity 引擎命名空间 (UnityEngine namespace)
using System.Collections;                                   // 引入 System.Collections 命名空间，用于集合 (Collections)
using System.Collections.Generic;                           // 引入 System.Collections.Generic 命名空间，用于泛型集合 (Generic Collections)
using UnityEngine.XR;                                       // 引入 XR 命名空间 (XR namespace)

namespace Lightbug.LaserMachine                             // 定义命名空间 Lightbug.LaserMachine
{
    public class LaserMachine : MonoBehaviour
    {
        // 定义激光元素结构体 (LaserElement structure)
        struct LaserElement
        {
            public Transform transform;                      // 变换组件 (Transform)
            public LineRenderer lineRenderer;                // 线渲染器 (LineRenderer)
            public GameObject sparks;                        // 火花效果 (Sparks effect)
            public bool impact;                              // 碰撞状态 (Impact flag)
        };

        // 激光元素列表 (List to store LaserElement)
        List<LaserElement> elementsList = new List<LaserElement>();

        [Header("External Data")]                         // 外部数据
        [SerializeField] LaserData m_data;                 // 外部激光数据 (Laser Data)

        [Tooltip("This variable is true by default, all the inspector properties will be overridden.")]
        [SerializeField] bool m_overrideExternalProperties = true;  // 是否覆盖外部属性 (Override inspector properties)

        [SerializeField] LaserProperties m_inspectorProperties = new LaserProperties();  // 检查器激光属性 (Inspector Laser Properties)

        LaserProperties m_currentProperties;                // 当前激光属性 (Current Properties)

        // 新增：判断枪是否握在右手 (New: Check if gun is held in right hand)
        [Header("Gun Settings")]
        [SerializeField] bool m_isGunHeldInRightHand = true; // 是否枪握在右手 (Is gun held in right hand)

        float m_time = 0;                                   // 计时器 (Timer)
        bool m_active = true;                               // 激活状态 (Active status)
        bool m_assignLaserMaterial;                         // 激光材质分配标记 (Assign laser material)
        bool m_assignSparks;                                // 火花特效分配标记 (Assign sparks effect)

        void OnEnable()
        {
            m_currentProperties = m_overrideExternalProperties ? m_inspectorProperties : m_data.m_properties;
            m_currentProperties.m_initialTimingPhase = Mathf.Clamp01(m_currentProperties.m_initialTimingPhase);
            m_time = m_currentProperties.m_initialTimingPhase * m_currentProperties.m_intervalTime;

            float angleStep = m_currentProperties.m_angularRange / m_currentProperties.m_raysNumber;

            m_assignSparks = m_data.m_laserSparks != null;
            m_assignLaserMaterial = m_data.m_laserMaterial != null;

            for (int i = 0; i < m_currentProperties.m_raysNumber; i++)
            {
                LaserElement element = new LaserElement();

                GameObject newObj = new GameObject("lineRenderer_" + i.ToString());

                if (m_currentProperties.m_physicsType == LaserProperties.PhysicsType.Physics2D)
                    newObj.transform.position = (Vector2)transform.position;
                else
                    newObj.transform.position = transform.position;

                newObj.transform.rotation = transform.rotation;
                newObj.transform.Rotate(Vector3.up, i * angleStep);
                newObj.transform.position += newObj.transform.forward * m_currentProperties.m_minRadialDistance;

                newObj.AddComponent<LineRenderer>();

                if (m_assignLaserMaterial)
                    newObj.GetComponent<LineRenderer>().material = m_data.m_laserMaterial;

                newObj.GetComponent<LineRenderer>().receiveShadows = false;
                newObj.GetComponent<LineRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                newObj.GetComponent<LineRenderer>().startWidth = m_currentProperties.m_rayWidth;
                newObj.GetComponent<LineRenderer>().useWorldSpace = true;
                newObj.GetComponent<LineRenderer>().SetPosition(0, newObj.transform.position);
                newObj.GetComponent<LineRenderer>().SetPosition(1, newObj.transform.position + transform.forward * m_currentProperties.m_maxRadialDistance);
                newObj.transform.SetParent(transform);

                if (m_assignSparks)
                {
                    GameObject sparks = Instantiate(m_data.m_laserSparks);
                    sparks.transform.SetParent(newObj.transform);
                    sparks.SetActive(false);
                    element.sparks = sparks;
                }

                element.transform = newObj.transform;
                element.lineRenderer = newObj.GetComponent<LineRenderer>();
                element.impact = false;

                elementsList.Add(element);
            }
        }

        void Update()
        {
            if (m_currentProperties.m_intermittent)
            {
                m_time += Time.deltaTime;
                if (m_time >= m_currentProperties.m_intervalTime)
                {
                    m_active = !m_active;
                    m_time = 0;
                    return;
                }
            }

            RaycastHit2D hitInfo2D;
            RaycastHit hitInfo3D;

            foreach (LaserElement element in elementsList)
            {
                if (m_currentProperties.m_rotate)
                {
                    if (m_currentProperties.m_rotateClockwise)
                        element.transform.RotateAround(transform.position, transform.up, Time.deltaTime * m_currentProperties.m_rotationSpeed);
                    else
                        element.transform.RotateAround(transform.position, transform.up, -Time.deltaTime * m_currentProperties.m_rotationSpeed);
                }

                if (m_active)
                {
                    element.lineRenderer.enabled = true;
                    element.lineRenderer.SetPosition(0, element.transform.position);

                    // 只检测激光射线碰撞和火花特效 (Check collision and sparks effect)
                    if (m_currentProperties.m_physicsType == LaserProperties.PhysicsType.Physics3D)
                    {
                        Physics.Linecast(
                            element.transform.position,
                            element.transform.position + element.transform.forward * m_currentProperties.m_maxRadialDistance,
                            out hitInfo3D,
                            m_currentProperties.m_layerMask
                        );

                        if (hitInfo3D.collider)
                        {
                            element.lineRenderer.SetPosition(1, hitInfo3D.point);
                            bool showSparks = false;

                            // 只有当枪握在右手时才检测输入 (Only check input if gun is held in right hand)
                            if (m_isGunHeldInRightHand)
                            {
                                if (Input.GetKey(KeyCode.X))
                                    showSparks = true;

                                List<InputDevice> devices = new List<InputDevice>();
                                InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
                                if (devices.Count > 0)
                                {
                                    InputDevice device = devices[0];
                                    bool triggerPressed = false;
                                    if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) && triggerPressed)
                                        showSparks = true;
                                }
                            }

                            if (showSparks)
                            {
                                element.sparks.transform.position = hitInfo3D.point;
                                element.sparks.transform.rotation = Quaternion.LookRotation(hitInfo3D.normal);
                            }
                            if (m_assignSparks)
                                element.sparks.SetActive(showSparks);
                        }
                        else
                        {
                            element.lineRenderer.SetPosition(1, element.transform.position + element.transform.forward * m_currentProperties.m_maxRadialDistance);
                            if (m_assignSparks)
                                element.sparks.SetActive(false);
                        }
                    }
                    else
                    {
                        hitInfo2D = Physics2D.Linecast(
                            element.transform.position,
                            element.transform.position + element.transform.forward * m_currentProperties.m_maxRadialDistance,
                            m_currentProperties.m_layerMask
                        );

                        if (hitInfo2D.collider)
                        {
                            element.lineRenderer.SetPosition(1, hitInfo2D.point);
                            bool showSparks = false;

                            // 只有当枪握在右手时才检测输入 (Only check input if gun is held in right hand)
                            if (m_isGunHeldInRightHand)
                            {
                                if (Input.GetKey(KeyCode.X))
                                    showSparks = true;

                                List<InputDevice> devices = new List<InputDevice>();
                                InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
                                if (devices.Count > 0)
                                {
                                    InputDevice device = devices[0];
                                    bool triggerPressed = false;
                                    if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) && triggerPressed)
                                        showSparks = true;
                                }
                            }

                            if (showSparks)
                            {
                                element.sparks.transform.position = hitInfo2D.point;
                                element.sparks.transform.rotation = Quaternion.LookRotation(hitInfo2D.normal);
                            }
                            if (m_assignSparks)
                                element.sparks.SetActive(showSparks);
                        }
                        else
                        {
                            element.lineRenderer.SetPosition(1, element.transform.position + element.transform.forward * m_currentProperties.m_maxRadialDistance);
                            if (m_assignSparks)
                                element.sparks.SetActive(false);
                        }
                    }
                }
                else
                {
                    element.lineRenderer.enabled = false;
                    if (m_assignSparks)
                        element.sparks.SetActive(false);
                }
            }
        }

        /*
        EXAMPLE : 
        // 示例函数，可在激光碰撞时执行特定操作
        void DoAction()
        {
            // 在此添加代码
        }
        */
    }
}
