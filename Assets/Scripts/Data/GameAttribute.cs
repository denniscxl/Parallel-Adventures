using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EObjectAttr
{
    //  对象基本属性.
    BaseAttr_Start = -1,
    GUID,               // GUID 0.
    Type,               // 0 无效, 1 Player 2 Enemy 3 Npc.
    BaseAttr_Count,

    UnitAttr_Start = 99,
    ID,                 // 
    Power,              // 战力
    Name,               // 名称
    Level,              // 等级
    MaxExp,             // 最大经验 (升级所需要经验)
    Exp,                // 当前经验
    SkillLevel,         // 技能等级
    MaxSkillExp,        // 最大技能经验.
    SkillExp,           // 当前技能经验.
    Unit_Skills,        // 角色技能列表.  List
    MaxHp,              // 最大hp 
    MaxMp,              // 最大mp 
    Hp,                 // 当前hp 
    Mp,                 // 当前mp 
    Mood,               // 心情 
    MoveSpeed,          // 移动速度.
    RotationSpeed,      // 旋转速度.
    Ken,                // 视野范围.
    AttackRange,        // 攻击范围.
    AttackInterval,     // 攻击间隔.
    Strength,           // 力量
    Agility,            // 敏捷
    Intelligence,       // 智力
    TotalStrength,      // 总和力量
    TotalAgility,       // 总和敏捷
    TotalIntelligence,  // 总和智力
    PhyAttck,           // 物理基础攻击
    MagicAttack,        // 魔法基础攻击
    PhyDefense,         // 物理基础攻防御
    MagicDefense,       // 魔法基础攻防御
    Debarb,             // 回避
    TargetPosX,         // 目标位置x 
    TargetPosY,         // 目标位置y 
    TargetPosZ,         // 目标位置z 
    PosX,               // 当前位置x 
    PosY,               // 当前位置y 
    PosZ,               // 当前位置z 
    Rotation,           // 旋转标志位 0 不旋转 1 旋转中
    Direction,          // 方向(度, 顺时针)  
    CanBeDestory,       // 是否可被破坏. 0 false, 1 true.
    IsDead,             // 是否死亡. 0 未死亡, 1 死亡.
    DeathCount,         // 死亡数
    KillCount,          // 历史总杀戮数.
    InGameKillCount,    // 游戏中斩杀数.
    SkillTreeID,        // 角色技能书索引.
    UsedSkillPoint,     // 已使用技能点.
    Job,                // 职业.
    Score,              // 历史总得分.
    InGameScore,        // 游戏中所得分数.
    FightCount,         // 出战总数.
    Unit_Equipments,    // 角色装备列表.  List
    Camp,               // 阵营 (当前归属).
    LayerMask,          // 可移动地块值.


    TileID,             // 所属地块.
    Output,             // 资源产出量.
    UnitAttr_Count,     

    // 玩家数据段.
    PlayerAttr_Start = 299,
    Coins,              // 300 玩家金币数.
    Diamond,            // 301 玩家钻石数
    Belief,             // 玩家信仰 (战场内).
    Food,               // 玩家食物 (战场内).
    InventoryLevel,     // 玩家等级.
    Achievements,       // 玩家已获得成就. List
    Title,              // 玩家称号.
    CreateTime,         // 创建时间.
    PlayerAttr_Count,

    // 成就累积数据.
    PlayerAchievemt_Start = 499,
    AchiKillCount,          // 总击杀数.
    AchiDeathCount,         // 总角色死亡数.
    AchiCoinCost,           // 总金币支出累积.
    AchiDiamondCost,        // 总钻石支出累积.
    AchiConsumeCost,        // 总消耗品累积.
    AchiFightingCount,      // 总战斗次数.
    AchiSkillUpgrade,       // 总技能升级总数。
    AchiThrowCount,         // 总丢弃物品总数.
    AchiWinCount,           // 总胜利数.
    AchiDefeatedCount,      // 总失败数.
    PlayerAchievemt_Count,

    // 通用设置数据段
    OptionAttr_Start = 899,
    Sound,              // 音效.
    Music,              // 音乐.
    RendingQuality,     // 渲染质量.
    Language,           // 语言.
    OptionAttr_Count,

}
