using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CA.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CA.State
{
    public class CABattleState : CAStateBase
    {
        public override void Init()
        {
            base.Init();
        }
        public override void LoadScene()
        {
            CASceneManager.Instance.LoadSceneAsync_Single(CAScene.BattleScene);
        }

        protected override void OnLoadCompleteScene()
        {
            base.OnLoadCompleteScene();
            base.LoadScene();
        }
        public override void LoadUI()
        {
            CAUIManager.Instance.LoadUIController(CAUIAddress.CAPage_Battle, (CAUIController ui)=> 
            {
                CAUIManager.Instance.CloseUIController(CAUIAddress.CAPage_LobbyMain);
                base.LoadUI();
            });            
        }
        public override void LoadCharacter()
        {
            base.LoadCharacter();
        }
        public override void LoadETC()
        {
            Transform[] _transMyEnemyWayPoint = new Transform[5];
            Transform[] _transAIEnemyWayPoint = new Transform[5];
            Transform[] _transMyUnitTile = new Transform[20];
            Transform[] _transAIUnitTile = new Transform[20];
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene == null || scene.IsValid() == false)
                    continue;

                if (scene.name == "BattleScene")
                {
                    GameObject[] rootObjects = scene.GetRootGameObjects();
                    if (rootObjects == null || rootObjects.Length == 0)
                        continue;
                    for (int index = 0; index < rootObjects.Length; ++index)
                    {
                        Transform myEnemyWayPoint = CAUtils.FindObjectInChildren<Transform>(rootObjects[index], CAConstants.MY_ENEMY_TRANSFORM);
                        if (myEnemyWayPoint != null)
                        {
                            _transMyEnemyWayPoint = myEnemyWayPoint
                                .GetComponentsInChildren<Transform>()
                                .Where(t => t != myEnemyWayPoint)
                                .ToArray();
                        }
                        Transform aiEnemyWayPoint = CAUtils.FindObjectInChildren<Transform>(rootObjects[index], CAConstants.AI_ENEMY_TRANSFORM);
                        if (aiEnemyWayPoint != null)
                        {
                            _transAIEnemyWayPoint = aiEnemyWayPoint
                                .GetComponentsInChildren<Transform>()
                                .Where(t => t != aiEnemyWayPoint)
                                .ToArray();
                        }
                        Transform myUnitTiles = CAUtils.FindObjectInChildren<Transform>(rootObjects[index], CAConstants.MY_UNIT_TILE_TRANSFORM);
                        if (myUnitTiles != null)
                        {
                            _transMyUnitTile = myUnitTiles
                                .GetComponentsInChildren<Transform>()
                                .Where(t => t != myUnitTiles)
                                .ToArray();
                        }
                        Transform aiUnitTiles = CAUtils.FindObjectInChildren<Transform>(rootObjects[index], CAConstants.AI_UNIT_TILE_TRANSFORM);
                        if (aiUnitTiles != null)
                        {
                            _transAIUnitTile = aiUnitTiles
                                .GetComponentsInChildren<Transform>()
                                .Where(t => t != aiUnitTiles)
                                .ToArray();
                        }
                    }
                }
            }
            
            CABattleManager.Instance.StartBattlePhase();
            CABattleManager.Instance.SetMyEnemyWayPoint(_transMyEnemyWayPoint);
            CABattleManager.Instance.SetAIEnemyWayPoint(_transAIEnemyWayPoint);
            CABattleManager.Instance.SetTilesTransform(_transMyUnitTile);

            CABattleAIManager.Instance.StartAIBattlePhase();
            CABattleAIManager.Instance.SetTilesTransform(_transAIUnitTile);

            base.LoadETC();
        }
    }
}