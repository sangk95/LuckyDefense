using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CABattleManager;
using static CAUnitSpawner;

public class CAUnitController : MonoBehaviour
{
    public Animator _animator;
    public SpriteRenderer _renderer;
    public CircleCollider2D _collider;
    public Coroutine _attackCo;

    private CAUnit _unitData;
    private LinkedList<CAEnemyController> _listTargetEnemy;
    private WaitForSeconds _attackDelay;
    private int _attackPower;
    private Action<CAEnemyController> _callBack;

    private bool _isInitialized = false;
    public void Initialize(ref Action<CAEnemyController> enemyDead)
    {
        if (_isInitialized)
            return;

        _listTargetEnemy = new LinkedList<CAEnemyController>();
        _callBack = enemyDead;
        if(enemyDead != null)
            enemyDead += EnemyDead;

        float attackSpeed = 0f;
        switch (_unitData.UnitGrade)
        {
            case CAUnitGrade.Normal:
                {
                    _attackPower = CAConstants.ATTACK_POWER_NORMAL;

                    if (_unitData.UnitType == CAUnitType.Type1)
                        attackSpeed = CAConstants.ATTACK_SPEED_TYPE_1;
                    else if (_unitData.UnitType == CAUnitType.Type2)
                        attackSpeed = CAConstants.ATTACK_SPEED_TYPE_2;
                }
                break;
            case CAUnitGrade.Rare:
                {
                    _attackPower = CAConstants.ATTACK_POWER_RARE;

                    if (_unitData.UnitType == CAUnitType.Type1)
                        attackSpeed = CAConstants.ATTACK_SPEED_TYPE_1;
                    else if (_unitData.UnitType == CAUnitType.Type2)
                        attackSpeed = CAConstants.ATTACK_SPEED_TYPE_2;
                }
                break;
            case CAUnitGrade.Hero:
                {
                    _attackPower = CAConstants.ATTACK_POWER_HERO;

                    if (_unitData.UnitType == CAUnitType.Type1)
                        attackSpeed = CAConstants.ATTACK_SPEED_HERO_1;
                    else if (_unitData.UnitType == CAUnitType.Type2)
                        attackSpeed = CAConstants.ATTACK_SPEED_HERO_2;
                }
                break;
            case CAUnitGrade.Mythic:
                {
                    _attackPower = CAConstants.ATTACK_POWER_MYTHIC;

                    if (_unitData.UnitType == CAUnitType.Type1)
                        attackSpeed = CAConstants.ATTACK_SPEED_MYTHIC_1;
                    else if (_unitData.UnitType == CAUnitType.Type2)
                        attackSpeed = CAConstants.ATTACK_SPEED_MYTHIC_2;
                }
                break;
            default: _attackPower = 0; break;
        }

        _attackDelay = new WaitForSeconds(attackSpeed);
        _attackCo = StartCoroutine(AttackCo());
        _isInitialized = true;
    }
    private void OnDestroy()
    {
        if(_callBack != null)
            _callBack -= EnemyDead;
        if (_attackCo != null)
        {
            StopCoroutine(_attackCo);
            _attackCo = null;
        }
    }
    public void SetColliderOffset(Transform tileTransform)
    {
        if (tileTransform != null && _collider != null)
        {
            Vector2 offsetWorld = tileTransform.position - transform.position;

            offsetWorld.x /= transform.lossyScale.x;
            offsetWorld.y /= transform.lossyScale.y;

            _collider.offset = offsetWorld;
        }
    }
    public void SetUnitData(CAUnitGrade grade, CAUnitType type)
    {
        CAUnit unitData = new CAUnit(grade, type);
        _unitData = unitData;
    }
    public CAUnit GetUnitData()
    {
        return _unitData;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
            return;

        var enemyController = collision.GetComponent<CAEnemyController>();
        if (enemyController)
            _listTargetEnemy.AddLast(enemyController);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
            return;

        var enemyController = collision.GetComponent<CAEnemyController>();
        if (enemyController)
            _listTargetEnemy.Remove(enemyController);
    }
    public IEnumerator AttackCo()
    {
        _listTargetEnemy = new LinkedList<CAEnemyController>();
        while (true)
        {
            if (_listTargetEnemy != null && _listTargetEnemy.Count > 0)
            {
                AttackTarget();
                yield return _attackDelay;
            }
            yield return null;
        }
    }
    private void AttackTarget()
    {
        _animator.Play("Attack");

        CAEnemyController enemy = _listTargetEnemy.First.Value;
        if (_renderer)
        {
            if (enemy.transform.position.x < transform.position.x)
                _renderer.flipX = true;
            else
                _renderer.flipX = false;
        }

        enemy.OnAttacked(_attackPower);
    }
    public void EnemyDead(CAEnemyController enemy)
    {
        if (_listTargetEnemy != null && enemy && _listTargetEnemy.Contains(enemy))
            _listTargetEnemy.Remove(enemy);
    }
}
