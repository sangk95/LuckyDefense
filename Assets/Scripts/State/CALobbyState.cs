using CA.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CA.State
{
    public class CALobbyState : CAStateBase
    {
        public override void Init()
        {
            base.Init();
        }
        public override void LoadScene()
        {
            CASceneManager.Instance.LoadSceneAsync_Single(CAScene.Lobby);
        }

        protected override void OnLoadCompleteScene()
        {
            base.OnLoadCompleteScene();
            base.LoadScene();
        }
        public override void LoadUI()
        {
            CAUIManager.Instance.LoadUIController(CAUIAddress.CAPage_LobbyMain, AsyncLoadUI);
        }
        public void AsyncLoadUI(CAUIController uiController)
        {
            if (uiController == null || (uiController is CAPageLobbyMain) == false)
                return;
            base.LoadUI();
        }
        public override void LoadCharacter()
        {
            base.LoadCharacter();
        }
        public override void LoadETC()
        {
            base.LoadETC();
        }
    }
}