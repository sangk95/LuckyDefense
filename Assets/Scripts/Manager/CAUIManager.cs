using CA.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class CAUIManager : CAManagerBase<CAUIManager>
{
    private const float WAITTIME = 3.0f;

    private WaitForSeconds m_waitTime;
    private Dictionary<CAUIAddress, GameObject> _dicUIController;
    private Dictionary<CAUIAddress, List<GameObject>> _dicReusableUI;
    private Dictionary<CAUIAddress, DateTime> m_dicCloseUITime; // has delay

    public GameObject _rootCanvas;
    public CAUIPool _uiPool;

    public override IEnumerator CAAwake()
    {
        _dicUIController = new Dictionary<CAUIAddress, GameObject>();
        _dicReusableUI = new Dictionary<CAUIAddress, List<GameObject>>();
        m_dicCloseUITime = new Dictionary<CAUIAddress, DateTime>();

        m_waitTime = new WaitForSeconds(WAITTIME);

        if(_uiPool)
            _uiPool.LoadComplete += LoadCompleteController;

        StartCoroutine(CheckCloseUI());

        return base.CAAwake();
    }
    private IEnumerator CheckCloseUI()
    {
        while(true)
        {
            if(m_dicCloseUITime.Count > 0)
            {
                foreach(var data in m_dicCloseUITime)
                {
                    if((DateTime.Now - data.Value).Milliseconds > WAITTIME)
                    {
                        _uiPool.DestroyUI(data.Key);
                        m_dicCloseUITime.Remove(data.Key);

                        break;
                    }
                }
            }

            yield return m_waitTime;
        }
    }
    public void LoadUIController(CAUIAddress address, Action<CAUIController> callBack = null)
    {
        if (address != CAUIAddress.CAUIDamage && address != CAUIAddress.CAUIAddGold && address != CAUIAddress.CAUIAddDiamond)
        {
            if (_dicUIController.ContainsKey(address) == true)
            {
                Debug.LogWarning("Already Loaded UI Address ==== " + address);
                return;
            }
        }
        _uiPool.GetUI(address, callBack);
    }

    private void LoadCompleteController(CAUIAddress address, GameObject obj, Action<CAUIController> callBack)
    {
        if (obj == null)
            return;
        obj.transform.SetParent(_rootCanvas.transform);

        if(m_dicCloseUITime.ContainsKey(address) == true)
        {
            m_dicCloseUITime.Remove(address);
        }

        CAUIController uiController = obj.GetComponent<CAUIController>();
        if (uiController == null)
            return;

        if (address == CAUIAddress.CAUIDamage || address == CAUIAddress.CAUIAddGold || address == CAUIAddress.CAUIAddDiamond)
        {
            if (_dicReusableUI.TryGetValue(address, out List<GameObject> uiList))
            {
                uiList.Add(obj);
            }
            else
            {
                uiList = new List<GameObject>();
                uiList.Add(obj);
                _dicReusableUI[address] = uiList;
            }
        }
        else
        {
            if (_dicUIController.ContainsKey(address) == true)
                _dicUIController[address] = obj;
            else
                _dicUIController.Add(address, obj);
        }
        RectTransform rect = obj.GetComponent<RectTransform>();
        Vector2 vector = Vector2.zero;
        rect.offsetMax = vector;
        rect.offsetMin = vector;
        obj.SetActive(true);

        if (uiController.IsInit() == false)
            uiController.UIInit();

        uiController.UIEnter();

        uiController.SetControllerAddress(address);

        callBack?.Invoke(uiController);
    }

    public void CloseUIController(CAUIController controller)
    {
        if (controller.m_address == CAUIAddress.CAUIDamage || controller.m_address == CAUIAddress.CAUIAddGold || controller.m_address == CAUIAddress.CAUIAddDiamond)
        {
            if (_dicReusableUI.TryGetValue(controller.m_address, out List<GameObject> uiList))
            {
                GameObject matchingObj = uiList.FirstOrDefault(uiObj => uiObj == controller.gameObject);
                if (matchingObj != null)
                {
                    controller.UIExit();
                    _uiPool.ReturnUI(controller.m_address, matchingObj);
                    uiList.Remove(matchingObj);
                }
            }
            return;
        }

        if (_dicUIController.TryGetValue(controller.m_address, out GameObject obj) == true)
        {
            controller.UIExit();

            m_dicCloseUITime[controller.m_address] = DateTime.Now;

            _uiPool.ReturnUI(controller.m_address, obj);

            _dicUIController.Remove(controller.m_address);
        }
        else
        {
            Debug.LogError("UIController is not using :" + controller.m_address);
            return;
        }
    }
    public void CloseUIController(CAUIAddress address)
    {
        if (_dicUIController.TryGetValue(address, out GameObject obj) == true)
        {
            obj.GetComponent<CAUIController>().UIExit();

            m_dicCloseUITime[address] = DateTime.Now;

            _uiPool.ReturnUI(address, obj);

            _dicUIController.Remove(address);
        }
        else
        {
            Debug.LogError("UIController is not using :" + address);
            return;
        }
    }
    public CAUIController GetUIController(CAUIAddress address)
    {
        if (_dicUIController.TryGetValue(address, out GameObject obj) == true)
        {
            CAUIController uiController = obj.GetComponent<CAUIController>();
            if (uiController != null)
                return uiController;
        }

        return null;
    }
}