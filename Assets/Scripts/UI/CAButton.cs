using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using TMPro;
using UnityEngine.UI;

namespace CA.UI
{
    public class CAButton : CAUIBase, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        private Dictionary<CAUIState, int> m_dicHashCode = new Dictionary<CAUIState, int>();
        private Dictionary<CAUIState, System.Object> m_dicObject = new Dictionary<CAUIState, object>();
        private CAUIState m_curState;
        private bool m_isDisabled;
        private bool m_isSelected;
        private static DateTime m_lastClick = DateTime.MinValue;
        private const float CLICK_DELAY = 200;
        private bool _isInitialized = false;

        private Dictionary<CAUIState, GameObject> m_dicStateObject = new Dictionary<CAUIState, GameObject>();

        public Action<CAUIState, int, object> CallBack;
        public List<TextMeshProUGUI> m_listText = new List<TextMeshProUGUI>();
        public TextMeshProUGUI _txtName;
        public GameObject m_objNormal;
        public GameObject m_objHover;
        public GameObject m_objClick;
        public GameObject m_objDisable;
        public GameObject m_objSelect;

        public override void UIPreInit()
        {
            if (_isInitialized)
                return;

            m_lastClick = DateTime.Now;
            m_dicHashCode = new Dictionary<CAUIState, int>();
            m_dicObject = new Dictionary<CAUIState, object>();
            m_dicStateObject = new Dictionary<CAUIState, GameObject>();
            m_curState = CAUIState.None;
            m_isDisabled = false;
            m_isSelected = false;

            if (m_objNormal)
            {
                m_objNormal.SetActive(true);
                m_dicStateObject.Add(CAUIState.Normal, m_objNormal);
            }
            if (m_objHover)
            {
                m_objHover.SetActive(false);
                m_dicStateObject.Add(CAUIState.Hover, m_objHover);
            }
            if (m_objClick)
            {
                m_objClick.SetActive(false);
                m_dicStateObject.Add(CAUIState.Click, m_objClick);
            }
            if (m_objDisable)
            {
                m_objDisable.SetActive(false);
                m_dicStateObject.Add(CAUIState.Disable, m_objDisable);
            }
            if (m_objSelect)
            {
                m_objSelect.SetActive(false);
            }
            if (m_listText != null)
            {
                for (int i = 0; i < m_listText.Count; ++i)
                    m_listText[i].gameObject.SetActive(false);
            }

            _isInitialized = true;
        }

        protected override void UIEnd()
        {
            CallBack = null;
        }

        protected override void OnEnable()
        {
            SetState(CAUIState.Normal);
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            if (m_isDisabled == true || m_isSelected == true)
                return;

            SetState(CAUIState.Normal);
            CheckSendCallBack();
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_isDisabled == true)
                return;
            if (m_curState == CAUIState.Click)
                return;

            SetState(CAUIState.Hover);
            CheckSendCallBack();
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_isDisabled == true || m_isSelected == true)
                return;
            if (m_curState == CAUIState.Click)
                return;

            SetState(CAUIState.Click);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_isDisabled == true || m_isSelected == true)
                return;

            CheckSendCallBack();


            SetState(CAUIState.Hover);
        }
        private void CheckSendCallBack()
        {
            if (m_dicHashCode.TryGetValue(m_curState, out int hashCode) == true)
            {
                //if ((DateTime.Now - m_lastClick).Milliseconds > CLICK_DELAY)
                {
                    if (m_dicObject.TryGetValue(m_curState, out object _obj) == true)
                        CallBack?.Invoke(m_curState, hashCode, _obj);
                    else
                        CallBack?.Invoke(m_curState, hashCode, null);
                    m_lastClick = DateTime.Now;
                }
                //else
                //{
                //    Debug.LogWarning("Click delayed" + (DateTime.Now - m_lastClick).Milliseconds);
                //}
            }
        }

        public void AddUICallBack(CAUIState state, int hashCode, object obj = null)
        {
            UIPreInit();
            if (m_dicHashCode.ContainsKey(state) == true)
                m_dicHashCode[state] = hashCode;
            else
                m_dicHashCode.Add(state, hashCode);

            if (m_dicObject.ContainsKey(state) == true)
                m_dicObject[state] = obj;
            else
                m_dicObject.Add(state, obj);
        }
        public void SetDisable(bool flag)
        {
            m_isDisabled = flag;

            if(flag)
                SetState(CAUIState.Disable);
            else
                SetState(CAUIState.Normal);
        }
        public void SetSelect(bool flag)
        {
            m_isSelected = flag;
            if (flag)
                SetState(CAUIState.Selected);
            else
                SetState(CAUIState.Normal);
        }

        private void SetState(CAUIState state)
        {
            foreach (var obj in m_dicStateObject)
            {
                if (obj.Key == CAUIState.None || obj.Value == null)
                    continue;

                if (obj.Key == state)
                    obj.Value.SetActive(true);
                else
                    obj.Value.SetActive(false);
            }

            m_curState = state;
        }

        public void SetText(string text)
        {
            if (m_listText != null && m_listText.Count > 0)
            {
                for (int i = 0; i < m_listText.Count; ++i)
                {
                    m_listText[i].gameObject.SetActive(true);
                    m_listText[i].SetText(text);
                }
            }
            if (_txtName)
                _txtName.SetText(text);
        }
        public void SetTextColor(Color color)
        {
            if (m_listText != null && m_listText.Count > 0)
            {
                for (int i = 0; i < m_listText.Count; ++i)
                {
                    m_listText[i].color = color;
                }
            }
            if (_txtName)
                _txtName.color = color;
        }
    }
}