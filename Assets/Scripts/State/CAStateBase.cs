using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CA.State
{
    public class CAStateBase
    {
        private CAStateProcess _curProcess = CAStateProcess.Init;
        public CAStateProcess CurProcess => _curProcess;

        public virtual void Init()
        {
            CASceneManager.Instance.OnLoadCompleteScene += OnLoadCompleteScene;
            _curProcess = CAStateProcess.Scene;
        }
        public virtual void LoadScene()
        {
            _curProcess = CAStateProcess.UI;
        }
        public virtual void LoadUI()
        {
            _curProcess = CAStateProcess.Player;
        }
        public virtual void LoadCharacter()
        {
            _curProcess = CAStateProcess.ETC;
        }

        public virtual void LoadETC()
        {
            _curProcess = CAStateProcess.End;
        }

        protected virtual void OnLoadCompleteScene()
        {
            CASceneManager.Instance.OnLoadCompleteScene -= OnLoadCompleteScene;
        }
    }
}