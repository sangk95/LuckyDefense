using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CAEnemySpawner : MonoBehaviour
{
    public List<GameObject> _listEnemyObject = new List<GameObject>();
    public GameObject _objBoss;
    private Dictionary<int, List<CAEnemyController>> _dicEnemyPool = new Dictionary<int, List<CAEnemyController>>();
    private CAEnemyController CreateEnemy(int curLevel)
    {
        if (_listEnemyObject == null || (curLevel != int.MaxValue && _listEnemyObject.Count <= curLevel))
            return null;

        CAEnemyController controller = null;
        GameObject enemyObject = null;
        if (curLevel == int.MaxValue)
            enemyObject = Instantiate(_objBoss);
        else
            enemyObject = Instantiate(_listEnemyObject[curLevel]);
        if (enemyObject != null)
        {
            enemyObject.SetActive(false);
            controller = enemyObject.GetComponent<CAEnemyController>();
        }
        return controller;
    }
    public List<CAEnemyController> GetEnemies(int curWave)
    {
        List<CAEnemyController> newEnemies = new List<CAEnemyController>();
        int totalEnemyCount = CAConstants.ENEMY_COUNT_PER_ROUND;
        int curLevel = curWave / 10;

        if (curWave % 10 == 0)
        {
            curLevel = int.MaxValue;
            totalEnemyCount = 2;
        }

        if (_dicEnemyPool.TryGetValue(curLevel, out List<CAEnemyController> enemies))
        {
            int addCount = Mathf.Min(enemies.Count, totalEnemyCount);
            newEnemies.AddRange(enemies.Take(addCount));
            enemies.RemoveRange(0, addCount);
            totalEnemyCount -= addCount;
        }

        for (int i = 0; i < totalEnemyCount; ++i)
        {
            CAEnemyController controller = CreateEnemy(curLevel);
            newEnemies.Add(controller);
        }

        return newEnemies;
    }
    public void ReturnEnemy(CAEnemyController controller)
    {
        controller.gameObject.SetActive(false);
        int curLevel = controller.MyWave / 10;

        if (controller.MyWave % 10 == 0)
            curLevel = int.MaxValue;

        if (_dicEnemyPool.ContainsKey(curLevel))
        {
            _dicEnemyPool[curLevel].Add(controller);
        }
        else
        {
            List<CAEnemyController> listPool = new List<CAEnemyController>{ controller };
            _dicEnemyPool.Add(curLevel, listPool);
        }
    }
}
