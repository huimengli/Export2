using Export.Attribute;
using Export.Enums;
using Export.Interface;
using Export.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/// <summary>
/// 玩家触控移动类
/// </summary>
public class PlayerUIMove : MonoBehaviour, IPlayerMove, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// 部件唯一标识符
    /// </summary>
    [ReadOnly]
    public string uuid;

    /// <summary>
    /// 控制器类型
    /// </summary>
    [ReadOnly]
    public ControlEnum control = ControlEnum.UI; // 控制器类型

    /// <summary>
    /// 静步移动速度
    /// </summary>
    [Header("移动速度设定")]
    [Range(0.1f, 5.0f)]
    public float quiteWalkSpeed = 2.0f; // 静步移动速度

    /// <summary>
    /// 玩家移动速度
    /// </summary>
    [Range(2f, 10f)]
    public float moveSpeed = 5.0f; // 玩家移动速度

    /// <summary>
    /// 冲刺速度
    /// </summary>
    [Range(5f, 20f)]
    public float runSpeed = 10.0f; // 冲刺速度

    /// <summary>
    /// 当前移动速度
    /// </summary>
    [ReadOnly]
    public float currentMoveSpeed = 0f; // 当前移动速度

    /// <summary>
    /// 摇杆图片
    /// </summary>
    [Header("摇杆设置")]
    public RawImage rawImage; // 移动摇杆图片

    /// <summary>
    /// 摇杆按钮图片
    /// </summary>
    public RawImage buttonImage; // 摇杆按钮图片

    /// <summary>
    /// 摇杆size
    /// </summary>
    [Range(300f, 800f)]
    public float rawImageSize = 500f;

    /// <summary>
    /// 摇杆按钮size
    /// </summary>
    [Range(50f, 300f)]
    public float buttonImageSize = 100f;

    /// <summary>
    /// 摇杆可移动半径
    /// </summary>
    [Range(10f, 200f)]
    public float buttonImageRadius = 150f; // 摇杆可移动半径

    /// <summary>
    /// 摇杆缩放大小
    /// </summary>
    [Range(0.5f, 3f)]
    public float rawImageZoom = 1.0f; // 摇杆缩放大小

    /// <summary>
    /// 摇杆定位
    /// </summary>
    public Vector2 rawImagePosition = Vector2.zero; // 摇杆定位

    /// <summary>
    /// 摇杆位置
    /// corners数组依次存储左下、左上、右上、右下角坐标
    /// </summary>
    public Vector3[] corners;

    /// <summary>
    /// 跑步按钮设置
    /// </summary>
    [Header("跑步设置")]
    public RawImage runImage;

    /// <summary>
    /// 摇杆按钮位置
    /// </summary>
    [ReadOnly]
    public Vector2 buttonImagePosition = Vector2.zero; // 摇杆按钮位置

    /// <summary>
    /// 是否使用Unity拖拽事件系统
    /// </summary>
    [Header("拖拽设置")]
    [SerializeField]
    private bool useDragEvents = true; // 是否使用Unity拖拽事件系统
    /// <summary>
    /// 是否正在拖拽
    /// </summary>
    private bool isDragging = false; // 是否正在拖拽
    /// <summary>
    /// 拖拽起始位置
    /// </summary>
    private Vector2 dragStartPos; // 拖拽起始位置
    /// <summary>
    /// 摇杆中心点屏幕坐标
    /// </summary>
    private Vector2 rawImageCenter; // 摇杆中心点屏幕坐标
    /// <summary>
    /// 是否在跑步
    /// </summary>
    private bool isRunning = false; // 是否在跑步
    /// <summary>
    /// 跑步按钮是否被按下
    /// </summary>
    private bool isRunButtonPressed = false; // 新增字段

    /// <summary>
    /// 旋转平滑度
    /// </summary>
    [Header("旋转设置")]
    [Range(0.1f, 20f)]
    public float rotationSmoothness = 5.0f;

    /// <summary>
    /// 地面检测距离（从玩家底部向下检测的距离）
    /// </summary>
    [Header("物理设置")]
    public float groundCheckDistance = 0.2f;
    /// <summary>
    /// 地面层级掩码（用于射线检测识别哪些物体是地面）
    /// </summary>
    public LayerMask groundLayer;
    /// <summary>
    /// 是否在地面上（检测玩家当前是否接触地面）
    /// </summary>
    private bool isGrounded;

    public Vector3 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public Quaternion Rotation
    {
        get { return transform.rotation; }
        set { transform.rotation = value; }
    }

    public string UUID
    {
        get { return uuid; }
    }

    public float MoveSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = value; }
    }

    public float QuiteWalkSpeed
    {
        get { return quiteWalkSpeed; }
        set { quiteWalkSpeed = value; }
    }

    public float RunSpeed
    {
        get { return runSpeed; }
        set { runSpeed = value; }
    }

    public float CurrentMoveSpeed
    {
        get { return currentMoveSpeed; }
        set { currentMoveSpeed = value; }
    }

    public Rigidbody player
    {
        get
        {
            var ret = gameObject.GetComponent<Rigidbody>();
            if (ret == null)
            {
                ret = gameObject.AddComponent<Rigidbody>();
            }
            return ret;
        }
    }

    private void Awake()
    {
        uuid = Item.NewUUID(); // 生成唯一标识符
        buttonImagePosition = Vector2.zero; // 初始化摇杆按钮位置
    }

    private void Start()
    {
        // 设置移动速度
        currentMoveSpeed = moveSpeed;

        // 根据缩放大小修改摇杆
        rawImage.rectTransform.sizeDelta = Vector2.one * rawImageSize;
        buttonImage.rectTransform.sizeDelta = Vector2.one * buttonImageSize;

        rawImage.rectTransform.localScale = Vector3.one * rawImageZoom;
        buttonImage.rectTransform.localScale = Vector3.one * rawImageZoom;

        // 修改摇杆位置
        rawImage.rectTransform.anchoredPosition = Vector2.one * rawImageSize * rawImageZoom / 2 + rawImagePosition;

        // 设置按钮方法
        var rawButton = rawImage.gameObject.GetComponent<Button>();
        if (rawButton == null)
        {
            rawImage.gameObject.AddComponent<Button>();
            rawButton = rawImage.gameObject.GetComponent<Button>();
        }
        var boxCollider2D = rawImage.gameObject.GetComponent<BoxCollider2D>();
        if (boxCollider2D == null)
        {
            rawImage.gameObject.AddComponent<BoxCollider2D>();
            boxCollider2D = rawImage.gameObject.GetComponent<BoxCollider2D>();
        }

        // 添加按钮数据
        RectTransform buttonRect = rawImage.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        buttonRect.GetWorldCorners(corners);
        Vector2 localPos = buttonRect.anchoredPosition;

        // 获取摇杆中心点世界坐标
        rawImageCenter = RectTransformUtility.WorldToScreenPoint(
            null,
            rawImage.rectTransform.position
        );

        // 跑步设置
        var runButton = runImage.gameObject.GetComponent<Button>();
        if (runButton == null)
        {
            runButton = runImage.gameObject.AddComponent<Button>();
        }

        // 添加事件触发器组件
        var runEventTrigger = runImage.gameObject.GetComponent<EventTrigger>();
        if (runEventTrigger == null)
        {
            runEventTrigger = runImage.gameObject.AddComponent<EventTrigger>();
        }

        // 创建按下事件
        var pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { OnRunButtonPressed(); });
        runEventTrigger.triggers.Add(pointerDownEntry);

        // 创建抬起事件
        var pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { OnRunButtonReleased(); });
        runEventTrigger.triggers.Add(pointerUpEntry);

        // 添加刚体物理设置
        player.freezeRotation = true; // 防止物理旋转导致抖动
        player.collisionDetectionMode = CollisionDetectionMode.Continuous; // 避免穿墙
    }

    /// <summary>
    /// 当跑步按钮按下时调用
    /// </summary>
    private void OnRunButtonPressed()
    {
        isRunButtonPressed = true;
        UpdateRunningState();
    }

    /// <summary>
    /// 当跑步按钮释放时调用
    /// </summary>
    private void OnRunButtonReleased()
    {
        isRunButtonPressed = false;
        UpdateRunningState();
    }

    /// <summary>
    /// 更新跑步状态
    /// </summary>
    private void UpdateRunningState()
    {
        // 只有当摇杆有输入时才允许跑步
        isRunning = isRunButtonPressed && buttonImagePosition.magnitude > 0.01f;
    }

    /// <summary>
    /// 根据移动方向更新角色朝向
    /// </summary>
    private void UpdateRotation(Vector3 moveDirection)
    {
        // 计算目标朝向角度（俯视角）
        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

        // 平滑旋转
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmoothness * Time.deltaTime
        );
    }

    /// <summary>
    /// 处理拖拽开始事件
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!useDragEvents) return;

        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform,
            eventData.position,
            null,
            out dragStartPos
        );
    }

    /// <summary>
    /// 处理拖拽事件
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!useDragEvents || !isDragging) return;

        // 获取触摸点在摇杆局部坐标系中的位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform,
            eventData.position,
            null,
            out Vector2 localPoint
        );

        // 计算摇杆偏移向量
        Vector2 direction = localPoint - dragStartPos;

        // 限制摇杆移动半径
        if (direction.magnitude > buttonImageRadius)
        {
            direction = direction.normalized * buttonImageRadius;
        }

        // 更新按钮位置
        buttonImage.rectTransform.anchoredPosition = direction;
        buttonImagePosition = direction / buttonImageRadius;
    }

    /// <summary>
    /// 处理拖拽结束事件
    /// </summary>
    public void OnEndDrag(PointerEventData eventData) => ResetJoystick();
    public void OnPointerUp(PointerEventData eventData) => ResetJoystick();

    /// <summary>
    /// 重置摇杆位置
    /// </summary>
    private void ResetJoystick()
    {
        if (useDragEvents) return;

        isDragging = false;
        buttonImage.rectTransform.anchoredPosition = Vector2.zero;
        buttonImagePosition = Vector2.zero;
    }

    private void Update()
    {
        // 备用触摸检测（不使用事件系统时）
        if (!useDragEvents)
        {
            HandleTouchInput();
        }

        // 移除这里的移动旋转变更
    }

    private void FixedUpdate() // 物理更新
    {
        CheckGrounded();
        HandleMovement();
    }

    private void CheckGrounded()
    {
        // 射线检测是否在地面
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance + 0.1f,
            groundLayer
        );
    }

    private void HandleMovement()
    {
        if (!isDragging || buttonImagePosition.magnitude < 0.01f)
        {
            // 无输入时保持垂直速度（重力），水平速度归零
            player.velocity = new Vector3(0, player.velocity.y, 0);
            return;
        }

        // 计算移动方向（XZ平面）
        Vector3 moveDirection = new Vector3(
            buttonImagePosition.x,
            0,
            buttonImagePosition.y
        ).normalized;

        // 更新跑步状态
        UpdateRunningState();

        // 应用速度（保留Y轴重力）
        Vector3 targetVelocity = moveDirection * currentMoveSpeed;
        targetVelocity.y = player.velocity.y; // 保持垂直速度

        // 平滑过渡速度
        player.velocity = Vector3.Lerp(
            player.velocity,
            targetVelocity,
            rotationSmoothness * Time.fixedDeltaTime
        );

        // 更新旋转
        UpdateRotation(moveDirection);
    }

    /// <summary>
    /// 处理触摸输入（备用方案）
    /// </summary>
    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (IsTouchOnJoystick(touch.position))
                    {
                        isDragging = true;
                        dragStartPos = touch.position;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        UpdateJoystickPosition(touch.position);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    ResetJoystick();
                    break;
            }
        }
        // 编辑器鼠标支持
        else if (Input.GetMouseButtonDown(0))
        {
            if (IsTouchOnJoystick(Input.mousePosition))
            {
                isDragging = true;
                dragStartPos = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateJoystickPosition(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            ResetJoystick();
        }
    }

    /// <summary>
    /// 检测触摸点是否在摇杆上
    /// </summary>
    private bool IsTouchOnJoystick(Vector2 touchPos)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform,
            touchPos,
            null,
            out localPoint
        );

        // 计算触摸点与摇杆中心的距离
        float distance = Vector2.Distance(localPoint, Vector2.zero);
        return distance <= buttonImageRadius;
    }

    /// <summary>
    /// 更新摇杆位置（基于屏幕坐标）
    /// </summary>
    private void UpdateJoystickPosition(Vector2 touchPos)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform,
            touchPos,
            null,
            out localPoint
        );

        Vector2 direction = localPoint;
        if (direction.magnitude > buttonImageRadius)
        {
            direction = direction.normalized * buttonImageRadius;
        }

        buttonImage.rectTransform.anchoredPosition = direction;
        buttonImagePosition = direction / buttonImageRadius;
    }

    /// <summary>
    /// 计算移动向量
    /// </summary>
    public Vector3 Move()
    {
        if (!isDragging || buttonImagePosition.magnitude < 0.01f)
        {
            isRunning = false; // 没有移动时重置跑步状态
            return Vector3.zero;
        }

        // 更新跑步状态（处理运行时摇杆输入变化）
        UpdateRunningState();

        // 计算移动方向（XZ平面）
        // 因为buttonImagePosition已经是归一化向量，所以可以直接作为方向向量
        // 根据跑步状态和摇杆幅度设置速度
        if (isRunning)
        {
            currentMoveSpeed = runSpeed;
        }
        else if (buttonImagePosition.magnitude > 0.5f)
        {
            currentMoveSpeed = moveSpeed;
        }
        else
        {
            currentMoveSpeed = quiteWalkSpeed;
        }
        // 同时其长度代表了输入强度（0-1）
        Vector3 moveDirection = new Vector3(
            buttonImagePosition.x,
            0,
            buttonImagePosition.y
        );

        // 修改点：直接使用buttonImagePosition的magnitude作为速度因子
        // 不再需要额外的速度因子计算
        return moveDirection * (currentMoveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 获取当前触摸点（世界坐标）
    /// </summary>
    private Vector3 TouchPoint()
    {
        if (!isDragging) return Vector3.zero;

        Vector3 touchPos = Input.touchCount > 0
            ? (Vector3)Input.GetTouch(0).position
            : Input.mousePosition;

        touchPos.z = 10; // 设置合适的Z轴深度
        return Camera.main.ScreenToWorldPoint(touchPos);
    }
}
