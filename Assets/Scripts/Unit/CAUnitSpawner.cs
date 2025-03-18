using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static CABattleManager;

public class CAUnitSpawner : MonoBehaviour
{

    public GameObject normalType1Prefab;
    public GameObject normalType2Prefab;
    public GameObject rareType1Prefab;
    public GameObject rareType2Prefab;
    public GameObject heroType1Prefab;
    public GameObject heroType2Prefab;
    public GameObject mythicType1Prefab;
    public GameObject mythicType2Prefab;


    public CAUnitController SpawnUnit(CAUnitGrade grade, CAUnitType type, ref UnitTile spawnTile)
    {
        GameObject prefab = GetPrefabForUnit(grade, type);
        if (prefab == null)
            return null;

        GameObject newUnit = Instantiate(prefab, spawnTile.PlacedTile.position, Quaternion.identity);
        CAUnitController unitController = newUnit.GetComponent<CAUnitController>();
        unitController.SetUnitData(grade, type);
        spawnTile.AddUnit(unitController);
        return unitController;
    }

    private GameObject GetPrefabForUnit(CAUnitGrade grade, CAUnitType type)
    {
        switch (grade)
        {
            case CAUnitGrade.Normal:
                return type == CAUnitType.Type1 ? normalType1Prefab : normalType2Prefab;
            case CAUnitGrade.Rare:
                return type == CAUnitType.Type1 ? rareType1Prefab : rareType2Prefab;
            case CAUnitGrade.Hero:
                return type == CAUnitType.Type1 ? heroType1Prefab : heroType2Prefab;
            case CAUnitGrade.Mythic:
                return type == CAUnitType.Type1 ? mythicType1Prefab : mythicType2Prefab;
            default:
                return null;
        }
    }
}