using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CAManagerBase : MonoBehaviour
{
    protected bool _awakeComplete = false;
    protected bool _startComplete = false;
    public virtual IEnumerator CAAwake()
    {
        _awakeComplete = true;

        yield return null;
    }
    public virtual IEnumerator CAStart()
    {
        _startComplete = true;

        yield return null;
    }
    protected virtual void OnDisable()
    {
        StopAllCoroutines();
    }
}
public class CAManagerBase<T> : CAManagerBase where T : MonoBehaviour
{
    protected static T _instance = null;

    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<T>();

            return _instance;
        }
    }
    public override IEnumerator CAAwake()
    {
        _instance = FindObjectOfType<T>();
        DontDestroyOnLoad(Instance);

        yield return StartCoroutine(base.CAAwake());
    }
}