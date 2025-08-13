using UnityEngine;

public enum EventType
{
    BossSpawnCinematic,
    BossDead,
}


public enum GimmickEvent
{
    CandleOn,
    MeteorFall,
}
public enum EnemyType
{
    //Enemy
    Enemy_01,
    Enemy_02,

    //Boss
    Boss_01,
    Boss_02,
}

//애니메이션 용
public enum AnimationType
{
    Attack1,
    Attack2,
    Attack3,
    Attack4,
    Attack5,
    Special1,
    Special2,
    Die,
    TakeDamage,
}

public enum RoomDirection
{
    None,
    East,
    North,
    South,
    West
}

public enum RoomShape
{
    Cell,
    Holed_Cell,
    Hallway
}

public enum StageTheme
{
    Castle = 0,
    Dungeon = 1,
    Catacomb = 2,
    Volcano = 3,
}

public enum RoomType
{
    Normal,
    Start,
    BossPortal,
    Boss,
    Shop,
}

public enum ItemType
{
    SkillBook,
    Rune,
    Potion,
    None,
    Gold
}

public enum ElementalType
{
    Normal = 0,
    Fire = 1,
    Water = 2,
    Ice = 3,
    Electric = 4,
    Dark = 5,
    Light = 6

}

public enum GameState
{
    Init,
    MainMenu,
    Synopsis,
    Tutorial,
    Village,
    Dungeon,
    GameOver,
    Ending,
    Boss
}

public enum LoadState
{
    LoadStart,
    LoadComplete
}


public enum ItemGrade
{
    Normal,
    Rare,
    Epic,
    Legendary
}

public enum DungeonLevel
{
    Easy,
    Normal,
    Hard
}

public enum CameraEventType
{
    Shake,
}

public enum GimmickType
{
    None,
    GasCloud,
    //Countdown,
    //Darkness,
    //RoamingLight
}
public enum DialogType
{
    Purchase,// 구매창 띄울 때
    PurchaseConf,//구매 확인 버튼 눌렀을 때 
    SellConf//판매 확인 버튼 눌렀을 때 
}