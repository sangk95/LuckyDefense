using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CA.UI
{
    public class CAUIDamage : CAUIController
    {
        private Transform _target;

        public TextMeshProUGUI _txtDamage;
        public DOTweenAnimation _tweenAnim;

        public void SetDamage(Transform target, int damage)
        {
            _target = target;
            if (_txtDamage)
                _txtDamage.SetText(damage.ToString());
            if (_tweenAnim)
                _tweenAnim.DORestart();
            StartCoroutine(ShowUICo());
        }
        private IEnumerator ShowUICo()
        {
            if (_tweenAnim)
                _tweenAnim.DORestart();

            float duration = CAConstants.DAMAGE_DISPLAY_TIME;
            float elapsedTime = 0f;

            if (_txtDamage)
                _txtDamage.gameObject.SetActive(true);
            RectTransform uiRect = GetComponent<RectTransform>();
            Vector3 offset = new Vector3(0, CAConstants.UI_DAMAGE_OFFSET);
            while (duration > elapsedTime)
            {
                uiRect.position = Camera.main.WorldToScreenPoint(_target.position) + offset;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            if (_txtDamage)
                _txtDamage.gameObject.SetActive(false);

            CAUIManager.Instance.CloseUIController(this);
        }
    }
}