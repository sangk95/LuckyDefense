using CA.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAStateManager : CAManagerBase<CAStateManager>
{
    CAStateBase _curState;

    public override IEnumerator CAAwake()
    {
        _curState = new CALobbyState();

        StartCoroutine(ChangeStateCo());

        return base.CAAwake();
    }
    public void ChangeState(CAGameState gameState)
    {
        switch(gameState)
        {
            case CAGameState.State_Lobby:
                {
                    _curState = new CALobbyState();
                }
                break;
            case CAGameState.State_Battle:
                {
                    _curState = new CABattleState();
                }
                break;

            default:
                Debug.LogError("Wrong GameState - " + gameState);
                return;
        }
        StartCoroutine(ChangeStateCo());
    }
    private IEnumerator ChangeStateCo()
    {
        if (_curState == null)
            yield break;

        _curState.Init();
        yield return new WaitUntil(() => _curState.CurProcess == CAStateProcess.Scene);

        _curState.LoadScene();
        yield return new WaitUntil(() => _curState.CurProcess == CAStateProcess.UI);

        _curState.LoadUI();
        yield return new WaitUntil(() => _curState.CurProcess == CAStateProcess.Player);

        _curState.LoadCharacter();
        yield return new WaitUntil(() => _curState.CurProcess == CAStateProcess.ETC);
        
        _curState.LoadETC();
        yield return new WaitUntil(() => _curState.CurProcess == CAStateProcess.End);
    }
}
