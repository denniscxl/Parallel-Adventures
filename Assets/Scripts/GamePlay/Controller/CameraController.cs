using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKBase;

public class CameraController : GKSingleton<CameraController>
{

    #region PublicField
    // 角色选择变更.
    public delegate void StateChanged(MachineStateID state);
    public event StateChanged OnStateChangedEvent = null;

    // 前一个状态.
    public MachineStateID LastState{get { return _lastState; }set { _lastState = value; }}
    #endregion

    #region PrivateField
    private GKCameraStateMachine _stateMachine = null;
    private Transform _focus = null;
    private Camera _mainCamera = null;
    private Transform _mainCameraTransform = null;
    private float _distance = 25;
    private int _zoomLevel = 0;
    private float _moveSpeed = 2;
    private bool _moving = false;
    private Vector3 _direction = Vector3.zero;
    private float _rotSpeed = 2;
    private Vector2 _targetPos = Vector2.zero;
    private MachineStateID _lastState = MachineStateID.BirdsEye;
    #endregion

    #region PublicMethod
    public void Init()
    {
        InitState();
    }

    public void Update()
    {
        if (null != _stateMachine)
        {
            _stateMachine.Update();
        }
    }
    // 设置和获取焦点对象方法.
    public void SetFocus(Transform t)
    {
        _focus = t;
    }
    public Transform GetFocus()
    {
        return _focus;
    }
    // 每次场景加载后重置摄像机状态.
    public void ResetMainCamera()
    {
        //Debug.Log("ResetMainCamera");
        _mainCamera = Camera.main;
        _mainCameraTransform = _mainCamera.transform;
        _stateMachine.GoToState(MachineStateID.BirdsEye);
    }
    public float GetMoveSpeed()
    {
        return _moveSpeed;
    }
    public float GetRotationSpeed()
    {
        return _rotSpeed;
    }
    // 获取主摄像机.
    public Camera GetMainCamera()
    {
        return _mainCamera;
    }
    public Transform GetMainCameraTransform()
    {
        return _mainCameraTransform;
    }
    // 设置获取摄像机距离.
    public void SetDistance(float distance)
    {
        _distance = distance;
    }
    public float GetDistance()
    {
        return _distance;
    }
    // 获取摄像机正方向.
    public Vector3 GetForward()
    {
        return _mainCameraTransform.forward;
    }
    // 设置与获取zoom值.
    public void SetZoomLevel(bool zoomIn)
    {
        int val = zoomIn ? 1 : -1;
        _zoomLevel += val;
        if (_zoomLevel < 0)
            _zoomLevel = 0;
        if (_zoomLevel > 3)
            _zoomLevel = 3;
    }
    public int GetZoomlevel()
    {
        return _zoomLevel;
    }
    public int GetZoomVal()
    {
        return _zoomLevel * 90;
    }
    // 只有鸟瞰模式下才可移动视口.
    public void MoveCamera(Vector3 v3)
    {
        if (MachineStateID.BirdsEye != _stateMachine.GetCurrentState().ID)
            return;
        _moving = true;
        _direction = v3;
    }

    public void MoveStop()
    {
        _moving = false;
    }

    // 获取摄像机移动目标位置.
    public Vector2 GetTargetPos()
    {
        if(_moving)
        {
            _targetPos.x += _direction.x * (_zoomLevel + 1) * 0.4f;
            _targetPos.y += _direction.y * (_zoomLevel + 1) * 0.4f;
        }
        return _targetPos;
    }
    public void SetTargetPos(float x, float y)
    {
        _targetPos.x = x;
        _targetPos.y = y;
    }
    public void ResetTargetPos()
    {
        _targetPos = Vector2.zero;
    }

    public GKCameraStateMachine GetStateMachine()
    {
        return _stateMachine;
    }

    public void ChangeState(MachineStateID state)
    {
        _lastState = _stateMachine.GetCurrentState().ID;
        _stateMachine.GoToState(state);
        if (null != OnStateChangedEvent)
            OnStateChangedEvent(state);
    }
    public MachineStateID GetCurrentState()
    {
        if (null == _stateMachine)
            return MachineStateID.BirdsEye;
        return _stateMachine.GetCurrentState().ID;
    }

    // 关卡结束后释放对应资源.
    public void ReleaseCamera()
    {
        Debug.Log("CameraController Release");
        OnStateChangedEvent = null;
        _focus = null;
        _mainCamera = null;
        _mainCameraTransform = null;
    }

    // 变更摄像机父节点.
    public void ResetCameraParent(bool bBridsEye)
    {
        if(bBridsEye)
        {
            GK.SetParent(_mainCamera.gameObject, LevelController.Instance().GetBridsEysRoot(), true);
        }
        else
        {
            if(null != _focus)
                GK.SetParent(_mainCamera.gameObject, _focus.gameObject, true);
        }
    }
    #endregion

    #region PrivateMethod
    private void InitState()
    {
        _stateMachine = new GKCameraStateMachine();
        _stateMachine.AddState(new GKCameraStopState(), true);
        _stateMachine.AddState(new GKCameraBirdsEyeState(), false);
        _stateMachine.AddState(new GKCameraFollowState(), false);
        _stateMachine.AddState(new GKCameraOverall(), false);
    }
    #endregion



}
