using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GKData;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameData : GKGameData
{

    #region UnitData
    [System.Serializable]
    public class UnitData
    {
        public int id;
        public int name;
        public int maxHp;
        public int maxMp;
        public float speed;
        public float rotate;
        public int ken;
        public int atkRange;
        public float atkInterval;
        public int strength;
        public int agility;
        public int intelligence;
        public int skillConfigID;
        public int description;
        public int job;
        public int layerMask;
        public int costFood;
        public int costBelief;
    }
    [SerializeField]
    public UnitData[] _unitData;
    public UnitData GetUnitData(int id)
    {
        if (id < 0 || id >= _unitData.Length)
        {
            Debug.LogError(string.Format("Get unit data faile. id: {0}", id));
            return null;
        }
        return _unitData[id];
    }
#if UNITY_EDITOR
    public void InitUnitProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _unitData[idx].id;
        p.FindPropertyRelative("name").intValue = _unitData[idx].name;
        p.FindPropertyRelative("maxHp").intValue = _unitData[idx].maxHp;
        p.FindPropertyRelative("maxMp").intValue = _unitData[idx].maxMp;
        p.FindPropertyRelative("speed").floatValue = _unitData[idx].speed;
        p.FindPropertyRelative("rotate").floatValue = _unitData[idx].rotate;
        p.FindPropertyRelative("ken").intValue = _unitData[idx].ken;
        p.FindPropertyRelative("atkRange").floatValue = _unitData[idx].atkRange;
        p.FindPropertyRelative("atkInterval").floatValue = _unitData[idx].atkInterval;
        p.FindPropertyRelative("strength").intValue = _unitData[idx].strength;
        p.FindPropertyRelative("agility").intValue = _unitData[idx].agility;
        p.FindPropertyRelative("intelligence").intValue = _unitData[idx].intelligence;
        p.FindPropertyRelative("skillConfigID").intValue = _unitData[idx].skillConfigID;
        p.FindPropertyRelative("description").intValue = _unitData[idx].description;
        p.FindPropertyRelative("job").intValue = _unitData[idx].job;
        p.FindPropertyRelative("layerMask").intValue = _unitData[idx].layerMask;
        p.FindPropertyRelative("costFood").intValue = _unitData[idx].costFood;
        p.FindPropertyRelative("costBelief").intValue = _unitData[idx].costBelief;
    }
    public void ResetUnitDataTypeArray(int length) { ResetDataArray<UnitData>(length, ref _unitData); }
#endif
    #endregion

    #region Exp
    [System.Serializable]
    public class ExpData
    {
        public int id;
        public int level;
        // 等级经验.
        public int exp;
        // 技能经验.
        public int skill;
    }
    [SerializeField]
    public ExpData[] _expData;
    public ExpData GetExpData(int id)
    {
        if (id < 0 || id >= _expData.Length)
        {
            Debug.LogError(string.Format("Get exp data faile. id: {0}", id));
            return null;
        }
        return _expData[id];
    }
#if UNITY_EDITOR
    public void InitExpProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _expData[idx].id;
        p.FindPropertyRelative("level").intValue = _expData[idx].level;
        p.FindPropertyRelative("exp").intValue = _expData[idx].exp;
        p.FindPropertyRelative("skill").intValue = _expData[idx].skill;
    }
    public void ResetExpDataTypeArray(int length) { ResetDataArray<ExpData>(length, ref _expData); }
#endif
    #endregion

    #region Skill
    [System.Serializable]
    public class SkillData
    {
        public int id;
        // 技能ID最后二位为等级.
        public int key;
        public int name;
        // 技能升级所需要依赖属性.
        public int strength;
        public int agility;
        public int intelligence;
        // 技能升级所依赖ID. -1为不需要依赖.
        public int demandA;
        public int demandB;
        public int demandC;
        public int description;
    }
    [SerializeField]
    public SkillData[] _skillData;
    public SkillData GetSkillData(int id)
    {
        if (id < 0 || id >= _skillData.Length)
        {
            Debug.LogError(string.Format("Get skill data faile. id: {0}", id));
            return null;
        }
        return _skillData[id];
    }
#if UNITY_EDITOR
    public void InitSkillProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _skillData[idx].id;
        p.FindPropertyRelative("key").intValue = _skillData[idx].key;
        p.FindPropertyRelative("name").intValue = _skillData[idx].name;
        p.FindPropertyRelative("strength").intValue = _skillData[idx].strength;
        p.FindPropertyRelative("agility").intValue = _skillData[idx].agility;
        p.FindPropertyRelative("intelligence").intValue = _skillData[idx].intelligence;
        p.FindPropertyRelative("demandA").intValue = _skillData[idx].demandA;
        p.FindPropertyRelative("demandB").intValue = _skillData[idx].demandB;
        p.FindPropertyRelative("demandC").intValue = _skillData[idx].demandC;
        p.FindPropertyRelative("description").intValue = _skillData[idx].description;
    }
    public void ResetSkillDataTypeArray(int length) { ResetDataArray<SkillData>(length, ref _skillData); }
#endif
    #endregion

    #region SkillTree
    [System.Serializable]
    public class SkillTreeData
    {
        public int id;
        public List<int> skills;
    }
    [SerializeField]
    public SkillTreeData[] _skillTreeData;
    public SkillTreeData GetSkillTreeData(int id)
    {
        if (id < 0 || id >= _skillTreeData.Length)
        {
            Debug.LogError(string.Format("Get skillTree data faile. id: {0}", id));
            return null;
        }
        return _skillTreeData[id];
    }
#if UNITY_EDITOR
    /// List 反序列化暂时没有.
    public void InitSkillTreeProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _skillTreeData[idx].id;
    }
    public void ResetSkillTreeDataTypeArray(int length) { ResetDataArray<SkillTreeData>(length, ref _skillTreeData); }
#endif
    #endregion

    #region Store
    [System.Serializable]
    public class StoreData
    {
        public int id;
        public int goldPay;
        public int goldEarnings;
        public int diamondPay;
        public int diamondEarnings;
        public int itemPay;
        public int itemEarnings;
    }
    [SerializeField]
    public StoreData[] _storeData;
    public StoreData GetStoreData(int id)
    {
        if (id < 0 || id >= _storeData.Length)
        {
            Debug.LogError(string.Format("Get store data faile. id: {0}", id));
            return null;
        }
        return _storeData[id];
    }
#if UNITY_EDITOR
    public void InitStoreProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _storeData[idx].id;
        p.FindPropertyRelative("goldPay").intValue = _storeData[idx].goldPay;
        p.FindPropertyRelative("goldEarnings").intValue = _storeData[idx].goldEarnings;
        p.FindPropertyRelative("diamondPay").intValue = _storeData[idx].diamondPay;
        p.FindPropertyRelative("diamondEarnings").intValue = _storeData[idx].diamondEarnings;
        p.FindPropertyRelative("itemPay").intValue = _storeData[idx].itemPay;
        p.FindPropertyRelative("itemEarnings").intValue = _storeData[idx].itemEarnings;
    }
    public void ResetStoreDataTypeArray(int length) { ResetDataArray<StoreData>(length, ref _storeData); }
#endif
    #endregion

    #region Lottery
    [System.Serializable]
    public class LotteryData
    {
        public int id;
        public int coin;
        public int diamond;
    }
    [SerializeField]
    public LotteryData[] _lotteryData;
    public LotteryData GetLotteryData(int id)
    {
        if (id < 0 || id >= _lotteryData.Length)
        {
            Debug.LogError(string.Format("Get lottery data faile. id: {0}", id));
            return null;
        }
        return _lotteryData[id];
    }
#if UNITY_EDITOR
    public void InitLotteryProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _lotteryData[idx].id;
        p.FindPropertyRelative("coin").intValue = _lotteryData[idx].coin;
        p.FindPropertyRelative("diamond").intValue = _lotteryData[idx].diamond;
    }
    public void ResetLotteryDataTypeArray(int length) { ResetDataArray<LotteryData>(length, ref _lotteryData); }
#endif
    #endregion

    #region Equipment
    [System.Serializable]
    public class EquipmentData
    {
        public int id;
        public int name;
        public int part;
        public string job;
        public int strength;
        public int agility;
        public int intelligence;
        public int skillEffectA;
        public int skillEffectB;
        public int skillEffectC;
        public int description;
    }
    [SerializeField]
    public EquipmentData[] _equipmentData;
    public EquipmentData GetEquipmentData(int id)
    {
        if (id < 0 || id >= _equipmentData.Length)
        {
            Debug.LogError(string.Format("Get equipment data faile. id: {0}", id));
            return null;
        }
        return _equipmentData[id];
    }
#if UNITY_EDITOR
    public void InitEquipmentProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _equipmentData[idx].id;
        p.FindPropertyRelative("name").intValue = _equipmentData[idx].name;
        p.FindPropertyRelative("part").intValue = _equipmentData[idx].part;
        p.FindPropertyRelative("job").stringValue = _equipmentData[idx].job;
        p.FindPropertyRelative("strength").intValue = _equipmentData[idx].strength;
        p.FindPropertyRelative("agility").intValue = _equipmentData[idx].agility;
        p.FindPropertyRelative("intelligence").intValue = _equipmentData[idx].intelligence;
        p.FindPropertyRelative("skillEffectA").intValue = _equipmentData[idx].skillEffectA;
        p.FindPropertyRelative("skillEffectB").intValue = _equipmentData[idx].skillEffectB;
        p.FindPropertyRelative("skillEffectC").intValue = _equipmentData[idx].skillEffectC;
        p.FindPropertyRelative("description").intValue = _equipmentData[idx].description;
    }
    public void ResetEquipmentDataTypeArray(int length) { ResetDataArray<EquipmentData>(length, ref _equipmentData); }
#endif
    #endregion

    #region ConsumeData
    [System.Serializable]
    public class ConsumeData
    {
        public int id;
        public int name;
        public List<int> effect;
        public int description;
    }
    [SerializeField]
    public ConsumeData[] _consumeData;
    public ConsumeData GetConsumeData(int id)
    {
        if (id < 0 || id >= _consumeData.Length)
        {
            Debug.LogError(string.Format("Get consume data faile. id: {0}", id));
            return null;
        }
        return _consumeData[id];
    }
#if UNITY_EDITOR
    public void InitConsumeProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _consumeData[idx].id;
        p.FindPropertyRelative("name").intValue = _consumeData[idx].name;
        p.FindPropertyRelative("description").intValue = _consumeData[idx].description;
    }
    public void ResetConsumeDataTypeArray(int length) { ResetDataArray<ConsumeData>(length, ref _consumeData); }
#endif
    #endregion

    #region InventoryUpgrade
    [System.Serializable]
    public class InventoryUpgradeData
    {
        public int id;
        public int coin;
        public int diamond;
    }
    [SerializeField]
    public InventoryUpgradeData[] _inventoryUpgradeData;
    public InventoryUpgradeData GetInventoryUpgradeData(int id)
    {
        if (id < 0 || id >= _inventoryUpgradeData.Length)
        {
            Debug.LogError(string.Format("Get inventory upgrade data faile. id: {0}", id));
            return null;
        }
        return _inventoryUpgradeData[id];
    }
#if UNITY_EDITOR
    public void InitInventoryUpgradeProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _inventoryUpgradeData[idx].id;
        p.FindPropertyRelative("coin").intValue = _inventoryUpgradeData[idx].coin;
        p.FindPropertyRelative("diamond").intValue = _inventoryUpgradeData[idx].diamond;
    }
    public void ResetInventoryUpgradeDataTypeArray(int length) { ResetDataArray<InventoryUpgradeData>(length, ref _inventoryUpgradeData); }
#endif
    #endregion

    #region AchievementData
    [System.Serializable]
    public class AchievementData
    {
        public int id;
        public int action;
        public int points;
        public int title;
        public List<int> parameter;
    }
    [SerializeField]
    public AchievementData[] _achievementData;
    public AchievementData GetAchievementData(int id)
    {
        if (id < 0 || id >= _achievementData.Length)
        {
            Debug.LogError(string.Format("Get achievement data faile. id: {0}", id));
            return null;
        }
        return _achievementData[id];
    }
#if UNITY_EDITOR
    public void InitAchievementProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _achievementData[idx].id;
        p.FindPropertyRelative("action").intValue = _achievementData[idx].action;
        p.FindPropertyRelative("points").intValue = _achievementData[idx].points;
        p.FindPropertyRelative("title").intValue = _achievementData[idx].title;
    }
    public void ResetAchievementDataTypeArray(int length) { ResetDataArray<AchievementData>(length, ref _achievementData); }
#endif
    #endregion

    #region Localization
    [System.Serializable]
    public class LocalizationData
    {
        public int id;
        public string english;
        public string chinese;
    }
    [SerializeField]
    public LocalizationData[] _localizationData;
    public LocalizationData GetLocalizationData(int id)
    {
        if (id < 0 || id >= _localizationData.Length)
        {
            Debug.LogError(string.Format("Get localization data faile. id: {0}", id));
            return null;
        }
        return _localizationData[id];
    }
#if UNITY_EDITOR
    public void InitLocalizationProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _localizationData[idx].id;
        p.FindPropertyRelative("english").stringValue = _localizationData[idx].english;
        p.FindPropertyRelative("chinese").stringValue = _localizationData[idx].chinese;
    }
    public void ResetLocalizationDataTypeArray(int length) { ResetDataArray<LocalizationData>(length, ref _localizationData); }
#endif
    #endregion

    #region LocalizationErroeCode
    [SerializeField]
    public LocalizationData[] _localizationErrorCodeData;
    public LocalizationData GetLocalizationErrorCodeData(int id)
    {
        if (id < 0 || id >= _localizationErrorCodeData.Length)
        {
            Debug.LogError(string.Format("Get localization error code data faile. id: {0}", id));
            return null;
        }
        return _localizationErrorCodeData[id];
    }
#if UNITY_EDITOR
    public void InitLocalizationErrorCodeProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _localizationErrorCodeData[idx].id;
        p.FindPropertyRelative("english").stringValue = _localizationErrorCodeData[idx].english;
        p.FindPropertyRelative("chinese").stringValue = _localizationErrorCodeData[idx].chinese;
    }
    public void ResetLocalizationErrorCodeDataTypeArray(int length) { ResetDataArray<LocalizationData>(length, ref _localizationErrorCodeData); }
#endif
    #endregion

    #region LocalizationUnit
    [SerializeField]
    public LocalizationData[] _localizationUnitData;
    public LocalizationData GetLocalizationUnitData(int id)
    {
        if (id < 0 || id >= _localizationUnitData.Length)
        {
            Debug.LogError(string.Format("Get localization unit data faile. id: {0}", id));
            return null;
        }
        return _localizationUnitData[id];
    }
#if UNITY_EDITOR
    public void InitLocalizationUnitProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _localizationUnitData[idx].id;
        p.FindPropertyRelative("english").stringValue = _localizationUnitData[idx].english;
        p.FindPropertyRelative("chinese").stringValue = _localizationUnitData[idx].chinese;
    }
    public void ResetLocalizationUnitDataTypeArray(int length) { ResetDataArray<LocalizationData>(length, ref _localizationUnitData); }
#endif
    #endregion

    #region LocalizationSkill
    [SerializeField]
    public LocalizationData[] _localizationSkillData;
    public LocalizationData GetLocalizationSkillData(int id)
    {
        if (id < 0 || id >= _localizationSkillData.Length)
        {
            Debug.LogError(string.Format("Get localization skill data faile. id: {0}", id));
            return null;
        }
        return _localizationSkillData[id];
    }
#if UNITY_EDITOR
    public void InitLocalizationSkillProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _localizationSkillData[idx].id;
        p.FindPropertyRelative("english").stringValue = _localizationSkillData[idx].english;
        p.FindPropertyRelative("chinese").stringValue = _localizationSkillData[idx].chinese;
    }
    public void ResetLocalizationSkillDataTypeArray(int length) { ResetDataArray<LocalizationData>(length, ref _localizationSkillData); }
#endif
    #endregion

    #region LocalizationItem
    [SerializeField]
    public LocalizationData[] _localizationItemData;
    public LocalizationData GetLocalizationItemData(int id)
    {
        if (id < 0 || id >= _localizationItemData.Length)
        {
            Debug.LogError(string.Format("Get localization item data faile. id: {0}", id));
            return null;
        }
        return _localizationItemData[id];
    }
#if UNITY_EDITOR
    public void InitLocalizationItemProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _localizationItemData[idx].id;
        p.FindPropertyRelative("english").stringValue = _localizationItemData[idx].english;
        p.FindPropertyRelative("chinese").stringValue = _localizationItemData[idx].chinese;
    }
    public void ResetLocalizationItemDataTypeArray(int length) { ResetDataArray<LocalizationData>(length, ref _localizationItemData); }
#endif
    #endregion

    #region LocalizationAchievement
    [SerializeField]
    public LocalizationData[] _localizationAchiData;
    public LocalizationData GetLocalizationAchiData(int id)
    {
        if (id < 0 || id >= _localizationAchiData.Length)
        {
            Debug.LogError(string.Format("Get localization achi data faile. id: {0}", id));
            return null;
        }
        return _localizationAchiData[id];
    }
#if UNITY_EDITOR
    public void InitLocalizationAchiProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _localizationAchiData[idx].id;
        p.FindPropertyRelative("english").stringValue = _localizationAchiData[idx].english;
        p.FindPropertyRelative("chinese").stringValue = _localizationAchiData[idx].chinese;
    }
    public void ResetLocalizationAchiDataTypeArray(int length) { ResetDataArray<LocalizationData>(length, ref _localizationAchiData); }
#endif
    #endregion

    #region LocalizationAchievementDescription
    [SerializeField]
    public LocalizationData[] _localizationAchiDescData;
    public LocalizationData GetLocalizationAchiDescData(int id)
    {
        if (id < 0 || id >= _localizationAchiDescData.Length)
        {
            Debug.LogError(string.Format("Get localization achievement description data faile. id: {0}", id));
            return null;
        }
        return _localizationAchiDescData[id];
    }
#if UNITY_EDITOR
    public void InitLocalizationAchiDescProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _localizationAchiDescData[idx].id;
        p.FindPropertyRelative("english").stringValue = _localizationAchiDescData[idx].english;
        p.FindPropertyRelative("chinese").stringValue = _localizationAchiDescData[idx].chinese;
    }
    public void ResetLocalizationAchiDescDataTypeArray(int length) { ResetDataArray<LocalizationData>(length, ref _localizationAchiDescData); }
#endif
    #endregion

    #region LocalizationTitle
    [SerializeField]
    public LocalizationData[] _localizationTitleData;
    public LocalizationData GetLocalizationTitleData(int id)
    {
        if (id < 0 || id >= _localizationTitleData.Length)
        {
            Debug.LogError(string.Format("Get localization title data faile. id: {0}", id));
            return null;
        }
        return _localizationTitleData[id];
    }
#if UNITY_EDITOR
    public void InitLocalizationTitleProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _localizationTitleData[idx].id;
        p.FindPropertyRelative("english").stringValue = _localizationTitleData[idx].english;
        p.FindPropertyRelative("chinese").stringValue = _localizationTitleData[idx].chinese;
    }
    public void ResetLocalizationTitleDataTypeArray(int length) { ResetDataArray<LocalizationData>(length, ref _localizationTitleData); }
#endif
    #endregion

    #region LocalizationTitleDescription
    [SerializeField]
    public LocalizationData[] _localizationTitleDescData;
    public LocalizationData GetLocalizationTitleDescData(int id)
    {
        if (id < 0 || id >= _localizationTitleDescData.Length)
        {
            Debug.LogError(string.Format("Get localization title description data faile. id: {0}", id));
            return null;
        }
        return _localizationTitleDescData[id];
    }
#if UNITY_EDITOR
    public void InitLocalizationTitleDescProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _localizationTitleDescData[idx].id;
        p.FindPropertyRelative("english").stringValue = _localizationTitleDescData[idx].english;
        p.FindPropertyRelative("chinese").stringValue = _localizationTitleDescData[idx].chinese;
    }
    public void ResetLocalizationTitleDescDataTypeArray(int length) { ResetDataArray<LocalizationData>(length, ref _localizationTitleDescData); }
#endif
    #endregion

    #region TerrainData
    [System.Serializable]
    public class TerrainData
    {
        public int id;
        public float speed;
        public int cost;
    }
    [SerializeField]
    public TerrainData[] _terrainData;
    public TerrainData GetTerrainData(int id)
    {
        if (id < 0 || id >= _terrainData.Length)
        {
            Debug.LogError(string.Format("Get terrain data faile. id: {0}", id));
            return null;
        }
        return _terrainData[id];
    }
#if UNITY_EDITOR
    public void InitTerrainProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _terrainData[idx].id;
        p.FindPropertyRelative("speed").floatValue = _terrainData[idx].speed;
        p.FindPropertyRelative("cost").intValue = _terrainData[idx].cost;
    }
    public void ResetTerrainDataTypeArray(int length) { ResetDataArray<TerrainData>(length, ref _terrainData); }

#endif
    #endregion

    #region EnemyData
    [System.Serializable]
    public class EnemyData
    {
        public int id;
        public int unit;
        public List<int> skills;
        public List<int> equips;
    }
    [SerializeField]
    public EnemyData[] _enemyData;
    public EnemyData GetEnemyData(int id)
    {
        if (id < 0 || id >= _enemyData.Length)
        {
            Debug.LogError(string.Format("Get enemy data faile. id: {0}", id));
            return null;
        }
        return _enemyData[id];
    }
#if UNITY_EDITOR
    public void InitEnemyProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _enemyData[idx].id;
        p.FindPropertyRelative("units").intValue = _enemyData[idx].unit;
    }
    public void ResetEnemyDataTypeArray(int length) { ResetDataArray<EnemyData>(length, ref _enemyData); }

#endif
    #endregion

    #region EnemyConfigData
    [System.Serializable]
    public class EnemyConfigData
    {
        public int id;
        public List<int> units;
    }
    [SerializeField]
    public EnemyConfigData[] _enemyConfigData;
    public EnemyConfigData GetEnemyConfigData(int id)
    {
        if (id < 0 || id >= _enemyConfigData.Length)
        {
            Debug.LogError(string.Format("Get enemy config data faile. id: {0}", id));
            return null;
        }
        return _enemyConfigData[id];
    }
#if UNITY_EDITOR
    public void InitEnemyConfigProperty(ref SerializedProperty p, int idx)
    {
        p.FindPropertyRelative("id").intValue = _enemyConfigData[idx].id;
    }
    public void ResetEnemyConfigDataTypeArray(int length) { ResetDataArray<EnemyConfigData>(length, ref _enemyConfigData); }

#endif
    #endregion
}
