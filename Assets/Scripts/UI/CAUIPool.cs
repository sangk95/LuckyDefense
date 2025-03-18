using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CA.UI
{
    public class CAUIPool : MonoBehaviour
    {
        private Dictionary<CAUIAddress, GameObject> _dicUIObjectPool = new Dictionary<CAUIAddress, GameObject>();
        public Dictionary<CAUIAddress, Queue<GameObject>> _dicReusableUI = new Dictionary<CAUIAddress, Queue<GameObject>>();

        public Action<CAUIAddress, GameObject, Action<CAUIController>> LoadComplete;
        public GameObject ObjectDamage;
        public GameObject ObjectAddGold;
        public GameObject ObjectAddDiamond;

        public void GetUI(CAUIAddress address, Action<CAUIController> callBack)
        {
            GameObject uiObj = null; 
            GameObject prefab = address switch
            {
                CAUIAddress.CAUIDamage => ObjectDamage,
                CAUIAddress.CAUIAddGold => ObjectAddGold,
                CAUIAddress.CAUIAddDiamond => ObjectAddDiamond,
                _ => null
            };
            if (prefab != null)
            {
                if (_dicReusableUI.TryGetValue(address, out var uiQueue) && uiQueue != null && uiQueue.Count > 0)
                    uiObj = uiQueue.Dequeue();
                else
                    uiObj = Instantiate(prefab);

                if (uiObj != null)
                {
                    LoadComplete?.Invoke(address, uiObj, callBack);
                    return;
                }
            }

            if (_dicUIObjectPool.TryGetValue(address, out GameObject uiObject) == false)
            {
                Addressables.LoadAssetAsync<GameObject>(address.ToString()).Completed += (handle) =>
                {
                    if (handle.Result == null)
                        return;

                    GameObject uiObj = Instantiate(handle.Result);
                    LoadComplete?.Invoke(address, uiObj, callBack);
                };
            }
            else
            {
                LoadComplete?.Invoke(address, uiObject, callBack);
             
                _dicUIObjectPool.Remove(address);
            }
        }

        public void ReturnUI(CAUIAddress address, GameObject obj)
        {
            obj.SetActive(false);

            if (address == CAUIAddress.CAUIDamage || address == CAUIAddress.CAUIAddGold || address == CAUIAddress.CAUIAddDiamond)
            {
                if (_dicReusableUI.TryGetValue(address, out Queue<GameObject> uiQueue))
                {
                    uiQueue.Enqueue(obj);
                }
                else
                {
                    uiQueue = new Queue<GameObject>();
                    uiQueue.Enqueue(obj);
                    _dicReusableUI[address] = uiQueue;
                }
                return;
            }

            if (_dicUIObjectPool.ContainsKey(address) == true)
            {
                _dicUIObjectPool[address] = obj;
            }
            else
            {
                _dicUIObjectPool.Add(address, obj);
            }
        }

        public void DestroyUI(CAUIAddress address)
        {
            if (_dicUIObjectPool.TryGetValue(address, out GameObject obj) == false)
                return;
            else
            {
                DestroyImmediate(obj);
                _dicUIObjectPool.Remove(address);
            }
        }
    }
}