using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GKBase;
using GKUI;

public class UIResult : SingletonUIBase<UIResult>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Text TitleText;
        public GameObject ResultContent;
        public UIResultItemSample UIResultItemSample;
        public Text ScoreText;
        public Button BackBtn;
    }
    #endregion

    #region PublicField
    public void SetData(bool bVictory)
    {
        _bVictory = bVictory;
    }
    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    [SerializeField]
    private GameObject[] _rewards;
    [SerializeField]
    private Image[] _rewardsIcon;
    // 游戏结果标志位.
    private bool _bVictory = false;
    private int _score = 0;
    #endregion

    #region PublicMethod

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
        GKUIEventTriggerListener.Get(m_ctl.BackBtn.gameObject).onClick = OnBack;
    }

    private void Init()
    {
        int id = _bVictory ? 94 : 95;
        m_ctl.TitleText.text = DataController.Instance().GetLocalization(id);
        _score = 0;

        // 生成参战角色战绩.
        GK.DestroyAllChildren(m_ctl.ResultContent);
        foreach (var card in PlayerController.Instance().GetFightCards().Values)
        {
            int score = 0;
            int kill = 0;
            card.EndGameResult(out score, out kill);
            var go = GameObject.Instantiate(m_ctl.UIResultItemSample.gameObject);
            if (null != go)
            {
                go.SetActive(true);
                GK.SetParent(go, m_ctl.ResultContent, false);
                GK.GetOrAddComponent<UIResultItemSample>(go).SetData(card.dataBase.GetAttribute((int)EObjectAttr.ID).ValInt, score, kill);
            }
            _score += score;
        }

        // 设置奖励道具.
        foreach(var reward in _rewards)
        {
            reward.SetActive(_bVictory);
        }
        if(_bVictory)
        {
            var lst = LevelController.Instance().GetEndGameReward();
            for (int i = 0; i < 3; i++)
            {
                if (i >= lst.Count || null == lst[i])
                    continue;

                switch(lst[i].type)
                {
                    case ItemType.Equipment:
                        _rewardsIcon[i].sprite = ConfigController.Instance().GetEquipmentSprite(lst[i].id);
                        break;
                    case ItemType.Consume:
                        _rewardsIcon[i].sprite = ConfigController.Instance().GetConsumeSprite(lst[i].id);
                        break;
                }

                PlayerController.Instance().NewItem(-1, (int)lst[i].type, lst[i].id, lst[i].count);
            }
        }

        m_ctl.ScoreText.text = _score.ToString();
    }

    private void OnBack(GameObject go)
    {
        CameraController.Instance().ChangeState(MachineStateID.Stop);
        UIMain.Close();
        UILoading.Open().Next(LoadingType.Lobby);
        Close();
    }
    #endregion
}
