using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CA.UI
{
    public class CAPageBattle : CAUIController
    {
        private const string _strClick_Spawn = "Click Spawn";
        private const string _strClick_Mythic = "Click Mythic";
        private const string _strClick_Gamble = "Click Gamble";
        private const string _strClick_GambleClose = "Click Gamble Close";
        private const string _strClick_GambleRare = "Click Gamble Rare";
        private const string _strClick_GambleHero = "Click Gamble Hero";
        private const string _strClick_Upgrade = "Click Upgrade";
        private const string _strClick_Sell = "Click Sell";

        public List<Sprite> _listUnitSpriteRare = new List<Sprite>();
        public List<Sprite> _listUnitSpriteHero = new List<Sprite>();

        public Image _imgArrow;

        [Header("Gamble")]
        public GameObject _objGamble;
        public GameObject _objGamble_Rare;
        public DOTweenAnimation _tweenGamble_Rare;
        public DOTweenAnimation _tweenGamble_RareFailed;
        public GameObject _objGamble_RareFailed;
        public Image _imgGamble_RareSuccess;
        public GameObject _objGamble_Hero;
        public DOTweenAnimation _tweenGamble_Hero;
        public DOTweenAnimation _tweenGamble_HeroFailed;
        public GameObject _objGamble_HeroFailed;
        public Image _imgGamble_HeroSuccess;

        [Header("GameObject")]
        public GameObject _objIntro;
        public List<GameObject> _listObjectInGame = new List<GameObject>();
        public List<GameObject> _listObjectResult = new List<GameObject>();

        public GameObject _objUnitControl;
        public GameObject _objSkullNormal;
        public GameObject _objSkullDanger;

        public GameObject _objDangerAlert;
        public GameObject _objBossAlert;
        public GameObject _objBossTimer;
        public GameObject _objBossKillAlert;
        public GameObject _objMythicAlert;
        public GameObject _objMythic01;
        public GameObject _objMythic02;

        public GameObject _objSpawnTimer_Mine;
        public GameObject _objSpawnTimer_AI;

        [Header("Text")]
        public TextMeshProUGUI _txtBattleTimer;
        public TextMeshProUGUI _txtBossTimer;
        public TextMeshProUGUI _txtCurrentWave;
        public TextMeshProUGUI _txtCurrentWave_BossAlert;
        public TextMeshProUGUI _txtEnemyCount;
        public TextMeshProUGUI _txtEnemyCount_DangerAlert;
        public TextMeshProUGUI _txtSpawnCost;
        public TextMeshProUGUI _txtMyGold;
        public TextMeshProUGUI _txtMyDiamond;
        public TextMeshProUGUI _txtMyDiamond_Gamble;
        public TextMeshProUGUI _txtMyUnitCount;
        public TextMeshProUGUI _txtMyUnitCount_Gamble;
        public TextMeshProUGUI _txtSpawnTimer_Mine;
        public TextMeshProUGUI _txtSpawnTimer_AI;

        [Header("Slider")]
        public Slider _sliderEnemyCount;

        [Header("Button")]
        public CAButton _btnSpawn;
        public CAButton _btnMythic;
        public CAButton _btnGamble;
        public CAButton _btnUpgrade;
        public CAButton _btnSell;

        public CAButton _btnGamble_Close;
        public CAButton _btnGamble_Rare;
        public CAButton _btnGamble_Hero;

        private Coroutine _goldAnimCo;
        private Coroutine _diamondAnimCo;
        private WaitForSeconds _alertDisplayTime;
        private WaitForSeconds _gambleDisplayTime;
        private WaitForSeconds _gambleResultDisplayTime;
        private bool _isGambling;
        public override void UIInit()
        {
            base.UIInit();
            if (_btnSpawn != null)
            {
                _btnSpawn.AddUICallBack(CAUIState.Click, _strClick_Spawn.GetHashCode());
                _btnSpawn.CallBack += UICallBack;
            }
            if (_btnMythic != null)
            {
                _btnMythic.AddUICallBack(CAUIState.Click, _strClick_Mythic.GetHashCode());
                _btnMythic.CallBack += UICallBack;
            }
            if (_btnGamble != null)
            {
                _btnGamble.AddUICallBack(CAUIState.Click, _strClick_Gamble.GetHashCode());
                _btnGamble.CallBack += UICallBack;
            }
            if (_btnGamble_Close != null)
            {
                _btnGamble_Close.AddUICallBack(CAUIState.Click, _strClick_GambleClose.GetHashCode());
                _btnGamble_Close.CallBack += UICallBack;
            }
            if (_btnGamble_Rare != null)
            {
                _btnGamble_Rare.AddUICallBack(CAUIState.Click, _strClick_GambleRare.GetHashCode());
                _btnGamble_Rare.CallBack += UICallBack;
            }
            if (_btnGamble_Hero != null)
            {
                _btnGamble_Hero.AddUICallBack(CAUIState.Click, _strClick_GambleHero.GetHashCode());
                _btnGamble_Hero.CallBack += UICallBack;
            }
            if (_btnUpgrade != null)
            {
                _btnUpgrade.SetText("Upgrade");
                _btnUpgrade.AddUICallBack(CAUIState.Click, _strClick_Upgrade.GetHashCode());
                _btnUpgrade.CallBack += UICallBack;
            }
            if (_btnSell != null)
            {
                _btnSell.SetText("Sell");
                _btnSell.AddUICallBack(CAUIState.Click, _strClick_Sell.GetHashCode());
                _btnSell.CallBack += UICallBack;
            }
        }
        public override void UIEnter()
        {
            base.UIEnter();

            if (_imgArrow)
                _imgArrow.gameObject.SetActive(false);

            if (_txtBattleTimer)
                _txtBattleTimer.SetText("00:" + CAConstants.TIME_PER_ROUND.ToString());
            if (_txtBossTimer)
                _txtBossTimer.SetText("00:" + CAConstants.TIME_PER_ROUND_BOSS.ToString());
            if (_txtEnemyCount)
                _txtEnemyCount.SetText("0 / " + CAConstants.ENEMY_MAX_COUNT.ToString());
            if (_txtEnemyCount_DangerAlert)
                _txtEnemyCount_DangerAlert.SetText("0 / " + CAConstants.ENEMY_MAX_COUNT.ToString());
            if (_txtMyUnitCount)
                _txtMyUnitCount.SetText("0 / " + CAConstants.UNIT_MAX_COUNT.ToString());
            if (_txtMyUnitCount_Gamble)
                _txtMyUnitCount_Gamble.SetText("0 / " + CAConstants.UNIT_MAX_COUNT.ToString());
            if (_txtSpawnCost)
                _txtSpawnCost.SetText(CAConstants.START_SPAWN_COST.ToString());
            if (_txtMyGold)
                _txtMyGold.SetText(CAConstants.START_GOLD.ToString());
            if (_txtMyDiamond)
                _txtMyDiamond.SetText("0");
            if (_txtMyDiamond_Gamble)
                _txtMyDiamond_Gamble.SetText("0");

            if (_objIntro)
                _objIntro.SetActive(true);
            if (_listObjectInGame != null && _listObjectInGame.Count > 0)
            {
                foreach(var obj in _listObjectInGame)
                {
                    obj.SetActive(false);
                }
            }
            if (_listObjectResult != null && _listObjectResult.Count > 0)
            {
                foreach(var obj in _listObjectResult)
                {
                    obj.SetActive(false);
                }
            }

            if (_objGamble)
                _objGamble.SetActive(false);
            if (_objGamble_Rare)
                _objGamble_Rare.SetActive(false);
            if (_objGamble_RareFailed)
                _objGamble_RareFailed.SetActive(false);
            if (_imgGamble_RareSuccess)
                _imgGamble_RareSuccess.gameObject.SetActive(false);
            if (_objGamble_Hero)
                _objGamble_Hero.SetActive(false);
            if (_objGamble_HeroFailed)
                _objGamble_HeroFailed.SetActive(false);
            if (_imgGamble_HeroSuccess)
                _imgGamble_HeroSuccess.gameObject.SetActive(false);

            if (_objUnitControl)
                _objUnitControl.SetActive(false);
            if (_objSkullNormal)
                _objSkullNormal.SetActive(true);
            if (_objSkullDanger)
                _objSkullDanger.SetActive(false);

            if (_objBossAlert)
                _objBossAlert.SetActive(false);
            if (_objDangerAlert)
                _objDangerAlert.SetActive(false);
            if (_objBossTimer)
                _objBossTimer.SetActive(false);
            if (_objBossKillAlert)
                _objBossKillAlert.SetActive(false);
            if (_objMythicAlert)
                _objMythicAlert.SetActive(false);
            if (_objMythic01)
                _objMythic01.SetActive(false);
            if (_objMythic02)
                _objMythic02.SetActive(false);

            if (_objSpawnTimer_Mine)
                _objSpawnTimer_Mine.SetActive(false);
            if (_objSpawnTimer_AI)
                _objSpawnTimer_AI.SetActive(false);

            if (_sliderEnemyCount)
                _sliderEnemyCount.value = 0;

            CABattleManager.Instance.ChangedEnemyCount += ChangeEnemyCount;
            CABattleManager.Instance.ChangedCurWave += ChangeCurWave;
            CABattleManager.Instance.ChangedSpawnCost += ChangeSpawnCost;
            CABattleManager.Instance.ChangedMyMoney += ChangeMyMoney;
            CABattleManager.Instance.ChangedUnitCount += ChangeUnitCount;

            _alertDisplayTime = new WaitForSeconds(CAConstants.ALERT_DISPLAY_TIME);
            _gambleDisplayTime = new WaitForSeconds(CAConstants.GAMBLE_DISPLAY_TIME);
            _gambleResultDisplayTime = new WaitForSeconds(CAConstants.GAMBLE_RESULT_DISPLAY_TIME);
            _isGambling = false;

            StartCoroutine(IntroCo());
        }
        IEnumerator IntroCo()
        {
            yield return new WaitForSeconds(CAConstants.INTRO_DISPLAY_TIME);
            if (_objIntro)
                _objIntro.SetActive(false);
            if (_listObjectInGame != null && _listObjectInGame.Count > 0)
            {
                foreach (var obj in _listObjectInGame)
                {
                    obj.SetActive(true);
                }
            }
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
                        if (hashCode == _strClick_Spawn.GetHashCode())
                        {
                            CABattleManager.Instance.SpawnUnit();
                        }
                        else if (hashCode == _strClick_Mythic.GetHashCode())
                        {
                            CAUIManager.Instance.LoadUIController(CAUIAddress.CAPage_Mythic);
                        }
                        else if (hashCode == _strClick_Gamble.GetHashCode())
                        {
                            if (_objGamble)
                            {
                                _objGamble.SetActive(true);
                                _objGamble.GetComponent<DOTweenAnimation>()?.DORestart();
                            }
                        }
                        else if (hashCode == _strClick_GambleClose.GetHashCode())
                        {
                            if (_objGamble)
                                _objGamble.SetActive(false);
                        }
                        else if (hashCode == _strClick_GambleRare.GetHashCode() && !_isGambling)
                        {
                            if (CABattleManager.Instance.MyDiamond < CAConstants.GAMBLE_COST_RARE)
                                return;
                            bool success = CABattleManager.Instance.OnGambling(CAUnitGrade.Rare);
                            if (_objGamble_Rare && _objGamble_RareFailed && _imgGamble_RareSuccess && _tweenGamble_Rare && _tweenGamble_RareFailed)
                                StartCoroutine(GambleCo(_objGamble_Rare, _objGamble_RareFailed, _imgGamble_RareSuccess, _tweenGamble_Rare, _tweenGamble_RareFailed, success));
                        }
                        else if (hashCode == _strClick_GambleHero.GetHashCode() && !_isGambling)
                        {
                            if (CABattleManager.Instance.MyDiamond < CAConstants.GAMBLE_COST_HERO)
                                return;
                            bool success = CABattleManager.Instance.OnGambling(CAUnitGrade.Hero);
                            if (_objGamble_Hero && _objGamble_HeroFailed && _imgGamble_HeroSuccess && _tweenGamble_Hero && _tweenGamble_HeroFailed)
                                StartCoroutine(GambleCo(_objGamble_Hero, _objGamble_HeroFailed, _imgGamble_HeroSuccess, _tweenGamble_Hero, _tweenGamble_HeroFailed, success));
                        }
                        else if (hashCode == _strClick_Upgrade.GetHashCode())
                        {
                            CABattleManager.Instance.UpgradeUnit();
                            CloseUnitControl();
                        }
                        else if (hashCode == _strClick_Sell.GetHashCode())
                        {
                            CABattleManager.Instance.SellUnit();
                            CloseUnitControl();
                        }
                    }
                    break;
            }
        }
        private IEnumerator GambleCo(GameObject gambleObj, GameObject failedObj, Image successObj, DOTweenAnimation tweenAnim, DOTweenAnimation tweenAnimFailed, bool success)
        {
            _isGambling = true;

            gambleObj.SetActive(true);
            tweenAnim.DORestart();
            yield return _gambleDisplayTime;
            gambleObj.SetActive(false);
            if (!success)
            {
                failedObj.SetActive(true);
                tweenAnimFailed.DORestart();
            }
            else
            {
                successObj.gameObject.SetActive(true);
                if (CABattleManager.Instance.GambleData.Item1 == CAUnitGrade.Rare)
                {
                    successObj.sprite = _listUnitSpriteRare[(int)CABattleManager.Instance.GambleData.Item2];
                }
                else if (CABattleManager.Instance.GambleData.Item1 == CAUnitGrade.Hero)
                {
                    successObj.sprite = _listUnitSpriteHero[(int)CABattleManager.Instance.GambleData.Item2];
                }
            }

            yield return _gambleResultDisplayTime;
            failedObj.SetActive(false);
            successObj.gameObject.SetActive(false);

            CABattleManager.Instance.SpawnGambleUnit();
            _isGambling = false;
        }
        private void Update()
        {
            if (_txtBattleTimer)
                _txtBattleTimer.SetText("00:" + CABattleManager.Instance.RemainTime.ToString("00"));
            if (_txtSpawnTimer_Mine)
                _txtSpawnTimer_Mine.SetText(CABattleManager.Instance.RemainTime.ToString("0"));
            if (_txtSpawnTimer_AI)
                _txtSpawnTimer_AI.SetText(CABattleManager.Instance.RemainTime.ToString("0"));
            if(_txtBossTimer)
                _txtBossTimer.SetText("00:" + CABattleManager.Instance.RemainTime.ToString("00"));
        }
        public void SetNextWaveAlert()
        {
            if (_objSpawnTimer_Mine == null || _objSpawnTimer_AI == null)
                return;
            _objSpawnTimer_Mine.SetActive(true);
            _objSpawnTimer_Mine.GetComponent<DOTweenAnimation>()?.DORestart();
            _objSpawnTimer_AI.SetActive(true);
            _objSpawnTimer_AI.GetComponent<DOTweenAnimation>()?.DORestart();
            StartCoroutine(NextWaveAlertCo(CABattleManager.Instance.RemainTime));
        }
        IEnumerator NextWaveAlertCo(float remainTime)
        {
            while(remainTime > 0)
            {
                remainTime -= Time.deltaTime;
                yield return null;
            }
            _objSpawnTimer_Mine.SetActive(false);
            _objSpawnTimer_AI.SetActive(false);
        }
        public void SetMythicAlert(CAUnitType unitType)
        {
            if (_objMythicAlert == null || _objMythic01 == null || _objMythic02 == null)
                return;
            _objMythicAlert.SetActive(true);
            _objMythicAlert.GetComponent<DOTweenAnimation>()?.DORestart();
            _objMythic01.SetActive(false);
            _objMythic02.SetActive(false);
            if (unitType == CAUnitType.Type1)
                _objMythic01.SetActive(true);
            else if (unitType == CAUnitType.Type2)
                _objMythic02.SetActive(true);

            StartCoroutine(CloseAlert(_objMythicAlert));
        }
        public void HideArrowUI()
        {
            _imgArrow.gameObject.SetActive(false);
        }
        public void UpdateArrowUI(Vector2 startPos, Vector2 currentPos)
        {
            Vector2 direction = currentPos - startPos;
            float distance = direction.magnitude;

            if (distance < 0.1f)
            {
                _imgArrow.gameObject.SetActive(false);
                return;
            }

            float angle = Vector2.SignedAngle(Vector2.up, direction);
            _imgArrow.rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            _imgArrow.rectTransform.position = startPos;

            Vector2 newSize = _imgArrow.rectTransform.sizeDelta;
            newSize.y = distance * 0.9f;
            _imgArrow.rectTransform.sizeDelta = newSize;

            if (!_imgArrow.gameObject.activeSelf)
                _imgArrow.gameObject.SetActive(true);
        }

        public void SetBattleResult(CABattleResult result)
        {
            if (_listObjectResult == null || _listObjectResult.Count <= (int)result)
                return;

            _listObjectResult[(int)result].SetActive(true);
            _listObjectResult[(int)result].GetComponent<DOTweenAnimation>()?.DORestart();

            StartCoroutine(BattleEndCo(result));
        }
        private IEnumerator BattleEndCo(CABattleResult result)
        {
            yield return new WaitForSeconds(CAConstants.AUTO_EXIT_DURATION);

            CAUIManager.Instance.CloseUIController(this);
            CAStateManager.Instance.ChangeState(CAGameState.State_Lobby);
        }
        public void ShowUnitControl(Transform targetTile)
        {
            if(_objUnitControl)
            {
                _objUnitControl.SetActive(true);
                Vector3 screenPos = Camera.main.WorldToScreenPoint(targetTile.position);
                _objUnitControl.transform.position = screenPos;
            }
        }
        public void CloseUnitControl()
        {
            if (_objUnitControl)
                _objUnitControl.SetActive(false);
        }
        public void ChangeEnemyCount()
        {
            if (_txtEnemyCount)
                _txtEnemyCount.SetText(CABattleManager.Instance.EnemyCount.ToString() + " / " + CAConstants.ENEMY_MAX_COUNT.ToString());
            if (_txtEnemyCount_DangerAlert)
                _txtEnemyCount_DangerAlert.SetText(CABattleManager.Instance.EnemyCount.ToString() + " / " + CAConstants.ENEMY_MAX_COUNT.ToString());
            if (_sliderEnemyCount)
                _sliderEnemyCount.value = (float)CABattleManager.Instance.EnemyCount / CAConstants.ENEMY_MAX_COUNT;

            if (_objSkullNormal != null && _objSkullDanger != null && _objDangerAlert != null)
            {
                if (CABattleManager.Instance.EnemyCount == CAConstants.ENEMY_DANGER_COUNT &&
                    CABattleManager.Instance.EnemyCount > CABattleManager.Instance.PrevEnemyCount)
                {
                    _objSkullNormal.SetActive(false);
                    _objSkullDanger.SetActive(true);
                    _objDangerAlert.SetActive(true);
                    StartCoroutine(CloseAlert(_objDangerAlert));
                }
                else if(CABattleManager.Instance.EnemyCount < CAConstants.ENEMY_DANGER_COUNT)
                {
                    _objSkullNormal.SetActive(true);
                    _objSkullDanger.SetActive(false);
                }
            }
        }
        public void ChangeCurWave()
        {
            if (_txtCurrentWave)
                _txtCurrentWave.SetText(CABattleManager.Instance.CurWave.ToString());
            if (_txtCurrentWave_BossAlert)
                _txtCurrentWave_BossAlert.SetText(CABattleManager.Instance.CurWave.ToString());
            if(CABattleManager.Instance.CurWave % 10 == 0)
            {
                if (_objBossAlert)
                {
                    _objBossAlert.SetActive(true);
                    StartCoroutine(CloseAlert(_objBossAlert));
                }
                if (_objBossTimer)
                    _objBossTimer.SetActive(true);
            }
            else
            {
                if (_objBossTimer)
                    _objBossTimer.SetActive(false);
            }
        }
        public void KilledBoss()
        {
            if (_objBossTimer)
                _objBossTimer.SetActive(false);
            if (_objBossKillAlert)
            {
                _objBossKillAlert.SetActive(true);
                _objBossKillAlert.GetComponent<DOTweenAnimation>().DORestart();
                StartCoroutine(CloseAlert(_objBossKillAlert));
            }
        }
        private IEnumerator CloseAlert(GameObject alert)
        {
            yield return _alertDisplayTime;

            alert.SetActive(false);
        }
        public void ChangeSpawnCost()
        {
            if (_txtSpawnCost)
                _txtSpawnCost.SetText(CABattleManager.Instance.CurSpawnCost.ToString());
            if (CABattleManager.Instance.CurSpawnCost > CABattleManager.Instance.MyGold)
                _txtSpawnCost.color = Color.red;
            else
                _txtSpawnCost.color = Color.white;
        }
        public void ChangeMyMoney()
        {
            int curGold = CABattleManager.Instance.MyGold;
            int prevGold = CABattleManager.Instance.PrevGold;
            int curDiamond = CABattleManager.Instance.MyDiamond;
            int prevDiamond = CABattleManager.Instance.PrevDiamond;

            if (prevGold != curGold)
            {
                if (_goldAnimCo != null)
                {
                    StopCoroutine(_goldAnimCo);
                    _goldAnimCo = null;
                }
                _goldAnimCo = StartCoroutine(AnimateGoldText(prevGold, curGold));
            }
            if (prevDiamond != curDiamond)
            {
                if (_diamondAnimCo != null)
                {
                    StopCoroutine(_diamondAnimCo);
                    _diamondAnimCo = null;
                }
                _diamondAnimCo = StartCoroutine(AnimateDiamondText(prevDiamond, curDiamond));
            }

            if (_txtMyDiamond_Gamble)
                _txtMyDiamond_Gamble.SetText(CABattleManager.Instance.MyDiamond.ToString());
            if (CABattleManager.Instance.CurSpawnCost > CABattleManager.Instance.MyGold)
                _txtSpawnCost.color = Color.red;
            else
                _txtSpawnCost.color = Color.white;
        }
        public void ChangeUnitCount()
        {
            if (_txtMyUnitCount)
                _txtMyUnitCount.SetText(CABattleManager.Instance.MyUnitCount.ToString() + " / " + CAConstants.UNIT_MAX_COUNT.ToString());
            if (_txtMyUnitCount_Gamble)
                _txtMyUnitCount_Gamble.SetText(CABattleManager.Instance.MyUnitCount.ToString() + " / " + CAConstants.UNIT_MAX_COUNT.ToString());
        }
        private IEnumerator AnimateGoldText(int startGold, int endGold)
        {
            if (_txtMyGold == null)
                yield break;
            if (startGold > endGold)
                _txtMyGold.color = Color.red;
            else
            {
                if (_objGamble.activeSelf == false && endGold - startGold != CAConstants.INCREASE_ROUND_GOLD)
                {
                    CAUIManager.Instance.LoadUIController(CAUIAddress.CAUIAddGold, (CAUIController uiController) =>
                    {
                        if (uiController != null && uiController is CAUIAddGold uiAddGold)
                        {
                            uiAddGold.SetAmount(_txtMyGold.rectTransform, endGold - startGold);
                        }
                    });
                }
            }

            float elapsed = 0f;
            while (elapsed < CAConstants.MONEY_ANIM_DURATION)
            {
                float t = elapsed / CAConstants.MONEY_ANIM_DURATION;
                int displayGold = Mathf.RoundToInt(Mathf.Lerp(startGold, endGold, t));
                _txtMyGold.SetText(displayGold.ToString());
                elapsed += Time.deltaTime;
                yield return null;
            }
            _txtMyGold.color = Color.white;
            _txtMyGold.SetText(endGold.ToString());
        }
        private IEnumerator AnimateDiamondText(int startDiamond, int endDiamond)
        {
            if (_txtMyDiamond == null)
                yield break;
            if (startDiamond > endDiamond)
                _txtMyDiamond.color = Color.red;
            else
            {
                if (_objGamble.activeSelf == false)
                {
                    CAUIManager.Instance.LoadUIController(CAUIAddress.CAUIAddDiamond, (CAUIController uiController) =>
                    {
                        if (uiController != null && uiController is CAUIAddDiamond uiAddDiamond)
                        {
                            uiAddDiamond.SetAmount(_txtMyDiamond.rectTransform, endDiamond - startDiamond);
                        }
                    });
                }
            }

            float elapsed = 0f;
            while (elapsed < CAConstants.MONEY_ANIM_DURATION)
            {
                float t = elapsed / CAConstants.MONEY_ANIM_DURATION;
                int displayDiamond = Mathf.RoundToInt(Mathf.Lerp(startDiamond, endDiamond, t));
                _txtMyDiamond.SetText(displayDiamond.ToString());
                elapsed += Time.deltaTime;
                yield return null;
            }
            _txtMyDiamond.color = Color.white;
            _txtMyDiamond.SetText(endDiamond.ToString());
        }
    }
}