using UnityEngine.UI;
using GKBase;
using GKUI;

public class UIInventoryItemSample : DynamicInfinityItem
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Image Icon;
        public Text CountText;
    }
    #endregion

    #region PublicField
    // 当前对象是否正在进行操作.
    public bool operation = false;
    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private Item _data;
    private InventoryOperationMode _mode;
    #endregion

    #region PublicMethod
    // 获取数据信息.
    public Item GetItemData()
    {
        return _data;
    }

    // 刷新数据与内容.
    public void Refresh()
    {
        // 如果当前物品对象正在进行操作, 关闭操作界面。
        if (operation)
        {
            operation = false;
            UIInventory.instance.ShowOperationPanel(false);
        }

        _data = (Item)mData;

        if (null == _data || null == m_ctl || null == m_ctl.Icon)
            return;

        m_ctl.Icon.gameObject.SetActive(ItemType.Empty != _data.type);
        if (ItemType.Empty != _data.type)
        {
            switch (_data.type)
            {
                case ItemType.Equipment:
                    m_ctl.Icon.sprite = ConfigController.Instance().GetEquipmentSprite(_data.id);
                    break;
                case ItemType.Consume:
                    m_ctl.Icon.sprite = ConfigController.Instance().GetConsumeSprite(_data.id);
                    break;
            }
            m_ctl.CountText.text = _data.count.ToString();
        }
    }

    public void OnClick()
    {
        _data = (Item)mData;

        if (null == _data || null == m_ctl || null == m_ctl.Icon || ItemType.Empty == _data.type)
        {
            UIInventory.instance.ShowOperationPanel(false, null);
            return;
        }

        UIInventory.instance.ShowOperationPanel(true, this);
        UIInventory.instance.SetSelectSolt(_data.solt);
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

    }

    private void Init()
    {
        // 第一刷新数据存在对象为初始化成功. 故在初始化后再进行一次刷新.
        Refresh();
    }
    #endregion
}
