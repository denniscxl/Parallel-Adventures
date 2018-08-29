using UnityEngine;
using UnityEngine.EventSystems;
using GKBase;
using GKUI;

public class UIVirtualJoyStick : SingletonUIBase<UIVirtualJoyStick>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public GameObject Root;
        public GameObject Pad;
    }
    #endregion

    #region PublicField
    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    //最大的拖动距离.
    private float _maxDragDistance = 150f;
    private Transform _padTransform;
    //虚拟摇杆的方向.
    private Vector3 _direction;
    private Vector3 _normalized;
    private float _lastTime = 0;
    private readonly float _pressTime = 0.2f;
    private bool _show = false;
    private bool _press = false;
    private Transform _rootTransform = null;
    #endregion

    #region PublicMethod
    public void Show(bool show)
    {
        _show = show;
        _rootTransform.position = Input.mousePosition;
    }

    // 清空状态.
    public void Reset()
    {
        _press = false;
        _lastTime = Time.time;
    }
    #endregion

    #region PrivateMethod
    private void Start()
    {
        Serializable();
        InitListener();
        Init();
    }

    private void Serializable()
    {
        GK.FindControls(this.gameObject, ref m_ctl);
    }

    private void InitListener()
    {
        CameraController.Instance().OnStateChangedEvent += CameraStateChange;
    }

    private void Init()
    {
        _rootTransform = m_ctl.Root.transform;
        _padTransform = m_ctl.Pad.transform;
    }

    private void OnDestroy()
    {
        CameraController.Instance().OnStateChangedEvent -= CameraStateChange;
    }

    private void Update()
    {
        if(!EventSystem.current.IsPointerOverGameObject() && null !=  CameraController.Instance() && CameraController.Instance().GetCurrentState() == MachineStateID.BirdsEye)
        {
            // 如果按下不进行更新时间.
            if (!_press)
                _lastTime = Time.time;

            if(Input.GetMouseButtonDown(0))
            {
                _press = true;
            }
  
            if (Input.GetMouseButtonUp(0))
            {
                CameraStateChange(MachineStateID.Follow);
            }

            // 如果没有显示并且长按, 显示虚拟摇杆.
            if (!_show && _press && Time.time - _lastTime > _pressTime)
            {
                Show(true);
            }

            if(_show) 
            {
                OnDrag();
            }
        }
    }

    //拖拽中的时候.
    private void OnDrag()
    {
        _padTransform.position = Input.mousePosition;
        _direction = _padTransform.localPosition - Vector3.zero;
        _normalized = _direction.normalized;
        if (Vector3.Distance(Vector3.zero, _padTransform.localPosition) > _maxDragDistance)
        {
            _padTransform.localPosition = _normalized * _maxDragDistance;
        }
        CameraController.Instance().MoveCamera(_normalized);
    }

    //拖拽结束的时候.
    private void OnEndDrag()
    {
        _padTransform.localPosition = Vector3.zero;
        CameraController.Instance().MoveStop();
    } 

    private void CameraStateChange(MachineStateID state)
    {
        _press = false;
        if (_show)
        {
            OnEndDrag();
            Show(false);
        }
    }
    #endregion
}
