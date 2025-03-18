using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class CASceneManager : CAManagerBase<CASceneManager>
{
    private Dictionary<CAScene, SceneInstance> _dicSceneInstance;
    private Queue<CAScene> _queueLateChangeScene;
    private CAScene _curLoadScene;
    private bool _isLoadProcess;

    public Action OnLoadCompleteScene;
    public Action OnUnLoadCompleteScene;

    public override IEnumerator CAAwake()
    {
        _dicSceneInstance = new Dictionary<CAScene, SceneInstance>();
        _queueLateChangeScene = new Queue<CAScene>();
        _curLoadScene = CAScene.None;
        _isLoadProcess = false;
        return base.CAAwake();
    }
    public void LoadSceneAsync(CAScene sceneType)
    {
        if(_isLoadProcess == true)
        {
            _queueLateChangeScene.Enqueue(sceneType);
            return;
        }
        if (_dicSceneInstance.ContainsKey(_curLoadScene) == true)
            return;

        _curLoadScene = sceneType;
        _isLoadProcess = true;

        string sceneName = sceneType.ToString();
        Addressables.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).Completed += LoadCompleteScene;
    }
    public void LoadSceneAsync_Single(CAScene sceneType)
    {
        if(_isLoadProcess == true)
        {
            _queueLateChangeScene.Enqueue(sceneType);
            return;
        }
        if (_dicSceneInstance.ContainsKey(_curLoadScene) == true)
            return;

        _curLoadScene = sceneType;
        _isLoadProcess = true;

        string sceneName = sceneType.ToString();
        Addressables.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single).Completed += LoadCompleteScene_Single;
    }
    public void UnLoadSceneAsync(CAScene sceneType)
    {
        if(_dicSceneInstance.ContainsKey(sceneType) == false)
        {
            Debug.LogError($"UnLoadSceneAsync - {sceneType} doesn't exist");
            return;
        }
        Addressables.UnloadSceneAsync(_dicSceneInstance[sceneType]).Completed += UnLoadCompleteScene;
    }
    private void LoadCompleteScene(AsyncOperationHandle<SceneInstance> handle)
    {
        if (handle.Status != AsyncOperationStatus.Succeeded ||
            handle.Result.Scene.IsValid() == false)
            return;

        _dicSceneInstance.Add(_curLoadScene, handle.Result);
        
        _curLoadScene = CAScene.None;
        _isLoadProcess = false;

        if (_queueLateChangeScene.Count > 0)
        {
            LoadSceneAsync(_queueLateChangeScene.Dequeue());
        }
        else
        {
            if (OnLoadCompleteScene != null)
                OnLoadCompleteScene.Invoke();
        }
    }
    private void LoadCompleteScene_Single(AsyncOperationHandle<SceneInstance> handle)
    {
        if (handle.Status != AsyncOperationStatus.Succeeded ||
            handle.Result.Scene.IsValid() == false)
            return;

        _dicSceneInstance.Clear();

        _dicSceneInstance.Add(_curLoadScene, handle.Result);
        
        _curLoadScene = CAScene.None;
        _isLoadProcess = false;

        if (_queueLateChangeScene.Count > 0)
        {
            LoadSceneAsync(_queueLateChangeScene.Dequeue());
        }
        else
        {
            if (OnLoadCompleteScene != null)
                OnLoadCompleteScene.Invoke();
        }
    }
    private void UnLoadCompleteScene(AsyncOperationHandle<SceneInstance> handle)
    {
        if (handle.Status != AsyncOperationStatus.Succeeded)
            return;

        if (_dicSceneInstance.ContainsValue(handle.Result) == false)
            return;
        foreach(var data in _dicSceneInstance)
        {
            if (data.Value.Scene != handle.Result.Scene)
                continue;
            _dicSceneInstance.Remove(data.Key);
            break;
        }
        if (OnUnLoadCompleteScene != null)
            OnUnLoadCompleteScene.Invoke();
    }
}
