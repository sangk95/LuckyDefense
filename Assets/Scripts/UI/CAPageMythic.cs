using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CA.UI
{
    public class CAPageMythic : CAUIController
    {
        private const string _strClick_Close = "Click Close";
        private const string _strClick_Combine = "Click Combine";
        private const string _strClick_Mythic01 = "Click Mythic01";
        private const string _strClick_Mythic02 = "Click Mythic02";

        [Header("Text")]
        public TextMeshProUGUI _txtUnitName;
        public TextMeshProUGUI _txtPercent;
        public TextMeshProUGUI _txtIsOwnedUnit_Normal;
        public TextMeshProUGUI _txtIsOwnedUnit_Rare;
        public TextMeshProUGUI _txtIsOwnedUnit_Hero;
        public TextMeshProUGUI _txtProgress_Mythic01;
        public TextMeshProUGUI _txtProgress_Mythic02;
        public TextMeshProUGUI _txtProgress_TargetMythic;

        [Header("Button")]
        public CAButton _btnClose;
        public CAButton _btnCombine;
        public CAButton _btnMythic01;
        public CAButton _btnMythic02;

        [Header("Image")]
        public Image _imgUnitNormal;
        public Image _imgUnitRare;
        public Image _imgUnitHero;
        public Image _imgTargetMythic;

        [Header("Resource")]
        public List<Sprite> _listSpriteResource_Normal;
        public List<Sprite> _listSpriteResource_Rare;
        public List<Sprite> _listSpriteResource_Hero;

        private string _selectedUnit = "";
        private CAUnitType _selectedType;
        private List<(CAUnitGrade, CAUnitType)> _listOwnedUnit;

        public override void UIInit()
        {
            base.UIInit();
            if (_btnClose != null)
            {
                _btnClose.AddUICallBack(CAUIState.Click, _strClick_Close.GetHashCode());
                _btnClose.CallBack += UICallBack;
            }
            if (_btnCombine != null)
            {
                _btnCombine.AddUICallBack(CAUIState.Click, _strClick_Combine.GetHashCode());
                _btnCombine.CallBack += UICallBack;
            }
            if (_btnMythic01 != null)
            {
                _btnMythic01.AddUICallBack(CAUIState.Click, _strClick_Mythic01.GetHashCode());
                _btnMythic01.CallBack += UICallBack;
            }
            if (_btnMythic02 != null)
            {
                _btnMythic02.AddUICallBack(CAUIState.Click, _strClick_Mythic02.GetHashCode());
                _btnMythic02.CallBack += UICallBack;
            }
        }
        public override void UIEnter()
        {
            base.UIEnter();
            
            _selectedUnit = CAConstants.SELECT_MYTHIC_01;
            _selectedType = CAUnitType.Type1;
            if(_txtUnitName)
                _txtUnitName.SetText("Mythic01");
            if (_btnMythic01)
                _btnMythic01.m_objSelect.SetActive(true);
            if (_btnMythic02)
                _btnMythic02.m_objSelect.SetActive(false);

            int progress = 0;
            var ownList = CABattleManager.Instance.GetOwnedUnitsForMythicCombination(CAConstants.SELECT_MYTHIC_01);
            foreach (var data in ownList)
            {
                if (data.Item1 == CAUnitGrade.Normal)
                    progress += CAConstants.COMBINE_WEIGHT_NORMAL;
                else if (data.Item1 == CAUnitGrade.Rare)
                    progress += CAConstants.COMBINE_WEIGHT_RARE;
                else if (data.Item1 == CAUnitGrade.Hero)
                    progress += CAConstants.COMBINE_WEIGHT_HERO;
            }
            if (_txtProgress_Mythic01)
                _txtProgress_Mythic01.SetText(progress.ToString() + "%");
            progress = 0;
            ownList = CABattleManager.Instance.GetOwnedUnitsForMythicCombination(CAConstants.SELECT_MYTHIC_02);
            foreach (var data in ownList)
            {
                if (data.Item1 == CAUnitGrade.Normal)
                    progress += CAConstants.COMBINE_WEIGHT_NORMAL;
                else if (data.Item1 == CAUnitGrade.Rare)
                    progress += CAConstants.COMBINE_WEIGHT_RARE;
                else if (data.Item1 == CAUnitGrade.Hero)
                    progress += CAConstants.COMBINE_WEIGHT_HERO;
            }
            if (_txtProgress_Mythic02)
                _txtProgress_Mythic02.SetText(progress.ToString() + "%");

            SetCombinationData();
        }
        public override void UIExit()
        {
            base.UIExit();

        }
        protected override void UICallBack(CAUIState state, int hashCode, object obj = null)
        {
            base.UICallBack(state, hashCode, obj);

            switch (state)
            {
                case CAUIState.Click:
                    {
                        if (hashCode == _strClick_Close.GetHashCode())
                        {
                            CAUIManager.Instance.CloseUIController(this);
                        }
                        else if (hashCode == _strClick_Combine.GetHashCode())
                        {
                            CABattleManager.Instance.SpawnMythicUnit(_selectedType, _listOwnedUnit);
                            CAUIManager.Instance.CloseUIController(this);
                        }
                        else if (hashCode == _strClick_Mythic01.GetHashCode())
                        {
                            if (_btnMythic01)
                                _btnMythic01.m_objSelect.SetActive(true);
                            if (_btnMythic02)
                                _btnMythic02.m_objSelect.SetActive(false);
                            _selectedUnit = CAConstants.SELECT_MYTHIC_01;
                            _selectedType = CAUnitType.Type1;
                            _txtUnitName.SetText("Mythic01");
                            SetCombinationData();
                        }
                        else if (hashCode == _strClick_Mythic02.GetHashCode())
                        {
                            if (_btnMythic01)
                                _btnMythic01.m_objSelect.SetActive(false);
                            if (_btnMythic02)
                                _btnMythic02.m_objSelect.SetActive(true);
                            _selectedUnit = CAConstants.SELECT_MYTHIC_02;
                            _selectedType = CAUnitType.Type2;
                            _txtUnitName.SetText("Mythic02");
                            SetCombinationData();
                        }
                    }
                    break;
            }
        }
        private void ResetData()
        {
            _listOwnedUnit = new List<(CAUnitGrade, CAUnitType)>();
            if (_txtIsOwnedUnit_Normal)
                _txtIsOwnedUnit_Normal.SetText("Not Owned");
            if (_txtIsOwnedUnit_Rare)
                _txtIsOwnedUnit_Rare.SetText("Not Owned");
            if (_txtIsOwnedUnit_Hero)
                _txtIsOwnedUnit_Hero.SetText("Not Owned");
        }
        private void SetCombinationData()
        {
            if (_btnCombine == null)
                return;
            if (_imgUnitNormal == null || _imgUnitRare == null || _imgUnitHero == null)
                return;
            if (_txtIsOwnedUnit_Normal == null || _txtIsOwnedUnit_Rare == null || _txtIsOwnedUnit_Hero == null)
                return;
            if (_txtProgress_TargetMythic == null || _txtProgress_Mythic01 == null || _txtProgress_Mythic02 == null)
                return;

            ResetData();

            _listOwnedUnit = CABattleManager.Instance.GetOwnedUnitsForMythicCombination(_selectedUnit);
            if (_listOwnedUnit != null && _listOwnedUnit.Count == 3)
                _btnCombine.gameObject.SetActive(true);
            else
                _btnCombine.gameObject.SetActive(false);

            foreach (var data in _listOwnedUnit)
            {
                if (data.Item1 == CAUnitGrade.Normal)
                    _txtIsOwnedUnit_Normal.SetText("Owned");
                else if (data.Item1 == CAUnitGrade.Rare)
                    _txtIsOwnedUnit_Rare.SetText("Owned");
                else if (data.Item1 == CAUnitGrade.Hero)
                    _txtIsOwnedUnit_Hero.SetText("Owned");
            }

            switch (_selectedUnit)
            {
                case CAConstants.SELECT_MYTHIC_01:
                    {
                        _imgUnitNormal.sprite = _listSpriteResource_Normal[0];
                        _imgUnitRare.sprite = _listSpriteResource_Rare[0];
                        _imgUnitHero.sprite = _listSpriteResource_Hero[0];
                        _imgTargetMythic.color = Color.white;

                        if(_txtProgress_Mythic01)
                            _txtProgress_TargetMythic.SetText(_txtProgress_Mythic01.text);
                    }
                    break;
                case CAConstants.SELECT_MYTHIC_02:
                    {
                        _imgUnitNormal.sprite = _listSpriteResource_Normal[1];
                        _imgUnitRare.sprite = _listSpriteResource_Rare[1];
                        _imgUnitHero.sprite = _listSpriteResource_Hero[1];
                        if (ColorUtility.TryParseHtmlString("#FF4959", out Color targetColor))
                            _imgTargetMythic.color = targetColor;

                        if (_txtProgress_Mythic02)
                            _txtProgress_TargetMythic.SetText(_txtProgress_Mythic02.text);
                    }
                    break;
            }
        }
    }
}