using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CA.UI
{
    public class CAUIAddDiamond : CAUIController
    {
        public TextMeshProUGUI _txtAmount;
        public DOTweenAnimation _tweenAnim;

        public void SetAmount(RectTransform rect, int amount)
        {
            RectTransform uiRect = GetComponent<RectTransform>();
            uiRect.position = new Vector3(rect.position.x, rect.position.y + CAConstants.UI_MONEY_OFFSET);
            if (_txtAmount)
            {
                if (amount > 0)
                    _txtAmount.SetText("+" + amount.ToString());
                else
                    _txtAmount.SetText(amount.ToString());
            }

            StartCoroutine(ShowUICo());
        }
        private IEnumerator ShowUICo()
        {
            if (_tweenAnim)
                _tweenAnim.DORestart();

            float duration = CAConstants.DAMAGE_DISPLAY_TIME;
            float elapsedTime = 0f;
            while (duration > elapsedTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            CAUIManager.Instance.CloseUIController(this);
        }
    }
}