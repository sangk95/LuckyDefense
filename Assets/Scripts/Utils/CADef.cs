using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CAUIState
{
    None,
    Normal,
    Hover,
    Click,
    Disable,
    Selected,
}

public enum CAUIAddress
{
    None,
    CAPage_LobbyMain,
    CAPage_Battle,
    CAPage_Mythic,
    CAUIDamage,
    CAUIAddGold,
    CAUIAddDiamond,
}

public enum CAGameState
{
    None,
    State_Lobby,
    State_Battle,
}
public enum CAScene
{
    None,
    StandBy,
    Lobby,
    BattleScene,
}

public enum CAStateProcess
{
    Init,
    Scene,
    UI,
    Player,
    ETC,
    End,
}
public enum CABattleResult
{
    Lose_By_EnemyCount,
    Lose_By_Boss,
    Win,
}

public enum CAUnitGrade
{
    None = -1,
    Normal,
    Rare,
    Hero,
    Mythic
}

public enum CAUnitType
{
    None = -1,
    Type1,
    Type2
}

public enum CAUnitSpawnProbability
{
    Normal = 75,
    Rare = 20,
    Hero = 5,
}
