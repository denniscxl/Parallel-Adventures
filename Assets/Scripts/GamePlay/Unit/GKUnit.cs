using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using GKBase;
using GKData;
using GKMemory;
using GKMap;
using GKPathFinding;
using GKFOW;

namespace GKRole
{
    public class GKUnit : MonoBehaviour
    {
        #region PublicField
        public long InstanceID
        {
            get
            {
                return _instanceID;
            }
        }

        public Transform myTransform
        {
            protected set { _transform = value; }
            get
            {
                if (null == _transform)
                {
                    _transform = transform;
                }
                return _transform;
            }
        }

        public Animator myAnimator
        {
            get
            {
                if (null == _animator)
                {
                    _animator = GK.FindChildOfType<Animator>(gameObject);
                }
                return _animator;
            }
        }

        public Projector Range
        {
            get
            {
                if (null == _range)
                {
                    _range = GK.FindChildOfType<Projector>(gameObject);
                }
                return _range;
            }
        }

        public GKTerrainGrid Grid
        {
            get
            {
                
                return _curGrid;
            }
            set
            {
                // 离开之前地块.
                if (null != _curGrid)
                {
                    _curGrid.Leave(this);
                }

                _curGrid = value;
                // 进入新地块.
                if (null != value)
                {
                    _curGrid.Enter(this);
                }

                // 更新视野内对象.
                var lst = UpdateUnitSight();
                // 判断是否为玩家阵营. 如果是进行战争迷雾及视野更新.
                if(GetAttribute(EObjectAttr.Camp).ValInt == (int)PlayerController.Instance().Camp)
                {
                    List<int> idxLst = new List<int>();
                    foreach(var t in lst)
                    {
                        idxLst.Add(t.tileID);
                    }
                    // 更新战争迷雾.
                    FOW.Instance().UpdateDiscoverArea(GetAttribute(EObjectAttr.Camp).ValInt, idxLst);
                    // 更新玩家视野.
                    LevelController.Instance().UpdateGlobalSightByCamp(PlayerController.Instance().Camp);
                }
            }
        }

        // 行为树对象.
        public BehaviorTree AI
        {
            get
            {
                if(null == _behaviorTree)
                {
                    _behaviorTree = GK.GetOrAddComponent<BehaviorTree>(gameObject);
                    var extBt = GK.TryLoadResource<ExternalBehaviorTree>("AI/Unit/" + 0);
                    _behaviorTree.StartWhenEnabled = false;
                    _behaviorTree.ExternalBehavior = extBt;
                }
                return _behaviorTree;
            }
            set
            {
                _behaviorTree = value;
            } 
        }

        // 是否需要支援标志位.
        public bool bSupport = false;

        // 销毁时调用.
        public System.Action OnDestroyUnit;
        #endregion

        #region PrivateField
        protected long _instanceID = 0;
        protected Transform _transform = null;
        // 是否渲染角色.
        protected bool _isRender = true;
        protected List<Renderer> _renders = new List<Renderer>();
        protected List<SkinnedMeshRenderer> _skinMeshRenders = new List<SkinnedMeshRenderer>();
        protected Vector3 _pos = Vector3.zero;
        protected GKUnitStateMachine _stateMachine = null;
        protected Animator _animator = null;
        protected bool _collisionEnable = true;
        protected Collider _collider;
        protected bool bState = true;
        protected GKDataBase _data = new GKDataBase();
        // 范围对象. 
        // 实现方式为投影.
        protected Projector _range = null;
        // 当前角色所在地块.
        protected GKTerrainGrid _curGrid;
        protected BehaviorTree _behaviorTree = null;
        // 视野内对象.
        List<GKUnit> _sightUnitLst = new List<GKUnit>();
        // 视野内地块.
        List<GKTerrainGrid> _sightTileLst = new List<GKTerrainGrid>();
        #endregion

        #region PublicMethod
        virtual public void OnNew(GKDataBase data)
        {
            _instanceID = GKMemoryController.Instance().GetGUID();
            _data = data;
            InitData();
            InitStateMachine();
            InitAttrDelegate();
        }

        // 数据行为.
        public GKCommonValue SetAttribute(EObjectAttr idx, int value, bool bDoEvent)
        {
            return _data.SetAttribute((int)idx, value, bDoEvent);
        }
        public GKCommonValue SetAttribute(EObjectAttr idx, long value, bool bDoEvent)
        {
            return _data.SetAttribute((int)idx, value, bDoEvent);
        }
        public GKCommonValue SetAttribute(EObjectAttr idx, float value, bool bDoEvent)
        {
            return _data.SetAttribute((int)idx, value, bDoEvent);
        }
        public GKCommonValue SetAttribute(EObjectAttr idx, string value, bool bDoEvent)
        {
            return _data.SetAttribute((int)idx, value, bDoEvent);
        }
        public GKCommonValue GetAttribute(EObjectAttr idx)
        {
            return _data.GetAttribute((int)idx);
        }

        //  在创建角色时, 如果出现场景内存在此GUID. 使用此接口重新分配.
        public void ReSetGUID()
        {
            _instanceID = GKMemoryController.Instance().GetGUID();
        }

        virtual public void Destory(bool bDestoryPool)
        {
            _data.RecycleAllAttribute();
        }

        // 变更角色Shader.
        public void SetMatColor(Color color, float rimPower = 0.51f)
        {
            if (null != _renders && _renders.Count > 0)
            {
                GK.SetMatColor(_renders, color, gameObject, rimPower);
            }
        }

        // 获取当前点位置.
        public Vector3 GetPos()
        {
            _pos.x = GetAttribute(EObjectAttr.PosX).ValFloat;
            _pos.y = GetAttribute(EObjectAttr.PosY).ValFloat;
            _pos.z = GetAttribute(EObjectAttr.PosZ).ValFloat;
            return _pos;
        }
        // 设置Pos属性.
        public void SetPos(float x, float y, float z)
        {
            SetAttribute(EObjectAttr.PosX, x, false);
            SetAttribute(EObjectAttr.PosY, y, false);
            SetAttribute(EObjectAttr.PosZ, z, false);
        }
        public void SetPos(Vector3 v3)
        {
            SetAttribute(EObjectAttr.PosX, v3.x, false);
            SetAttribute(EObjectAttr.PosY, v3.y, false);
            SetAttribute(EObjectAttr.PosZ, v3.z, false);
        }
        // 获取目标点位置.
        public Vector3 GetTargetPos()
        {
            _pos.x = GetAttribute(EObjectAttr.TargetPosX).ValFloat;
            _pos.y = GetAttribute(EObjectAttr.TargetPosY).ValFloat;
            _pos.z = GetAttribute(EObjectAttr.TargetPosZ).ValFloat;
            return _pos;
        }

        // 设置旋转. 弧度.
        public void SetOrientation(float orient)
        {
            if (null == gameObject)
                return;
            int bRotate = GetAttribute(EObjectAttr.Rotation).ValInt;
            if (0 == bRotate)
            {
                float direction = GetAttribute(EObjectAttr.Direction).ValFloat;
                SetOrientationDeg(direction);
                return;
            }
            Vector3 eulerAngles = myTransform.eulerAngles;
            eulerAngles.y = orient * Mathf.Rad2Deg;
            myTransform.eulerAngles = eulerAngles;
        }
        // 度.
        public void SetOrientationDeg(float orient)
        {
            if (null == gameObject)
                return;
            int bRotate = GetAttribute(EObjectAttr.Rotation).ValInt;
            if (0 == bRotate)
            {
                float direction = GetAttribute(EObjectAttr.Direction).ValFloat;
                orient = direction;
            }
            Vector3 eulerAngles = myTransform.eulerAngles;
            eulerAngles.y = orient;
            myTransform.eulerAngles = eulerAngles;
        }

        // 设置包围盒激活状态.
        public void EnableCollision(bool enable)
        {
            if (_collisionEnable != enable)
            {
                _collisionEnable = enable;
                if (!ReferenceEquals(_collider, null) && _collider.enabled != enable)
                {
                    _collider.enabled = enable;
                }
            }
        }

        virtual public void Destory()
        {
            if (null != OnDestroyUnit)
            {

            }
        }

        // 当前是否渲染此对象.
        public void IsRending(bool rending)
        {
            _isRender = rending;
            foreach(var r in _skinMeshRenders)
            {
                if(null != r)
                    r.enabled = rending;
            }
        }

        // 获取角色有限状态机管理器.
        public GKUnitStateMachine GetStateMachine()
        {
            return _stateMachine;
        }

        #region AI
        // 设置决策.
        public void ChangeCommand(CommandType type)
        {
            if(null == AI)
            {
                Debug.LogError(string.Format("Change command faile. AI missing. type: {0}", type));
                return;
            }
            
            AI.GetVariable("CurrentCommand").SetValue((CommandType)type);
        }
        #endregion

        #region Action
        public void Idle()
        {
            ChangedState(MachineStateID.Idle);
        }

        public void Move(List<AStarPoint> lst)
        {
            GKUnitMoveState state = _stateMachine._GetStateById(MachineStateID.Move) as GKUnitMoveState;
            state.SetPath(lst);
            ChangedState(MachineStateID.Move);
        }

        // 攻击.
        public void Attack(GKUnit target)
        {
            // 计算二者距离.
            float distance = Vector3.Distance(myTransform.position, target.myTransform.position);
            // 攻击范围内, 有效.
            if (distance <= GetAttribute(EObjectAttr.AttackRange).ValInt)
            {
                GKUnitAttackState state = _stateMachine._GetStateById(MachineStateID.Attack) as GKUnitAttackState;
                state.SetTarget(LevelController.Instance().GetCurSelectTargetUnit());
                ChangedState(MachineStateID.Attack);
            }
        }

        // 收到攻击时, 如果为防御状态. 有一定几率格挡.
        public void Hit(int damage, GKUnit attacker)
        {
            bool isDefense = (MachineStateID.Defense == _stateMachine.GetCurrentState().ID);

            // 被格挡.
            if(IsDoge(isDefense, attacker)) 
                return;

            GKUnitHitState state = _stateMachine._GetStateById(MachineStateID.Hit) as GKUnitHitState;
            state.SetDamage(damage, attacker);
            ChangedState(MachineStateID.Hit);
        }

        // 防御.
        public void Defense()
        {
            ChangedState(MachineStateID.Defense);
        }
        #endregion
        // 获取移动路径.
        public List<AStarPoint> GetMovePath(GKTerrainGrid target)
        {
            var start = MyAStar.Instance().MakePointByGKTerrainGrid(_curGrid, new Vector2(target.row, target.col));
            var destination = MyAStar.Instance().MakePointByGKTerrainGrid(target, new Vector2(target.row, target.col));
            return MyAStar.Instance().FindWay(start, destination, FindWayType.Unit, _data.GetAttribute((int)EObjectAttr.LayerMask).ValInt);
        }

        // 显示范围.
        public void ShowRange(bool show, int radius = 0)
        {
            Range.enabled = show;

            if (show)
            {
                Range.orthographicSize = radius;
            }
        }

        // 获取普通攻击伤害值.
        // 目前仅为力量.
        public int GetAtkDamage()
        {
            return _data.GetAttribute((int)EObjectAttr.TotalStrength).ValInt;
        }

        // 是否被格挡.
        private bool IsDoge(bool isDefense, GKUnit attacker)
        {
            int baseDogeRate = isDefense ? 40 : 10;
            int attackAgi = attacker.GetAttribute(EObjectAttr.TotalAgility).ValInt;
            int selfAgi = GetAttribute(EObjectAttr.TotalAgility).ValInt;
            int DogeRate = (selfAgi - attackAgi) + baseDogeRate;
            int idx = Random.Range(0, 100);
            // 判断是否格挡成功.
            if (DogeRate >= idx)
            {
                ConfigController.Instance().ShowDoge(myTransform);
                return true;
            }
            return false;
        }

        #region Sight
        // 获取视野内对象.
        public List<GKUnit> GetUnitSightList()
        {
            if (0 == _sightUnitLst.Count)
                UpdateUnitSight();

            return _sightUnitLst;
        }

        // 获取视野内地块索引.
        public List<GKTerrainGrid> GetTileSightList()
        {
            if (0 == _sightTileLst.Count)
                UpdateUnitSight();

            return _sightTileLst;
        }

        // 获取视野内地块索引链表及更新视野内对象.
        public List<GKTerrainGrid> UpdateUnitSight()
        {
            _sightUnitLst.Clear();
            _sightTileLst.Clear();

            int range = _data.GetAttribute((int)EObjectAttr.Ken).ValInt;
            _sightTileLst.Add(Grid);
            _FindTileBySight(ref _sightTileLst, Grid, range);

            // 获取视野是也范围内角色.
            foreach (var t in _sightTileLst)
            {
                foreach (var n in t.GetUnitList())
                {
                    if (null != n && !_sightUnitLst.Contains(n) && n != this)
                    {
                        _sightUnitLst.Add(n);
                    }
                }
            }

            return _sightTileLst;
        }

        // 查找视野内地块列表.
        private void _FindTileBySight(ref List<GKTerrainGrid> outList, GKTerrainGrid root, int loopTime)
        {
            foreach (NextNode t in root._nexts)
            {
                if (null == t || null == t.grid)
                    continue;

                if (!outList.Contains(t.grid))
                {
                    outList.Add(t.grid);
                }

                if (0 < --loopTime && !outList.Contains(t.grid))
                {
                    _FindTileBySight(ref outList, t.grid, loopTime);
                }
            }
        }
        #endregion

        #endregion

        #region PrivateMethod
        protected void Start()
        {
            GK.FindAllChild<SkinnedMeshRenderer>(ref _skinMeshRenders, gameObject);
            FOW.Instance().OnSightChange += OnSightChanged;
        }

        protected void OnSightChanged(List<int> lst)
        {
            bool isRender = lst.Contains(Grid.tileID);
            if (isRender != _isRender)
                IsRending(isRender);
        }

        protected void InitStateMachine()
        {
            if (null == myAnimator)
                return;

            _stateMachine = new GKUnitStateMachine();
            _stateMachine.AddState(new GKUnitIdleState(this), true);
            _stateMachine.AddState(new GKUnitAmbushState(this), false);
            _stateMachine.AddState(new GKUnitDeadState(this), false);
            _stateMachine.AddState(new GKUnitDefenseState(this), false);
            _stateMachine.AddState(new GKUnitAttackState(this), false);
            _stateMachine.AddState(new GKUnitMoveState(this), false);
            _stateMachine.AddState(new GKUnitHitState(this), false);
        }

        protected void Update()
        {
            if (null != _stateMachine && bState)
                _stateMachine.Update();
        }

        // 变更角色状态.
        private void ChangedState(MachineStateID state)
        {
            _stateMachine.GoToState(state);
        }

        // 初始化角色数据.
        private void InitData()
        {
            if (null == _data)
                return;

            // Hp.
            int hp = _data.GetAttribute((int)EObjectAttr.MaxHp).ValInt;
            _data.SetAttribute((int)EObjectAttr.Hp, hp, false);
            // Mp.
            int mp = _data.GetAttribute((int)EObjectAttr.MaxMp).ValInt;
            _data.SetAttribute((int)EObjectAttr.Mp, mp, false);
            // Kill count.
            _data.SetAttribute((int)EObjectAttr.KillCount, 0, false);
        }

        private void OnDestroy()
        {
            ReleaseAttrDelegate();
            FOW.Instance().OnSightChange -= OnSightChanged;
        }

        #region Delegate
        private void InitAttrDelegate()
        {
            _data.GetAttribute((int)EObjectAttr.Hp).OnAttrbutChangedEvent += OnHpChanged;
        }

        private void ReleaseAttrDelegate()
        {
            _data.GetAttribute((int)EObjectAttr.Hp).OnAttrbutChangedEvent -= OnHpChanged;
        }

        private void OnHpChanged(object obj, GKCommonValue attr)
        {
            int lastHp = attr.LastValInt;
            int curHp = attr.ValInt;
            ConfigController.Instance().ShowDamageText((curHp - lastHp), myTransform);
        }
        #endregion

        #endregion

    }
}

public enum CampType
{
    Yellow = 0,
    Red,
    Blue,
    Green,
    Purple
}
