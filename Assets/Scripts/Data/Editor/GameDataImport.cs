using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using GKBase;

public class GameDataImport
{
    public static GameData LoadOrCreateGameData()
    {
        return GKEditor.LoadOrCreateAsset<GameData>("Assets/Resources/Data/_AutoGen_/GameData.asset");
    }

    public static void OnImportData(string filename)
    {
        var data = LoadOrCreateGameData();
        EditorUtility.SetDirty(data);

        var basename = System.IO.Path.GetFileName(filename);

        if (basename == "GameData_UnitData.csv") { OnImportUnitData(filename, data); return; }
        if (basename == "GameData_ExpData.csv") { OnImportExpData(filename, data); return; }
        if (basename == "GameData_SkillData.csv") { OnImportSkillData(filename, data); return; }
        if (basename == "GameData_SkillTreeData.csv") { OnImportSkillTreeData(filename, data); return; }
        if (basename == "GameData_StoreData.csv") { OnImportStoreData(filename, data); return; }
        if (basename == "GameData_LotteryData.csv") { OnImportLotteryData(filename, data); return; }
        if (basename == "GameData_EquipmentData.csv") { OnImportEquipmentData(filename, data); return; }
        if (basename == "GameData_ConsumeData.csv") { OnImportConsumeData(filename, data); return; }
        if (basename == "GameData_InventoryUpgradeData.csv") { OnImportInventoryUpgradeData(filename, data); return; }
        if (basename == "GameData_AchievementData.csv") { OnImportAchievementData(filename, data); return; }
        if (basename == "GameData_LocalizationData.csv") { OnImportLocalizationData(filename, data); return; }
        if (basename == "GameData_LocalizationErrorCodeData.csv") { OnImportLocalizationErrorCodeData(filename, data); return; }
        if (basename == "GameData_LocalizationUnitData.csv") { OnImportLocalizationUnitData(filename, data); return; }
        if (basename == "GameData_LocalizationSkillData.csv") { OnImportLocalizationSkillData(filename, data); return; }
        if (basename == "GameData_LocalizationItemData.csv") { OnImportLocalizationItemData(filename, data); return; }
        if (basename == "GameData_LocalizationAchiData.csv") { OnImportLocalizationAchiData(filename, data); return; }
        if (basename == "GameData_LocalizationAchiDescData.csv") { OnImportLocalizationAchiDescData(filename, data); return; }
        if (basename == "GameData_LocalizationTitleData.csv") { OnImportLocalizationTitleData(filename, data); return; }
        if (basename == "GameData_LocalizationTitleDescData.csv") { OnImportLocalizationTitleDescData(filename, data); return; }
        if (basename == "GameData_TerrainData.csv") { OnImportTerrainData(filename, data); return; }
        if (basename == "GameData_EnemyData.csv") { OnImportEnemyData(filename, data); return; }
        if (basename == "GameData_EnemyConfigData.csv") { OnImportEnemyConfigData(filename, data); return; }

    }
		
    static void OnImportUnitData(string filename, GameData data)
	{
		var p = GKCSVParser.OpenFile(filename, "#columns");
		if (p == null) return;

		int row = 0;

		// Calc valid lines.
		while (p.NextRow())
		{
			if (p.isRowStartWith("#")) continue;

			row++;
		}

		// Reset readIndex to 3.
		p.ResetReadIndex ();
		// Init item data array.
		data.ResetUnitDataTypeArray (row);

		while (p.NextRow())
		{
			if (p.isRowStartWith("#")) continue;

            var d = new GameData.UnitData();
            p.RowToObject<GameData.UnitData>(ref d);

            if(null == d || d.id< 0 || d.id >= data._unitData.Length)
				continue;

            data._unitData[d.id] = d;
		}
	}

    static void OnImportExpData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetExpDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.ExpData();
            p.RowToObject<GameData.ExpData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._expData.Length)
                continue;

            data._expData[d.id] = d;
        }
    }

    static void OnImportSkillData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetSkillDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.SkillData();
            p.RowToObject<GameData.SkillData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._skillData.Length)
                continue;

            data._skillData[d.id] = d;
        }
    }

    static void OnImportSkillTreeData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetSkillTreeDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.SkillTreeData();
            p.RowToObject<GameData.SkillTreeData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._skillTreeData.Length)
                continue;

            data._skillTreeData[d.id] = d;
        }
    }

    static void OnImportStoreData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetStoreDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.StoreData();
            p.RowToObject<GameData.StoreData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._storeData.Length)
                continue;

            data._storeData[d.id] = d;
        }
    }

    static void OnImportLotteryData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetLotteryDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.LotteryData();
            p.RowToObject<GameData.LotteryData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._lotteryData.Length)
                continue;

            data._lotteryData[d.id] = d;
        }
    }

    static void OnImportEquipmentData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetEquipmentDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.EquipmentData();
            p.RowToObject<GameData.EquipmentData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._equipmentData.Length)
                continue;

            data._equipmentData[d.id] = d;
        }
    }

    static void OnImportConsumeData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetConsumeDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.ConsumeData();
            p.RowToObject<GameData.ConsumeData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._consumeData.Length)
                continue;

            data._consumeData[d.id] = d;
        }
    }

    static void OnImportInventoryUpgradeData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetInventoryUpgradeDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.InventoryUpgradeData();
            p.RowToObject<GameData.InventoryUpgradeData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._inventoryUpgradeData.Length)
                continue;

            data._inventoryUpgradeData[d.id] = d;
        }
    }

    static void OnImportAchievementData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetAchievementDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.AchievementData();
            p.RowToObject<GameData.AchievementData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._achievementData.Length)
                continue;

            data._achievementData[d.id] = d;
        }
    }

    static void OnImportLocalizationData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetLocalizationDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.LocalizationData();
            p.RowToObject<GameData.LocalizationData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._localizationData.Length)
                continue;

            data._localizationData[d.id] = d;
        }
    }

    static void OnImportLocalizationErrorCodeData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetLocalizationErrorCodeDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.LocalizationData();
            p.RowToObject<GameData.LocalizationData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._localizationErrorCodeData.Length)
                continue;

            data._localizationErrorCodeData[d.id] = d;
        }
    }

    static void OnImportLocalizationUnitData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetLocalizationUnitDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.LocalizationData();
            p.RowToObject<GameData.LocalizationData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._localizationUnitData.Length)
                continue;

            data._localizationUnitData[d.id] = d;
        }
    }

    static void OnImportLocalizationSkillData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetLocalizationSkillDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.LocalizationData();
            p.RowToObject<GameData.LocalizationData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._localizationSkillData.Length)
                continue;

            data._localizationSkillData[d.id] = d;
        }
    }

    static void OnImportLocalizationItemData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetLocalizationItemDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.LocalizationData();
            p.RowToObject<GameData.LocalizationData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._localizationItemData.Length)
                continue;

            data._localizationItemData[d.id] = d;
        }
    }

    static void OnImportLocalizationAchiData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetLocalizationAchiDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.LocalizationData();
            p.RowToObject<GameData.LocalizationData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._localizationAchiData.Length)
                continue;

            data._localizationAchiData[d.id] = d;
        }
    }

    static void OnImportLocalizationAchiDescData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetLocalizationAchiDescDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.LocalizationData();
            p.RowToObject<GameData.LocalizationData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._localizationAchiDescData.Length)
                continue;

            data._localizationAchiDescData[d.id] = d;
        }
    }

    static void OnImportLocalizationTitleData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetLocalizationTitleDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.LocalizationData();
            p.RowToObject<GameData.LocalizationData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._localizationTitleData.Length)
                continue;

            data._localizationTitleData[d.id] = d;
        }
    }

    static void OnImportLocalizationTitleDescData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetLocalizationTitleDescDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.LocalizationData();
            p.RowToObject<GameData.LocalizationData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._localizationTitleDescData.Length)
                continue;

            data._localizationTitleDescData[d.id] = d;
        }
    }

    static void OnImportTerrainData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetTerrainDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.TerrainData();
            p.RowToObject<GameData.TerrainData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._terrainData.Length)
                continue;

            data._terrainData[d.id] = d;
        }
    }

    static void OnImportEnemyData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetEnemyDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.EnemyData();
            p.RowToObject<GameData.EnemyData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._enemyData.Length)
                continue;

            data._enemyData[d.id] = d;
        }
    }

    static void OnImportEnemyConfigData(string filename, GameData data)
    {
        var p = GKCSVParser.OpenFile(filename, "#columns");
        if (p == null) return;

        int row = 0;

        // Calc valid lines.
        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            row++;
        }

        // Reset readIndex to 3.
        p.ResetReadIndex();
        // Init item data array.
        data.ResetEnemyConfigDataTypeArray(row);

        while (p.NextRow())
        {
            if (p.isRowStartWith("#")) continue;

            var d = new GameData.EnemyConfigData();
            p.RowToObject<GameData.EnemyConfigData>(ref d);

            if (null == d || d.id < 0 || d.id >= data._enemyConfigData.Length)
                continue;

            data._enemyConfigData[d.id] = d;
        }
    }
}
