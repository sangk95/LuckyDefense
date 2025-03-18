using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CABattleAIManager : CAManagerBase<CABattleAIManager>
{
    private List<CABattleManager.UnitTile> _listAIUnitTile = new List<CABattleManager.UnitTile>();
    private Dictionary<(CAUnitGrade, CAUnitType), int> _dicSpawnedUnit = new Dictionary<(CAUnitGrade, CAUnitType), int>();

    private CAUnitSpawner _unitSpawner;
    private WaitForSeconds _actionDelay;

    public int AISpawnCost { get; private set; }
    public int AIUnitCount { get; private set; }
    public int AIGold { get; private set; }
    public int AIDiamond { get; private set; }

    public override IEnumerator CAAwake()
    {
        AISpawnCost = CAConstants.START_SPAWN_COST;
        AIGold = CAConstants.START_GOLD;
        AIDiamond = 0;
        _actionDelay = new WaitForSeconds(CAConstants.AI_ACTION_DELAY);
        yield return base.CAAwake();
    }
    public override IEnumerator CAStart()
    {
        CABattleManager.Instance.AddMoneyForAI += AddMoney;
        return base.CAStart();
    }
    private void AddMoney(int gold, int diamond)
    {
        AIGold += gold;
        AIDiamond += diamond;
    }
    public void SetTilesTransform(Transform[] tiles)
    {
        _listAIUnitTile.Clear();
        for (int i = 0; i < tiles.Length; ++i)
        {
            var tile = new CABattleManager.UnitTile(tiles[i]);
            _listAIUnitTile.Add(tile);
        }
    }
    public bool SpawnUnit()
    {
        if (AIGold < AISpawnCost)
            return false;
        if (AIUnitCount == CAConstants.UNIT_MAX_COUNT)
            return false;

        CAUnitGrade grade = GetRandomUnitGrade();
        CAUnitType type = GetRandomUnitType();

        CABattleManager.UnitTile spawnTile = GetNextSpawnTile(grade, type);
        if (spawnTile != null)
        {
            CAUnitController spawnedUnit = _unitSpawner.SpawnUnit(grade, type, ref spawnTile);
            Action<CAEnemyController> tempCallback = null;
            CABattleManager.CAUnit unitData = new CABattleManager.CAUnit(grade, type);
            spawnedUnit.Initialize(ref tempCallback);
            if (_dicSpawnedUnit.ContainsKey((grade, type)))
                _dicSpawnedUnit[(grade, type)]++;
            else
                _dicSpawnedUnit.Add((grade, type), 1);

            AIGold -= AISpawnCost;
            AIUnitCount++;
            AISpawnCost += CAConstants.INCREASE_SPAWN_GOLD;

            return true;
        }
        return false;
    }
    private CAUnitGrade GetRandomUnitGrade()
    {
        int totalProbability = (int)CAUnitSpawnProbability.Normal +
                               (int)CAUnitSpawnProbability.Rare +
                               (int)CAUnitSpawnProbability.Hero;

        int roll = UnityEngine.Random.Range(1, totalProbability + 1);
        if (roll <= (int)CAUnitSpawnProbability.Normal)
            return CAUnitGrade.Normal;

        roll -= (int)CAUnitSpawnProbability.Normal;
        if (roll <= (int)CAUnitSpawnProbability.Rare)
            return CAUnitGrade.Rare;

        roll -= (int)CAUnitSpawnProbability.Rare;
        if (roll <= (int)CAUnitSpawnProbability.Hero)
            return CAUnitGrade.Hero;

        return CAUnitGrade.Normal;
    }

    private CAUnitType GetRandomUnitType()
    {
        return (CAUnitType)UnityEngine.Random.Range(0, 2);
    }
    public CABattleManager.UnitTile GetNextSpawnTile(CAUnitGrade grade, CAUnitType type)
    {
        if (grade != CAUnitGrade.Mythic)
        {
            foreach (CABattleManager.UnitTile tile in _listAIUnitTile)
            {
                if (tile.ContainsUnit(grade, type) && tile.GetUnitCount() < 3)
                {
                    return tile;
                }
            }
        }
        foreach (CABattleManager.UnitTile tile in _listAIUnitTile)
        {
            if (tile.GetUnitCount() == 0)
            {
                return tile;
            }
        }
        return null;
    }
    public bool UpgradeUnit()
    {
        foreach (var tile in _listAIUnitTile)
        {
            if (tile.GetUnitCount() == 3)
            {
                CAUnitGrade currentGrade = tile.GetUnitGrade();
                CAUnitType currentType = tile.GetUnitType();

                for (int i = 0; i < 3; i++)
                {
                    CAUnitController removedUnit = tile.RemoveUnit();
                    if (removedUnit != null)
                        Destroy(removedUnit.gameObject);
                }

                var key = (currentGrade, currentType);
                if (_dicSpawnedUnit.ContainsKey(key))
                {
                    _dicSpawnedUnit[key] -= 3;
                    if (_dicSpawnedUnit[key] <= 0)
                        _dicSpawnedUnit.Remove(key);
                }

                CAUnitGrade upgradedGrade = GetUpgradedGrade(currentGrade);
                CAUnitType upgradedType = (UnityEngine.Random.value < 0.5f) ? CAUnitType.Type1 : CAUnitType.Type2;

                CABattleManager.UnitTile tempTile = tile;

                PlaceSynthesizedUnit(upgradedGrade, upgradedType, ref tempTile);
                CAUnitController synthesizedUnit = _unitSpawner.SpawnUnit(upgradedGrade, upgradedType, ref tempTile);

                Action<CAEnemyController> tempCallback = null;
                CABattleManager.CAUnit unitData = new CABattleManager.CAUnit(upgradedGrade, upgradedType);
                synthesizedUnit.Initialize(ref tempCallback);
                if (_dicSpawnedUnit.ContainsKey((upgradedGrade, upgradedType)))
                    _dicSpawnedUnit[(upgradedGrade, upgradedType)]++;
                else
                    _dicSpawnedUnit.Add((upgradedGrade, upgradedType), 1);

                AIUnitCount -= 2;

                return true;
            }
        }
        return false;
    }

    public bool SpawnMythicUnit(CAUnitType type, List<(CAUnitGrade, CAUnitType)> requiredUnitKeys)
    {
        foreach (var unitKey in requiredUnitKeys)
        {
            if (_dicSpawnedUnit.TryGetValue(unitKey, out int count))
            {
                _dicSpawnedUnit[unitKey] = count - 1;
                if (_dicSpawnedUnit[unitKey] <= 0)
                    _dicSpawnedUnit.Remove(unitKey);
            }
            foreach (var tile in _listAIUnitTile)
            {
                if (tile.ContainsUnit(unitKey.Item1, unitKey.Item2))
                {
                    CAUnitController removedUnit = tile.RemoveUnit();
                    if (removedUnit != null)
                        Destroy(removedUnit.gameObject);
                    break;
                }
            }
            AIUnitCount--;
        }
        CAUnitGrade mythicGrade = CAUnitGrade.Mythic;
        CABattleManager.UnitTile spawnTile = GetNextSpawnTile(mythicGrade, type);
        if (spawnTile != null)
        {
            CAUnitController spawnedUnit = _unitSpawner.SpawnUnit(mythicGrade, type, ref spawnTile);

            Action<CAEnemyController> tempCallback = null;
            CABattleManager.CAUnit unitData = new CABattleManager.CAUnit(mythicGrade, type);
            spawnedUnit.Initialize(ref tempCallback);
            if (_dicSpawnedUnit.ContainsKey((mythicGrade, type)))
                _dicSpawnedUnit[(mythicGrade, type)]++;
            else
                _dicSpawnedUnit.Add((mythicGrade, type), 1);
            AIUnitCount++;

            return true;
        }
        return false;
    }

    public CAUnitGrade GetUpgradedGrade(CAUnitGrade currentGrade)
    {
        switch (currentGrade)
        {
            case CAUnitGrade.Normal:
                return CAUnitGrade.Rare;
            case CAUnitGrade.Rare:
                return CAUnitGrade.Hero;
            case CAUnitGrade.Hero:
                return CAUnitGrade.Mythic;
            case CAUnitGrade.Mythic:
                return CAUnitGrade.Mythic;
            default:
                return currentGrade;
        }
    }

    public void PlaceSynthesizedUnit(CAUnitGrade grade, CAUnitType type, ref CABattleManager.UnitTile curTile)
    {
        foreach (var tile in _listAIUnitTile)
        {
            if (tile != curTile && tile.ContainsUnit(grade, type) && tile.GetUnitCount() < 3)
            {
                curTile = tile;
                return;
            }
        }
    }

    private IEnumerator AIBattlePhase()
    {
        bool success = false;
        while (true)
        {
            yield return _actionDelay;
            success = false;
            success = SpawnUnit();
            if (success)
            {
                success = false;
                yield return _actionDelay;
            }

            success = UpgradeUnit();
            if (success)
            {
                success = false;
                yield return _actionDelay;
            }

            List<(CAUnitGrade, CAUnitType)> ownedUnitsForMythic = CABattleManager.Instance.GetOwnedUnitsForMythicCombination(CAConstants.SELECT_MYTHIC_01, _dicSpawnedUnit);
            if (ownedUnitsForMythic.Count == 3)
                success = SpawnMythicUnit(CAUnitType.Type1, ownedUnitsForMythic);
            if (success)
            {
                success = false;
                yield return _actionDelay;
            }

            ownedUnitsForMythic = CABattleManager.Instance.GetOwnedUnitsForMythicCombination(CAConstants.SELECT_MYTHIC_02, _dicSpawnedUnit);
            if (ownedUnitsForMythic.Count == 3)
                SpawnMythicUnit(CAUnitType.Type2, ownedUnitsForMythic);
        }
    }

    public void StartAIBattlePhase()
    {
        _unitSpawner = GetComponent<CAUnitSpawner>();
        StartCoroutine(AIBattlePhase());
    }
}
