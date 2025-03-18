using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CAEnemyController : MonoBehaviour
{
    private Transform[] _wayPoints;
    private int _wayPointCount;
    private int _curIndex;
    private Vector2 _direction;
    private int _maxHP;
    private int _curHP;
    private int _myLevel;
    private Coroutine _moveCo;
    private Coroutine _attackedDisplayCo;

    public bool IsBoss { get; private set; }
    public int MyWave { get; private set; }
    public Slider _sliderHP;
    public Image _fillImage;
    public Canvas _canvas;
    public Animator _animator;
    public Animator _animatorAttacked;
    public SpriteRenderer _spriteRenderer;
    public void Initialize(Transform[] wayPoints, int curWave)
    {
        if (wayPoints == null || wayPoints.Length == 0)
            return;

        if (_animatorAttacked)
            _animatorAttacked.gameObject.SetActive(false);
        _curIndex = 0;
        _wayPointCount = wayPoints.Length;
        _wayPoints = new Transform[_wayPointCount];
        _wayPoints = wayPoints;
        MyWave = curWave;
        _myLevel = curWave/10 + 1;
        IsBoss = false;
        if (curWave % 10 == 0)
        {
            _maxHP = _myLevel * CAConstants.BOSS_LEVEL_HP;
            IsBoss = true;
        }
        else
        {
            _maxHP = _myLevel * CAConstants.ENEMY_LEVEL_HP + (curWave - 1) * CAConstants.ENEMY_WAVE_HP;
        }
        _curHP = _maxHP;

        if(_sliderHP)
            _sliderHP.value = 1;
        if(_fillImage)
            _fillImage.color = Color.green;
        if(_canvas)
            _canvas.worldCamera = Camera.main;

        transform.position = wayPoints[_curIndex].position;

        if(_moveCo != null)
        {
            StopCoroutine(_moveCo);
            _moveCo = null;
        }
        _moveCo = StartCoroutine(MoveCo());
    }

    private IEnumerator MoveCo()
    {
        float moveSpeed = 0;
        if (IsBoss)
            moveSpeed = CAConstants.BOSS_MOVE_SPEED;
        else
            moveSpeed = CAConstants.ENEMY_MOVE_SPEED;
        while (true)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                _wayPoints[_curIndex].position,
                moveSpeed * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, _wayPoints[_curIndex].position) < 0.02f)
                SetTarget();
            
            if (_wayPoints[_curIndex].position.y > transform.position.y || _wayPoints[_curIndex].position.x < transform.position.x)
            {
                _spriteRenderer.flipX = true;
            }
            else
            {
                _spriteRenderer.flipX = false;
            }

            yield return null;
        }
    }
    private void SetTarget()
    {
        if(_curIndex < _wayPointCount - 1)
        {
            transform.position = _wayPoints[_curIndex].position;
            ++_curIndex;
        }
        else
        {
            _curIndex = 0;
        }
    }

    public int OnAttacked(int damage)
    {
        if (_curHP == 0)
            return 0;

        _curHP -= damage;
        if (_curHP < 0)
            _curHP = 0;
        if(_attackedDisplayCo != null)
        {
            StopCoroutine(_attackedDisplayCo);
            _attackedDisplayCo = null;
        }
        _attackedDisplayCo = StartCoroutine(AttackedDisplay());
        float hpRate = (float)_curHP / _maxHP;
        if (_fillImage)
        {
            if (hpRate < CAConstants.ENEMY_HP_COLOR_RED)
                _fillImage.color = Color.red;
            else if (hpRate < CAConstants.ENEMY_HP_COLOR_YELLOW)
                _fillImage.color = Color.yellow;
        }
        if (_sliderHP)
            _sliderHP.value = hpRate;

        CABattleManager.Instance.SetDamageUI(this.transform, damage);

        if (_curHP == 0)
            OnDead();

        return _curHP;
    }
    IEnumerator AttackedDisplay()
    {
        if (_animatorAttacked == null)
            yield break;

        _animatorAttacked.gameObject.SetActive(true);
        _animatorAttacked.Play("Attacked");
        yield return new WaitForSeconds(0.3f);

        _animatorAttacked.gameObject.SetActive(false);
    }
    private void OnDead()
    {
        if (_moveCo != null)
        {
            StopCoroutine(_moveCo);
            _moveCo = null;
        }
        if (_animator)
            _animator.Play("Dead");
        CABattleManager.Instance.ReturnEnemy(this);
    }
}
