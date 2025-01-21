using System.Collections;
using BananaUtils.OnScreenDebugger.Scripts;
using KatanaMovement;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    private enum State
    {
        Idle,
        Chase,
        Attack,
        Flee
    }

    private enum EnemyType
    {
        Ground,
        Flying
    }

    [Header("Enemy Stats")]
    [SerializeField] private EnemyType _enemyType;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private float _minDamage;
    [SerializeField] private float _maxDamage;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _spotDistance;
    [SerializeField] private float _attackDistance;
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _fleeHealth;

    [Header("References")]
    [SerializeField] private Transform _target;

    private NavMeshAgent _agent;
    private State _state;
    private bool _canAttack;
    private float _health;

    private Vector3 _targetPosition;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _canAttack = true;
        _agent.speed = _moveSpeed;
        _health = _maxHealth;

        if (_enemyType == EnemyType.Flying)
        {
            Destroy(_agent);
        }
    }

    private void Update()
    {   

        //If no target found be idle
        if (_target == null)
        {
            _state = State.Idle;
        }

        //State Machine
        else
        {
            switch (_state)
            {
                case State.Idle:
                    Idle();
                    break;

                case State.Chase:
                    Chase();
                    break;

                case State.Attack:
                    Attack();
                    break;

                case State.Flee:
                    Flee();
                    break;
            }

            float distance = Vector3.Distance(transform.position, _target.transform.position);
            if (distance > _spotDistance)
            {
                _state = State.Idle;
            }

            else
            {
                _state = State.Chase;
            }

            if (_health <= _fleeHealth)
            {
                _state = State.Flee;
            }
        }

        Debug.Log(_target);
    }

    /* //Find target function finds targets this would be useful if the game was networked didn't use it here.
    //As the collider is a child and I don't really think there is a point looking in children for colliders better to just ref the target.
    private void FindTarget()
    {
        Collider[] colliders;
        colliders = Physics.OverlapSphere(transform.position, _spotDistance);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<PlayerMovementScript>(out PlayerMovementScript playerMovementScript))
            {
                _target = playerMovementScript.transform;
            }
        }
    }
    */

    //Idle state
    private void Idle()
    {
        switch (_enemyType)
        {
            case EnemyType.Ground:
                _agent.SetDestination(transform.position);
                break;
            case EnemyType.Flying:
                MoveToPosition(transform.position);
                break;
        }
    }

    //Chase state
    private void Chase()
    {
        switch (_enemyType)
        {
            case EnemyType.Ground:
                _agent.SetDestination(_target.position);

                if (Vector3.Distance(transform.position, _target.position) < _attackDistance)
                {
                    _state = State.Attack;
                }
                break;

            case EnemyType.Flying:
                MoveToPosition(_target.position);
                break;
        }
    }

    //Attack state
    private void Attack()
    {   //Raycast based attacking Physics.OverlapSphere can also work well here but the collider issue and it's present with raycasts too ofc.
        if (!_canAttack) return;
        if (_enemyType == EnemyType.Flying) return;
        _agent.SetDestination(transform.position);
        float distance = Vector3.Distance(transform.position, _target.position);
        if (distance < _attackDistance)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, _attackDistance))
            {
                if (hit.transform.TryGetComponent<HealthScript>(out HealthScript healthScript))
                {
                    healthScript.TakeDamage(CalculateDamage());
                }
            }

            StartCoroutine(AttackCooldownRoutine());
            Debug.Log("Enemy Attacked");
        }

        else
        {
            _state = State.Chase;
        }
    }

    //Flee state
    private void Flee()
    {
        //Flee is some direction this is some random calculation idk
        Vector3 fleeDirection = -transform.forward;
        switch (_enemyType)
        {
            case EnemyType.Ground:
                _agent.SetDestination(fleeDirection);
                break;

            case EnemyType.Flying:
                MoveToPosition(fleeDirection);
            break;
        }
    }

    private void MoveToPosition(Vector3 targetPosition)
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _moveSpeed);
    }
    
    //Calculate the damage this is simple asf for now but can contain crits etc.
    private float CalculateDamage()
    {
        float finaldamage = Random.Range(_minDamage, _maxDamage);
        return finaldamage;
    }
    
    //Cooldown for the attacks
    private IEnumerator AttackCooldownRoutine()
    {
        if (!_canAttack) yield return null;
        _canAttack = false;
        yield return new WaitForSeconds(_attackSpeed);
        _canAttack = true;
    }
}
