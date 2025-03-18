using System;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using CA.UI;
using System.Linq;
using UnityEngine.Tilemaps;
using static CAUnitSpawner;
using UnityEngine.EventSystems;

public class CABattleManager : CAManagerBase<CABattleManager>
{
    public class CAUnit
    {
        public CAUnitGrade UnitGrade { get; private set; }
        public CAUnitType UnitType { get; private set; }
        public CAUnit(CAUnitGrade grade, CAUnitType type)
        {
            UnitGrade = grade;
            UnitType = type;
        }
    }
    public class UnitTile
    {
        public Transform PlacedTile { get; private set; }
        private List<CAUnitController> listUnit = new List<CAUnitController>();

        public UnitTile(Transform tile)
        {
            PlacedTile = tile;
        }
        public List<CAUnitController> GetListUnit()
        {
            return listUnit;
        }
        public bool ContainsUnit(CAUnitGrade grade, CAUnitType type)
        {
            return listUnit.Any(u => u.GetUnitData().UnitGrade == grade && u.GetUnitData().UnitType == type);
        }

        public int GetUnitCount()
        {
            return listUnit.Count;
        }
        public CAUnitGrade GetUnitGrade()
        {
            if (listUnit.Count > 0)
                return listUnit[0].GetUnitData().UnitGrade;
            else
                return CAUnitGrade.None;
        }
        public CAUnitType GetUnitType()
        {
            if (listUnit.Count > 0)
            {
                return listUnit[0].GetUnitData().UnitType;
            }
            return CAUnitType.None;
        }

        public CAUnitController RemoveUnit()
        {
            if (listUnit.Count > 0)
            {
                CAUnitController unit = listUnit[0];
                listUnit.RemoveAt(0);

                Vector3 basePos = PlacedTile.position + new Vector3(0, 0.2f);
                switch (listUnit.Count)
                {
                    case 1:
                        listUnit[0].transform.position = basePos;
                        break;
                    case 2:
                        listUnit[0].transform.position = basePos + new Vector3(-0.2f, 0, 0);
                        listUnit[1].transform.position = basePos + new Vector3(0.2f, 0, 0);
                        break;
                    default:
                        break;
                }
                foreach (var ownedUnit in listUnit)
                {
                    ownedUnit.SetColliderOffset(PlacedTile);
                }

                return unit;
            }
            return null;
        }

        public void AddUnit(CAUnitController unit)
        {
            unit._renderer.sortingOrder += listUnit.Count;
            listUnit.Add(unit);
            Vector3 basePos = PlacedTile.position + new Vector3(0, 0.2f);
            switch (listUnit.Count)
            {
                case 1:
                    listUnit[0].transform.position = basePos;
                    break;
                case 2:
                    listUnit[0].transform.position = basePos + new Vector3(-0.2f, 0, 0);
                    listUnit[1].transform.position = basePos + new Vector3(0.2f, 0, 0);
                    break;
                case 3:
                    listUnit[0].transform.position = basePos + new Vector3(-0.2f, 0.1f, 0);
                    listUnit[1].transform.position = basePos + new Vector3(0.2f, 0.1f, 0);
                    listUnit[2].transform.position = basePos + new Vector3(0, -0.1f, 0);
                    break;
                default:
                    break;
            }
            foreach (var ownedUnit in listUnit)
            {
                ownedUnit.SetColliderOffset(PlacedTile);
            }
        }
        public void JustAddUnit(CAUnitController unit)
        {
            unit._renderer.sortingOrder += listUnit.Count;
            listUnit.Add(unit);
            foreach (var ownedUnit in listUnit)
            {
                ownedUnit.SetColliderOffset(PlacedTile);
            }
        }
    }

    private CAUnitSpawner _unitSpawner;
    private CAEnemySpawner _enemySpawner;
    private Coroutine _coBattlePhase;
    private Transform[] _myEnemyWayPoint;
    private Transform[] _aiEnemyWayPoint;
    private WaitForSeconds _spawnTerm;
    private WaitForSeconds _enemyDeadDelay;

    private List<UnitTile> _listMyUnitTile;
    private Dictionary<(CAUnitGrade, CAUnitType), int> _dicSpawnedUnit = new Dictionary<(CAUnitGrade, CAUnitType), int>();
    private HashSet<CAEnemyController> _hashSpawnedEnemy = new HashSet<CAEnemyController>();

    private UnitTile _clickedTile;
    private CAPageBattle _pageBattle;

    public (CAUnitGrade, CAUnitType, UnitTile) GambleData;
    public int CurSpawnCost { get; private set; }
    public int MyUnitCount { get; private set; }
    public int MyGold { get; private set; }
    public int MyDiamond { get; private set; }
    public int PrevGold { get; private set; }
    public int PrevDiamond { get; private set; }
    public int CurWave { get; private set; }
    public int PrevEnemyCount { get; private set; }
    public int EnemyCount => _hashSpawnedEnemy.Count;
    public float RemainTime { get; protected set; }

    public Action<CAEnemyController> EnemyDeadCallBack;
    public Action<int, int> AddMoneyForAI; // item1 : gold, item2 : diamond
    public Action ChangedEnemyCount;
    public Action ChangedCurWave;
    public Action ChangedSpawnCost;
    public Action ChangedMyMoney;
    public Action ChangedUnitCount;

    public override IEnumerator CAAwake()
    {
        CurSpawnCost = CAConstants.START_SPAWN_COST;
        MyGold = CAConstants.START_GOLD;
        return base.CAAwake();
    }
    private bool isDragging = false;
    private float pressStartTime = 0f;
    private Transform initialTileTransform = null;
    Vector3 _dragStartPos;
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject() || _pageBattle == null)
            return;
        if (isDragging)
        {
            Vector2 currentPos = Input.mousePosition;
            _pageBattle.UpdateArrowUI(_dragStartPos, currentPos);
        }

        if (Input.GetMouseButtonDown(0) && _pageBattle != null)
        {

            pressStartTime = Time.time;
            initialTileTransform = null;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            RaycastHit2D[] hit = Physics2D.RaycastAll(mousePos2D, Vector2.zero);
            if (hit != null)
            {
                foreach (var objHit in hit)
                {
                    if (objHit.collider.CompareTag("Tile"))
                    {
                        initialTileTransform = objHit.transform;
                        _clickedTile = _listMyUnitTile.FirstOrDefault(tile => tile.PlacedTile == initialTileTransform);
                        if(_clickedTile.GetUnitCount() > 0)
                            _dragStartPos = Input.mousePosition;
                        return;
                    }
                }
            }
            isDragging = false;
        }

        if (Input.GetMouseButton(0))
        {
            if (!isDragging && Time.time - pressStartTime >= CAConstants.PRESSING_TIME)
            { 
                isDragging = true;
                _pageBattle.CloseUnitControl();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
            RaycastHit2D[] hit = Physics2D.RaycastAll(mousePos2D, Vector2.zero);
            Transform releaseTileTransform = null;

            if (hit != null)
            {
                foreach (var objHit in hit)
                {
                    if (objHit.collider.CompareTag("Tile"))
                    {
                        releaseTileTransform = objHit.transform;
                        break;
                    }
                }
            }

            if (isDragging)
            {
                _pageBattle.CloseUnitControl();
                if (releaseTileTransform != null)
                {
                    UnitTile destinationTile = _listMyUnitTile.FirstOrDefault(tile => tile.PlacedTile == releaseTileTransform);
                    if (destinationTile != null && _clickedTile != null && destinationTile != _clickedTile)
                    {
                        MoveUnitsBetweenTiles(_clickedTile, destinationTile);
                    }
                }
                _clickedTile = null;
            }
            else
            {
                if (releaseTileTransform != null)
                {
                    ShowTileUnitInfo(releaseTileTransform);
                }
                else
                    _pageBattle.CloseUnitControl();
            }

            _pageBattle.HideArrowUI();

            isDragging = false;
            pressStartTime = 0f;
            initialTileTransform = null;
        }
    }
    public void StartBattlePhase()
    { 
        _dicSpawnedUnit = new Dictionary<(CAUnitGrade, CAUnitType), int>();
        _hashSpawnedEnemy = new HashSet<CAEnemyController>();

        _unitSpawner = GetComponent<CAUnitSpawner>();
        _enemySpawner = GetComponent<CAEnemySpawner>();
        _spawnTerm = new WaitForSeconds(CAConstants.ENEMY_SPAWN_DELAY);
        _enemyDeadDelay = new WaitForSeconds(CAConstants.ENEMY_DEAD_DELAY);
        CurWave = 0;
        PrevEnemyCount = 0;
        RemainTime = CAConstants.START_ROUND_DELAY;
        CurSpawnCost = CAConstants.START_SPAWN_COST;
        MyGold = CAConstants.START_GOLD;
        MyDiamond = 0;
        PrevGold = MyGold;
        PrevDiamond = MyDiamond;

        Time.timeScale = CAConstants.GAME_TIME_SCALE;

        _coBattlePhase = StartCoroutine(BattlePhase());
    }
    private IEnumerator BattlePhase()
    {
        var uiController = CAUIManager.Instance.GetUIController(CAUIAddress.CAPage_Battle);
        if (uiController != null && uiController is CAPageBattle)
            _pageBattle = uiController as CAPageBattle;

        _pageBattle.SetNextWaveAlert();
        while (RemainTime > 0)
        {
            RemainTime -= Time.deltaTime;
            yield return null;
        }

        bool hasNextWaveAlert = false;
        while (true)
        {
            if(_hashSpawnedEnemy.Count == CAConstants.ENEMY_MAX_COUNT)
            {
                _pageBattle.SetBattleResult(CABattleResult.Lose_By_EnemyCount);
                yield break;
            }
            if(CurWave == CAConstants.TARGET_CLEAR_WAVE)
            {
                _pageBattle.SetBattleResult(CABattleResult.Win);
                yield break;
            }
            if(RemainTime < 5 && !hasNextWaveAlert)
            {
                hasNextWaveAlert = true;
                _pageBattle.SetNextWaveAlert();
            }
            if(RemainTime <= 0)
            {
                if(CurWave % 10 == 0 && _hashSpawnedEnemy.Count > 0)
                {
                    _pageBattle.SetBattleResult(CABattleResult.Lose_By_Boss);
                    yield break;
                }
                ++CurWave;
                if(CurWave % 10 == 0)
                    RemainTime = CAConstants.TIME_PER_ROUND_BOSS;
                else
                    RemainTime = CAConstants.TIME_PER_ROUND;
                PrevGold = MyGold;
                if(CurWave > 1)
                    MyGold += CAConstants.INCREASE_ROUND_GOLD;
                AddMoneyForAI?.Invoke(CAConstants.INCREASE_ROUND_GOLD, 0);
                ChangedCurWave?.Invoke();
                ChangedMyMoney?.Invoke();
                hasNextWaveAlert = false;
                if (CurWave > CAConstants.TARGET_CLEAR_WAVE)
                {
                    _pageBattle.SetBattleResult(CABattleResult.Win);
                    yield break;
                }
                StartCoroutine(CreateEnemies());
            }
            RemainTime -= Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator CreateEnemies()
    {
        if (_enemySpawner == null)
            yield break;

        var enemies = _enemySpawner.GetEnemies(CurWave);
        if (enemies == null)
            yield break;

        int spawnCount = 0;
        foreach (CAEnemyController enemy in enemies)
        {
            if (enemy == null)
            {
                Debug.LogError("spawn enemy is null");
                continue;
            }

            PrevEnemyCount = _hashSpawnedEnemy.Count;
            _hashSpawnedEnemy.Add(enemy);
            ChangedEnemyCount?.Invoke();

            enemy.gameObject.SetActive(true);
            if (spawnCount % 2 == 0)
            {
                enemy.Initialize(_myEnemyWayPoint, CurWave);
            }
            else
            {
                enemy.Initialize(_aiEnemyWayPoint, CurWave);
                yield return _spawnTerm;
            }
            ++spawnCount;
        }
    }
    private void MoveUnitsBetweenTiles(UnitTile sourceTile, UnitTile destinationTile)
    {
        if (sourceTile == null || destinationTile == null)
            return;
        if (sourceTile == destinationTile)
            return;

        if (destinationTile.GetUnitCount() == 0)
        {
            while (sourceTile.GetUnitCount() > 0)
            {
                CAUnitController unit = sourceTile.RemoveUnit();
                if (unit != null)
                {
                    destinationTile.JustAddUnit(unit);
                }
            }
            var sourceUnits = destinationTile.GetListUnit();
            var listTargetPosition = CalculateTargetPosition(destinationTile, sourceUnits);
            for (int i = 0; i < sourceUnits.Count; ++i)
            {
                StartCoroutine(AnimateUnitMove(sourceUnits[i], listTargetPosition[i], destinationTile.PlacedTile));
            }
        }
        else
        {
            List<CAUnitController> sourceUnits = new List<CAUnitController>();
            List<CAUnitController> destUnits = new List<CAUnitController>();

            while (sourceTile.GetUnitCount() > 0)
            {
                CAUnitController unit = sourceTile.RemoveUnit();
                if (unit != null)
                    sourceUnits.Add(unit);
            }
            while (destinationTile.GetUnitCount() > 0)
            {
                CAUnitController unit = destinationTile.RemoveUnit();
                if (unit != null)
                    destUnits.Add(unit);
            }

            foreach (var unit in sourceUnits)
            {
                destinationTile.JustAddUnit(unit);
            }
            var listTargetPosition = CalculateTargetPosition(destinationTile, sourceUnits);
            for(int i=0; i<sourceUnits.Count; ++i)
            {
                sourceUnits[i].SetColliderOffset(destinationTile.PlacedTile);
                StartCoroutine(AnimateUnitMove(sourceUnits[i], listTargetPosition[i], destinationTile.PlacedTile));
            }

            foreach (var unit in destUnits)
            {
                sourceTile.JustAddUnit(unit);
            }
            listTargetPosition = CalculateTargetPosition(sourceTile, destUnits);
            for (int i = 0; i < destUnits.Count; ++i)
            {
                destUnits[i].SetColliderOffset(sourceTile.PlacedTile);
                StartCoroutine(AnimateUnitMove(destUnits[i], listTargetPosition[i], sourceTile.PlacedTile));
            }
        }
    }
    private IEnumerator AnimateUnitMove(CAUnitController unit, Vector3 targetPosition, Transform targetTile)
    {
        if (unit._attackCo != null)
        {
            unit.StopCoroutine(unit._attackCo);
            unit._attackCo = null;
        }

        Vector3 startPos = unit.transform.position;
        float elapsed = 0f;
        while (elapsed < CAConstants.UNIT_MOVE_DURATION)
        {
            unit.transform.position = Vector3.Lerp(startPos, targetPosition, elapsed / CAConstants.UNIT_MOVE_DURATION);
            elapsed += Time.deltaTime;
            yield return null;
        }
        unit.transform.position = targetPosition;
        unit.SetColliderOffset(targetTile);
        unit._attackCo = unit.StartCoroutine(unit.AttackCo());
    }
    private List<Vector3> CalculateTargetPosition(UnitTile tile, List<CAUnitController> listUnit)
    {
        Vector3 basePos = tile.PlacedTile.position + new Vector3(0, 0.2f);
        List<Vector3> listTargetPosition = new List<Vector3>();
        switch (listUnit.Count)
        {
            case 1:
                listTargetPosition.Add(basePos);
                break;
            case 2:
                listTargetPosition.Add(basePos + new Vector3(-0.2f, 0, 0));
                listTargetPosition.Add(basePos + new Vector3(0.2f, 0, 0));
                break;
            case 3:
                listTargetPosition.Add(basePos + new Vector3(-0.2f, 0.1f, 0));
                listTargetPosition.Add(basePos + new Vector3(0.2f, 0.1f, 0));
                listTargetPosition.Add(basePos + new Vector3(0, -0.1f, 0));
                break;
            default:
                break;
        }
        return listTargetPosition;
    }

    #region Public
    public void SetMyEnemyWayPoint(Transform[] wayPoints)
    {
        _myEnemyWayPoint = wayPoints;
    }
    public void SetAIEnemyWayPoint(Transform[] wayPoints)
    {
        _aiEnemyWayPoint = wayPoints;
    }
    public void ReturnEnemy(CAEnemyController enemy)
    {
        if (_hashSpawnedEnemy.Contains(enemy) == false)
            return;
        PrevEnemyCount = _hashSpawnedEnemy.Count;
        _hashSpawnedEnemy.Remove(enemy);
        int addGold = 0;
        int addDiamond = 0;
        if (enemy.IsBoss)
        {
            addGold = CurWave * 5;
            addDiamond = CurWave / 10 + 1;
            if (_hashSpawnedEnemy.Count(enemy => enemy.IsBoss) == 0)
            {
                RemainTime = CAConstants.START_ROUND_DELAY;
                if (_pageBattle)
                {
                    _pageBattle.KilledBoss();
                }
            }
        }
        else
            addGold = CurWave / 10 + 1;
        PrevGold = MyGold;
        PrevDiamond = MyDiamond;
        MyGold += addGold;
        MyDiamond += addDiamond;
        AddMoneyForAI?.Invoke(addGold, addDiamond);
        ChangedMyMoney?.Invoke();
        ChangedEnemyCount?.Invoke();
        EnemyDeadCallBack?.Invoke(enemy);

        StartCoroutine(EnemyDeadCo(enemy));
    }
    private IEnumerator EnemyDeadCo(CAEnemyController enemy)
    {
        yield return _enemyDeadDelay;

        if (_enemySpawner)
        {
            _enemySpawner.ReturnEnemy(enemy);
        }
    }
    public List<(CAUnitGrade, CAUnitType)> GetOwnedUnitsForMythicCombination(string mythicKey)
    {
        return CAMythicCombinationManager.Instance.GetOwnedUnitsForMythicCombination(mythicKey, _dicSpawnedUnit);
    }
    public List<(CAUnitGrade, CAUnitType)> GetOwnedUnitsForMythicCombination(string mythicKey, Dictionary<(CAUnitGrade, CAUnitType), int> dicSpawnedUnit)
    {
        return CAMythicCombinationManager.Instance.GetOwnedUnitsForMythicCombination(mythicKey, dicSpawnedUnit);
    }
    public bool OnGambling(CAUnitGrade gambleGrade)
    {
        GambleData = default;
        CAUnitType randomType = (UnityEngine.Random.value < 0.5f) ? CAUnitType.Type1 : CAUnitType.Type2;

        UnitTile spawnTile = GetNextSpawnTile(gambleGrade, randomType);
        if (spawnTile == null)
            return false;

        float gambleRate = 0f;
        int costDiamond = 0;
        switch (gambleGrade)
        {
            case CAUnitGrade.Rare:
                {
                    gambleRate = CAConstants.GAMBLE_RATE_RARE;
                    costDiamond = CAConstants.GAMBLE_COST_RARE;
                }
                break;
            case CAUnitGrade.Hero:
                {
                    gambleRate = CAConstants.GAMBLE_RATE_HERO;
                    costDiamond = CAConstants.GAMBLE_COST_HERO;
                }
                break;
            default:
                return false;
        }
        if (MyDiamond < costDiamond)
            return false;
        PrevDiamond = MyDiamond;
        MyDiamond -= costDiamond;
        ChangedMyMoney?.Invoke();
        bool success = UnityEngine.Random.Range(0, 100) < gambleRate;
        if (!success)
            return false;

        GambleData = (gambleGrade, randomType, spawnTile);
        return true;
    }
    public void SpawnGambleUnit()
    {
        if (GambleData == default)
            return;

        CAUnitController spawnedUnit = _unitSpawner.SpawnUnit(GambleData.Item1, GambleData.Item2, ref GambleData.Item3);
        CAUnit unitData = new CAUnit(GambleData.Item1, GambleData.Item2);
        spawnedUnit.Initialize(ref EnemyDeadCallBack);

        var key = (GambleData.Item1, GambleData.Item2);
        if (_dicSpawnedUnit.ContainsKey(key))
            _dicSpawnedUnit[key]++;
        else
            _dicSpawnedUnit.Add(key, 1);

        MyUnitCount++;
        ChangedUnitCount?.Invoke();

        GambleData = default;
    }
    #endregion

    #region SpawnUnit
    public void SpawnUnit()
    {
        if (MyGold < CurSpawnCost)
            return;
        if (MyUnitCount == CAConstants.UNIT_MAX_COUNT)
            return;

        CAUnitGrade grade = GetRandomUnitGrade();
        CAUnitType type = GetRandomUnitType();

        UnitTile spawnTile = GetNextSpawnTile(grade, type);
        if (spawnTile != null)
        {
            CAUnitController spawnedUnit = _unitSpawner.SpawnUnit(grade, type, ref spawnTile);
            CAUnit unitData = new CAUnit(grade, type);
            spawnedUnit.Initialize(ref EnemyDeadCallBack);
            if(_dicSpawnedUnit.ContainsKey((grade, type)))
                ++_dicSpawnedUnit[(grade, type)];
            else
                _dicSpawnedUnit.Add((grade, type), 1);

            PrevGold = MyGold;
            MyGold -= CurSpawnCost;
            ++MyUnitCount;
            CurSpawnCost += CAConstants.INCREASE_SPAWN_GOLD;
            ChangedMyMoney?.Invoke();
            ChangedSpawnCost?.Invoke();
            ChangedUnitCount?.Invoke();
        }
    }
    public void SpawnMythicUnit(CAUnitType type, List<(CAUnitGrade, CAUnitType)> requiredUnitKeys)
    {
        CAUnitGrade grade = CAUnitGrade.Mythic;

        foreach (var unitKey in requiredUnitKeys)
        {
            if (_dicSpawnedUnit.TryGetValue(unitKey, out int count))
            {
                --count;
                if (count <= 0)
                    _dicSpawnedUnit.Remove(unitKey);
                else
                    _dicSpawnedUnit[unitKey] = count;
            }
            foreach (var tile in _listMyUnitTile)
            {
                if (tile.ContainsUnit(unitKey.Item1, unitKey.Item2))
                {
                    CAUnitController removedUnit = tile.RemoveUnit();
                    Destroy(removedUnit.gameObject);
                }
            }
            --MyUnitCount;
        }
        UnitTile spawnTile = GetNextSpawnTile(grade, type);
        if (spawnTile != null)
        {
            CAUnitController spawnedUnit = _unitSpawner.SpawnUnit(grade, type, ref spawnTile);
            CAUnit unitData = new CAUnit(grade, type);
            spawnedUnit.Initialize(ref EnemyDeadCallBack);
            if (_dicSpawnedUnit.ContainsKey((grade, type)))
                ++_dicSpawnedUnit[(grade, type)];
            else
                _dicSpawnedUnit.Add((grade, type), 1);

            ++MyUnitCount;
        }
        if (_pageBattle)
            _pageBattle.SetMythicAlert(type);
        ChangedUnitCount?.Invoke();
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


    public void SetTilesTransform(Transform[] tiles)
    {
        _listMyUnitTile = new List<UnitTile>();
        for (int i = 0; i < tiles.Length; ++i)
        {
            UnitTile tile = new UnitTile(tiles[i]);
            _listMyUnitTile.Add(tile);
        }
    }
    public UnitTile GetNextSpawnTile(CAUnitGrade grade, CAUnitType type)
    {
        if (grade != CAUnitGrade.Mythic)
        {
            foreach (UnitTile tile in _listMyUnitTile)
            {
                if (tile.ContainsUnit(grade, type) && tile.GetUnitCount() < 3)
                {
                    return tile;
                }
            }
        }

        foreach (UnitTile tile in _listMyUnitTile)
        {
            if (tile.GetUnitCount() == 0)
            {
                return tile;
            }
        }

        return null;
    }
    #endregion
    #region Unit Upgrade/Sell
    public void ShowTileUnitInfo(Transform clickedTile)
    {
        _clickedTile = _listMyUnitTile.FirstOrDefault(tile => tile.PlacedTile == clickedTile);

        if (_clickedTile == null)
        {
            _pageBattle.CloseUnitControl();
            return;
        }

        int unitCount = _clickedTile.GetUnitCount();

        if (unitCount > 0)
        {
            CAUnitGrade unitGrade = _clickedTile.GetUnitGrade();
            if(unitGrade != CAUnitGrade.Mythic)
                _pageBattle.ShowUnitControl(clickedTile);
        }
        else
            _pageBattle.CloseUnitControl();
    }
    public void UpgradeUnit()
    {
        if (_clickedTile == null)
            return;

        if (_clickedTile.GetUnitCount() != 3)
            return;

        CAUnitGrade currentGrade = _clickedTile.GetUnitGrade();
        CAUnitType currentType = _clickedTile.GetUnitType();

        for (int i = 0; i < 3; i++)
        {
            CAUnitController removedUnit = _clickedTile.RemoveUnit();
            if (removedUnit != null)
            {
                Destroy(removedUnit.gameObject);
            }
        }

        var key = (currentGrade, currentType);
        if (_dicSpawnedUnit.ContainsKey(key))
        {
            _dicSpawnedUnit[key] -= 3;
            if (_dicSpawnedUnit[key] <= 0)
            {
                _dicSpawnedUnit.Remove(key);
            }
        }

        CAUnitGrade upgradedGrade = GetUpgradedGrade(currentGrade);
        CAUnitType upgradedType = (UnityEngine.Random.value < 0.5f) ? CAUnitType.Type1 : CAUnitType.Type2;


        PlaceSynthesizedUnit(upgradedGrade, upgradedType, ref _clickedTile);
        CAUnitController synthesizedUnit = _unitSpawner.SpawnUnit(upgradedGrade, upgradedType, ref _clickedTile);
        CAUnit unitData = new CAUnit(upgradedGrade, upgradedType);
        synthesizedUnit.Initialize(ref EnemyDeadCallBack);

        var upgradedKey = (upgradedGrade, upgradedType);
        if (_dicSpawnedUnit.ContainsKey(upgradedKey))
        {
            _dicSpawnedUnit[upgradedKey] += 1;
        }
        else
        {
            _dicSpawnedUnit.Add(upgradedKey, 1);
        }

        int index = _listMyUnitTile.FindIndex(tile => tile.PlacedTile == _clickedTile.PlacedTile);
        if (index >= 0)
        {
            _listMyUnitTile[index] = _clickedTile;
        }

        MyUnitCount -= 2;
        ChangedUnitCount?.Invoke();
        _clickedTile = null;
    }
    public void SellUnit()
    {
        if (_clickedTile == null)
            return;

        if (_clickedTile.GetUnitCount() == 0)
            return;

        CAUnitGrade currentGrade = _clickedTile.GetUnitGrade();
        CAUnitType currentType = _clickedTile.GetUnitType();

        CAUnitController removedUnit = _clickedTile.RemoveUnit();
        if (removedUnit != null)
        {
            int priceGold = 0;
            int priceDiamond = 0;

            switch (removedUnit.GetUnitData().UnitGrade)
            {
                case CAUnitGrade.Normal:
                    priceGold = CAConstants.SELL_PRICE_NORMAL;
                    break;
                case CAUnitGrade.Rare:
                    priceDiamond = CAConstants.SELL_PRICE_RARE;
                    break;
                case CAUnitGrade.Hero:
                    priceDiamond = CAConstants.SELL_PRICE_HERO;
                    break;
                default:
                    break;
            }
            PrevGold = MyGold;
            PrevDiamond = MyDiamond;
            MyGold += priceGold;
            MyDiamond += priceDiamond;
            --MyUnitCount;
            ChangedMyMoney?.Invoke();
            ChangedUnitCount?.Invoke();

            Destroy(removedUnit.gameObject);

            var key = (currentGrade, currentType);
            if (_dicSpawnedUnit.ContainsKey(key))
            {
                --_dicSpawnedUnit[key];
                if (_dicSpawnedUnit[key] <= 0)
                {
                    _dicSpawnedUnit.Remove(key);
                }
            }
        }
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
    public void PlaceSynthesizedUnit(CAUnitGrade grade, CAUnitType type, ref UnitTile curTile)
    {
        foreach (UnitTile tile in _listMyUnitTile)
        {
            if (tile != curTile && tile.ContainsUnit(grade, type) && tile.GetUnitCount() < 3)
            {
                curTile = tile;

                return;
            }
        }
    }
    #endregion

    #region UI

    public void SetDamageUI(Transform target, int damage)
    {
        CAUIManager.Instance.LoadUIController(CAUIAddress.CAUIDamage, (CAUIController uiController) => 
        {
            if(uiController != null && uiController is CAUIDamage uiDamage)
            {
                uiDamage.SetDamage(target, damage);
            }
        });
    }

    #endregion
}