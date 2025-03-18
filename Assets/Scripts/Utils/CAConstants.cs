using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CAConstants
{
    public const float GAME_TIME_SCALE = 1.5f;
    public const float ENEMY_MOVE_SPEED = 1.0f;
    public const float BOSS_MOVE_SPEED = 0.5f;
    public const float ENEMY_SPAWN_DELAY = 0.75f;
    public const float AI_ACTION_DELAY = 1.0f;
    public const float ATTACK_SPEED_TYPE_1 = 0.75f;
    public const float ATTACK_SPEED_TYPE_2 = 1.0f;
    public const float ATTACK_SPEED_HERO_1 = 0.5f;
    public const float ATTACK_SPEED_HERO_2 = 0.75f;
    public const float ATTACK_SPEED_MYTHIC_1 = 0.25f;
    public const float ATTACK_SPEED_MYTHIC_2 = 0.5f;
    public const float ENEMY_HP_COLOR_YELLOW = 0.5f;
    public const float ENEMY_HP_COLOR_RED = 0.25f;
    public const float ENEMY_DEAD_DELAY = 0.3f;
    public const float MONEY_ANIM_DURATION = 0.2f;
    public const float UNIT_MOVE_DURATION = 0.75f;
    public const float PRESSING_TIME = 0.2f;

    public const float INTRO_DISPLAY_TIME = 2.0f;
    public const float DAMAGE_DISPLAY_TIME = 0.5f;
    public const float ALERT_DISPLAY_TIME = 4.5f;
    public const float GAMBLE_DISPLAY_TIME = 1.0f;
    public const float GAMBLE_RESULT_DISPLAY_TIME = 1.0f;

    public const int GAMBLE_RATE_RARE = 60;
    public const int GAMBLE_RATE_HERO = 20;

    public const int ENEMY_COUNT_PER_ROUND = 40;
    public const int TIME_PER_ROUND = 20;
    public const int TIME_PER_ROUND_BOSS = 60;
    public const int START_ROUND_DELAY = 5;
    public const int AUTO_EXIT_DURATION = 3;
    public const int TARGET_CLEAR_WAVE = 80;

    public const int ATTACK_POWER_NORMAL = 50;
    public const int ATTACK_POWER_RARE = 200;
    public const int ATTACK_POWER_HERO = 700;
    public const int ATTACK_POWER_MYTHIC = 1500;

    public const int ENEMY_LEVEL_HP = 250;
    public const int ENEMY_WAVE_HP = 50;
    public const int BOSS_LEVEL_HP = 10000;
    
    public const int START_GOLD = 100;
    public const int START_SPAWN_COST = 20;
    public const int INCREASE_SPAWN_GOLD = 2;    
    public const int INCREASE_ROUND_GOLD = 20;

    public const int GAMBLE_COST_RARE = 1;
    public const int GAMBLE_COST_HERO = 2;

    public const int SELL_PRICE_NORMAL = 6;
    public const int SELL_PRICE_RARE = 1;
    public const int SELL_PRICE_HERO = 2;

    public const int COMBINE_WEIGHT_NORMAL = 10;
    public const int COMBINE_WEIGHT_RARE = 26;
    public const int COMBINE_WEIGHT_HERO = 64;

    public const int ENEMY_DANGER_COUNT = 70;
    public const int ENEMY_MAX_COUNT = 100;
    public const int UNIT_MAX_COUNT = 20;

    public const int UI_DAMAGE_OFFSET = 125;
    public const int UI_MONEY_OFFSET = 50;

    public const string MY_ENEMY_TRANSFORM = "TilemapWayPoints_Mine";
    public const string AI_ENEMY_TRANSFORM = "TilemapWayPoints_AI";
    public const string MY_UNIT_TILE_TRANSFORM = "Tilemap_Mine";
    public const string AI_UNIT_TILE_TRANSFORM = "Tilemap_AI";
    public const string SELECT_MYTHIC_01 = "Select_Mythic01";
    public const string SELECT_MYTHIC_02 = "Select_Mythic02";
}
public static class CAUtils
{
    public static T FindObjectInChildren<T>(GameObject parent, string name = "") where T : Component
    {
        T target = parent.GetComponent<T>();
        if (target != null && (string.IsNullOrEmpty(name) || name == target.name))
        {
            return target;
        }

        foreach (Transform child in parent.transform)
        {
            T childTarget = FindObjectInChildren<T>(child.gameObject);
            if (childTarget != null && (string.IsNullOrEmpty(name) || name == childTarget.name))
            {
                return childTarget;
            }
        }

        return null;
    }
}