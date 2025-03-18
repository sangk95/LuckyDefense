using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppMain : MonoBehaviour
{
    List<CAManagerBase> _listManager = new List<CAManagerBase>();
    CASceneManager _sceneManager;
    CAUIManager _uiManager;
    CAStateManager _stateManager;

    CABattleManager _battleManager;
    CABattleAIManager _battleAIManager;
    CAMythicCombinationManager _combinationManager;
    private void Awake()
    {
        _sceneManager = GetComponent<CASceneManager>();
        _uiManager = GetComponent<CAUIManager>();
        _stateManager = GetComponent<CAStateManager>();
        _battleManager = GetComponent<CABattleManager>();
        _battleAIManager = GetComponent<CABattleAIManager>();
        _combinationManager = GetComponent<CAMythicCombinationManager>();

        _listManager.Add(_sceneManager);
        _listManager.Add(_uiManager);
        _listManager.Add(_stateManager);
        _listManager.Add(_battleManager);
        _listManager.Add(_battleAIManager);
        _listManager.Add(_combinationManager);
    }
    private void Start()
    {
        StartCoroutine(ManagerAwakeCo());
    }
    private IEnumerator ManagerAwakeCo()
    {
        for(int i=0; i<_listManager.Count; ++i)
        {
            if(_listManager[i] == null)
                continue;

            yield return StartCoroutine(_listManager[i].CAAwake());
        }

        StartCoroutine(ManagerStartCo());
    }
    private IEnumerator ManagerStartCo()
    {
        for(int i=0; i<_listManager.Count; ++i)
        {
            if(_listManager[i] == null)
                continue;

            yield return StartCoroutine(_listManager[i].CAStart());
        }
    }
}
