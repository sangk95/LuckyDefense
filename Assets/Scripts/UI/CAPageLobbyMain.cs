using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CA.UI
{
    public class CAPageLobbyMain : CAUIController
    {
        private static string _strClick_StartGame = "Click StartGame";

        [Header("Buttons")]
        public CAButton _btnStartGame;

        public override void UIInit()
        {
            base.UIInit();
            if(_btnStartGame != null)
            {
                _btnStartGame.AddUICallBack(CAUIState.Click, _strClick_StartGame.GetHashCode());
                _btnStartGame.CallBack += UICallBack;
                _btnStartGame.SetText("GameStart");
            }
        }
        public override void UIEnter()
        {
            base.UIEnter();
        }

        protected override void UICallBack(CAUIState state, int hashCode, object obj = null)
        {
            base.UICallBack(state, hashCode, obj);

            switch(state)
            {
                case CAUIState.Click:
                    {
                        if (hashCode == _strClick_StartGame.GetHashCode())
                        {
                            CAStateManager.Instance.ChangeState(CAGameState.State_Battle);
                        }
                    }
                    break;
            }
        }
    }
}