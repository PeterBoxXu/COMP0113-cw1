using UnityEngine;                                           // 引入 Unity 引擎命名空间 (UnityEngine namespace)
using System.Collections;                                   // 引入 System.Collections 命名空间，用于集合 (Collections)
using System.Collections.Generic;                           // 引入 System.Collections.Generic 命名空间，用于泛型集合 (Generic Collections)
using UnityEngine.XR;  // 引入 XR 命名空间 (XR namespace)

namespace Lightbug.LaserMachine                             // 定义命名空间 Lightbug.LaserMachine
{
    // 定义 LaserMachine 类，继承自 MonoBehaviour (Unity 脚本基类)
    public class LaserMachine : MonoBehaviour
    {

        // 定义激光元素结构体，包含激光所需的各个组件 (LaserElement structure)
        struct LaserElement
        {
            public Transform transform;                      // 存储对象的 Transform 组件 (变换组件)
            public LineRenderer lineRenderer;                // 存储 LineRenderer 组件，用于绘制激光 (线渲染器)
            public GameObject sparks;                        // 存储火花特效的 GameObject (火花效果)
            public bool impact;                              // 标记是否发生碰撞 (碰撞状态)
        };

        // 创建激光元素列表，用于存储所有激光的元素 (列表存储 LaserElement)
        List<LaserElement> elementsList = new List<LaserElement>();

        [Header("External Data")]                         // Inspector 面板显示的标题 "External Data"（外部数据）

        [SerializeField] LaserData m_data;                   // 序列化 LaserData 类型数据，存储外部激光数据 (外部数据)

        [Tooltip("This variable is true by default, all the inspector properties will be overridden.")]
        [SerializeField] bool m_overrideExternalProperties = true;  // 序列化布尔变量，默认 true，表示会覆盖 Inspector 中的属性 (是否覆盖外部属性)

        [SerializeField] LaserProperties m_inspectorProperties = new LaserProperties();  // 序列化 LaserProperties 类型数据，来自 Inspector 的激光属性 (检查器属性)

        LaserProperties m_currentProperties;                // 当前使用的激光属性 (当前属性)

        float m_time = 0;                                   // 定义时间变量，用于计时 (计时器)
        bool m_active = true;                               // 标记激光是否激活 (激活状态)
        bool m_assignLaserMaterial;                         // 标记是否分配激光材质 (激光材质分配)
        bool m_assignSparks;                                // 标记是否分配火花特效 (火花特效分配)

        // 当脚本启用时调用 (OnEnable 是 Unity 生命周期函数)
        void OnEnable()
        {
            // 根据是否覆盖外部属性选择激光属性 (选择属性来源)
            m_currentProperties = m_overrideExternalProperties ? m_inspectorProperties : m_data.m_properties;

            // 限制初始时序阶段在 0 到 1 之间 (Clamp 初始时序阶段)
            m_currentProperties.m_initialTimingPhase = Mathf.Clamp01(m_currentProperties.m_initialTimingPhase);
            // 根据初始时序阶段计算初始时间 (初始化计时器)
            m_time = m_currentProperties.m_initialTimingPhase * m_currentProperties.m_intervalTime;

            // 计算每条射线之间的角度步长 (计算角度步长)
            float angleStep = m_currentProperties.m_angularRange / m_currentProperties.m_raysNumber;

            // 判断是否分配火花特效，依据外部数据是否为空 (火花特效是否可用)
            m_assignSparks = m_data.m_laserSparks != null;
            // 判断是否分配激光材质，依据外部数据是否为空 (激光材质是否可用)
            m_assignLaserMaterial = m_data.m_laserMaterial != null;

            // 根据射线数量循环创建激光元素 (创建每个激光元素)
            for (int i = 0; i < m_currentProperties.m_raysNumber; i++)
            {
                // 声明局部变量 element 来存储激光元素 (激光元素实例)
                LaserElement element = new LaserElement();

                // 创建一个新的空 GameObject 用于存放 LineRenderer (新建空对象)
                GameObject newObj = new GameObject("lineRenderer_" + i.ToString());

                // 根据物理类型设置新对象的位置 (Physics2D 或 3D)
                if (m_currentProperties.m_physicsType == LaserProperties.PhysicsType.Physics2D)
                    newObj.transform.position = (Vector2)transform.position; // 2D 位置
                else
                    newObj.transform.position = transform.position;           // 3D 位置

                newObj.transform.rotation = transform.rotation;  // 设置新对象的旋转与当前对象一致 (设置旋转)
                newObj.transform.Rotate(Vector3.up, i * angleStep);  // 围绕 Y 轴旋转一定角度 (旋转)
                // 移动新对象到最小半径位置 (沿射线方向移动)
                newObj.transform.position += newObj.transform.forward * m_currentProperties.m_minRadialDistance;

                // 为新对象添加 LineRenderer 组件 (添加线渲染器)
                newObj.AddComponent<LineRenderer>();

                // 如果激光材质可用，则分配材质 (分配材质)
                if (m_assignLaserMaterial)
                    newObj.GetComponent<LineRenderer>().material = m_data.m_laserMaterial;

                // 设置 LineRenderer 属性，不接收阴影 (关闭阴影)
                newObj.GetComponent<LineRenderer>().receiveShadows = false;
                newObj.GetComponent<LineRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                // 设置激光宽度 (射线宽度)
                newObj.GetComponent<LineRenderer>().startWidth = m_currentProperties.m_rayWidth;
                // 设置 LineRenderer 使用世界坐标系 (World Space)
                newObj.GetComponent<LineRenderer>().useWorldSpace = true;
                // 设置 LineRenderer 起始点位置为当前新对象的位置 (设置起点)
                newObj.GetComponent<LineRenderer>().SetPosition(0, newObj.transform.position);
                // 设置 LineRenderer 终点位置为新对象正前方延伸至最大半径距离 (设置终点)
                newObj.GetComponent<LineRenderer>().SetPosition(1, newObj.transform.position + transform.forward * m_currentProperties.m_maxRadialDistance);
                // 将新对象设置为当前对象的子物体 (设置父级)
                newObj.transform.SetParent(transform);

                // 如果火花特效可用，则实例化火花特效 (实例化火花)
                if (m_assignSparks)
                {
                    GameObject sparks = Instantiate(m_data.m_laserSparks);
                    sparks.transform.SetParent(newObj.transform);  // 将火花设为新对象的子物体
                    sparks.SetActive(false);                         // 初始时不激活火花特效
                    element.sparks = sparks;                         // 存储火花特效引用到激光元素
                }

                // 存储新对象的 Transform 和 LineRenderer 到激光元素中 (保存组件引用)
                element.transform = newObj.transform;
                element.lineRenderer = newObj.GetComponent<LineRenderer>();
                element.impact = false;   // 初始化碰撞标记为 false

                // 将激光元素加入列表中 (添加到集合)
                elementsList.Add(element);
            }

        }

        // 每帧更新调用 (Update 函数)
        void Update()
        {

            // 如果激光设置为间歇性 (间歇性激光)
            if (m_currentProperties.m_intermittent)
            {
                m_time += Time.deltaTime;  // 累加经过的时间 (增量时间)

                // 当累积时间大于间隔时间时 (检测间隔)
                if (m_time >= m_currentProperties.m_intervalTime)
                {
                    m_active = !m_active;  // 切换激光的激活状态 (开关切换)
                    m_time = 0;            // 重置计时器
                    return;                // 结束本帧 Update，不执行后续射线检测
                }
            }

            // 声明 2D 物理检测变量 (RaycastHit2D)
            RaycastHit2D hitInfo2D;
            // 声明 3D 物理检测变量 (RaycastHit)
            RaycastHit hitInfo3D;

            // 遍历所有激光元素 (遍历每个激光)
            foreach (LaserElement element in elementsList)
            {
                // 如果设置了旋转 (旋转激光)
                if (m_currentProperties.m_rotate)
                {
                    // 根据是否顺时针旋转来选择旋转方向 (旋转方向)
                    if (m_currentProperties.m_rotateClockwise)
                        element.transform.RotateAround(transform.position, transform.up, Time.deltaTime * m_currentProperties.m_rotationSpeed);    // 围绕中心点旋转 (顺时针旋转)
                    else
                        element.transform.RotateAround(transform.position, transform.up, -Time.deltaTime * m_currentProperties.m_rotationSpeed);   // 逆时针旋转
                }

                // 如果激光处于激活状态 (激光激活)
                if (m_active)
                {
                    element.lineRenderer.enabled = true;  // 启用 LineRenderer
                    // 更新 LineRenderer 起始点为当前激光元素位置
                    element.lineRenderer.SetPosition(0, element.transform.position);

                    // 判断物理检测类型：3D
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
                            // 设置激光终点为碰撞点 (Set the end position of the laser to the collision point)
                            element.lineRenderer.SetPosition(1, hitInfo3D.point);

                            bool showSparks = false;

                            // 检查键盘 X 键 (Check if keyboard X key is pressed)
                            if (Input.GetKey(KeyCode.X))
                                showSparks = true;

                            // 检查手柄右侧扳机 (Check if XR right-hand trigger is pressed)
                            List<InputDevice> devices = new List<InputDevice>();
                            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
                            if (devices.Count > 0)
                            {
                                InputDevice device = devices[0];
                                bool triggerPressed = false;
                                if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) && triggerPressed)
                                    showSparks = true;
                            }

                            // 如果任一输入触发，则更新火花位置和朝向 (If triggered, update sparks position and rotation)
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
                            // 未检测到碰撞时，延伸激光至最大距离并关闭火花 (If no collision, extend the laser and disable sparks)
                            element.lineRenderer.SetPosition(1, element.transform.position + element.transform.forward * m_currentProperties.m_maxRadialDistance);
                            if (m_assignSparks)
                                element.sparks.SetActive(false);
                        }

                    }
                    else
                    {
                        // 如果物理类型为 2D，进行 2D 线检测 (Physics2D.Linecast)
                        hitInfo2D = Physics2D.Linecast(
                            element.transform.position,
                            element.transform.position + element.transform.forward * m_currentProperties.m_maxRadialDistance,
                            m_currentProperties.m_layerMask
                        );

                        // 如果检测到碰撞 (2D 碰撞)
                        if (hitInfo2D.collider)
                        {
                            // 设置 LineRenderer 的终点为碰撞点 (Set the end position to the collision point)
                            element.lineRenderer.SetPosition(1, hitInfo2D.point);

                            bool showSparks = false;

                            // 检查键盘 X 键 (Check if keyboard X key is pressed)
                            if (Input.GetKey(KeyCode.X))
                                showSparks = true;

                            // 检查手柄右侧扳机 (Check if XR right-hand trigger is pressed)
                            List<InputDevice> devices = new List<InputDevice>();
                            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
                            if (devices.Count > 0)
                            {
                                InputDevice device = devices[0];
                                bool triggerPressed = false;
                                if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) && triggerPressed)
                                    showSparks = true;
                            }

                            // 如果任一输入触发，则更新火花位置和朝向 (If triggered, update sparks position and rotation)
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
                            // 未检测到碰撞时，将终点设置为最大距离并关闭火花 (If no collision, extend the laser and disable sparks)
                            element.lineRenderer.SetPosition(1, element.transform.position + element.transform.forward * m_currentProperties.m_maxRadialDistance);
                            if (m_assignSparks)
                                element.sparks.SetActive(false);
                        }


                        // 根据是否检测到碰撞来激活或关闭火花特效 (火花特效的显示)
                        if (m_assignSparks)
                            element.sparks.SetActive(hitInfo2D.collider != null);
                    }
                }
                else
                {
                    // 如果激光未激活，关闭 LineRenderer (关闭激光显示)
                    element.lineRenderer.enabled = false;

                    // 同时关闭火花特效 (隐藏火花)
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
