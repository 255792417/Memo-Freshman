using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PointerManager : MonoBehaviour
{
    // 单例模式
    public static PointerManager Instance { get; private set; }

    // 当前鼠标状态
    public bool IsMouseDown { get; private set; }
    public Vector2 MousePosition { get; private set; }
    public Vector2 WorldPosition { get; private set; }
    public GameObject TopHitObject { get; private set; }
    public RaycastHit2D Hit2DInfo { get; private set; }
    public IDraggable CurrentDraggingObject { get; private set; }
    public bool IsOverUI { get; private set; }

    // 射线检测相关
    [SerializeField] private LayerMask raycastLayerMask = -1;
    [SerializeField] private bool detectUI = true;

    // 事件系统
    public delegate void MouseEventHandler(Vector2 screenPosition, Vector2 worldPosition, GameObject hitObject);
    public event MouseEventHandler OnMouseDown;
    public event MouseEventHandler OnMouseUp;
    public event MouseEventHandler OnMouseMove;

    private Camera mainCamera;
    private Vector2 lastMousePosition;
    private bool wasMouseDown;

    private void Awake()
    {
        // 单例设置
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        mainCamera = Camera.main;
    }

    private void Update()
    {
        // 更新鼠标位置
        UpdateMousePosition();

        // 检测点击状态
        CheckMouseState();

        // 执行射线检测
        PerformRaycast();
    }

    private void UpdateMousePosition()
    {
        lastMousePosition = MousePosition;
        MousePosition = Input.mousePosition;
        WorldPosition = mainCamera.ScreenToWorldPoint(MousePosition);
    }

    private void CheckMouseState()
    {
        wasMouseDown = IsMouseDown;
        IsMouseDown = Input.GetMouseButton(0);

        // 触发事件
        if (!wasMouseDown && IsMouseDown)
        {
            OnMouseDown?.Invoke(MousePosition, WorldPosition, TopHitObject);
        }
        else if (wasMouseDown && !IsMouseDown)
        {
            OnMouseUp?.Invoke(MousePosition, WorldPosition, TopHitObject);
        }

        // 鼠标移动事件
        if (Vector2.Distance(MousePosition, lastMousePosition) > 0.1f)
        {
            OnMouseMove?.Invoke(MousePosition, WorldPosition, TopHitObject);
        }
    }

    private void PerformRaycast()
    {
        TopHitObject = null;

        // 检测UI
        if (detectUI && EventSystem.current != null)
        {
            IsOverUI = EventSystem.current.IsPointerOverGameObject();
            if (IsOverUI)
            {
                TopHitObject = GetTopUIElement();
                return;
            }
        }

        // 2D物体射线检测
        Hit2DInfo = Physics2D.Raycast(WorldPosition, Vector2.zero, Mathf.Infinity, raycastLayerMask);
        if (Hit2DInfo.collider != null)
        {
            TopHitObject = Hit2DInfo.collider.gameObject;
        }
    }

    private GameObject GetTopUIElement()
    {
        // 获取当前鼠标下的UI元素
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = MousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0)
        {
            return results[0].gameObject;
        }

        return null;
    }

    // 注册和取消注册拖动对象的方法
    public bool RegisterDraggingObject(IDraggable draggable)
    {
        if (CurrentDraggingObject == null)
        {
            CurrentDraggingObject = draggable;
            return true;
        }
        return false;
    }

    public void UnregisterDraggingObject(IDraggable draggable)
    {
        if (CurrentDraggingObject == draggable)
        {
            CurrentDraggingObject = null;
        }
    }

    // 公共方法，供其他脚本使用
    public bool IsObjectUnderMouse(GameObject obj)
    {
        return TopHitObject == obj;
    }

    public bool IsLayerUnderMouse(int layerValue)
    {
        return TopHitObject != null && TopHitObject.layer == layerValue;
    }

    public bool IsTagUnderMouse(string tag)
    {
        return TopHitObject != null && TopHitObject.CompareTag(tag);
    }
}
