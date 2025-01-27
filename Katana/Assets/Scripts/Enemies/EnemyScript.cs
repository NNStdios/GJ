using System.Collections;
using Assets.Scripts.Core;
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

    [Header("Enemy Stats")]
    [SerializeField] private float _attackSpeed = 1.6f;
    [SerializeField] private float _minDamage = 10;
    [SerializeField] private float _maxDamage = 20;
    [SerializeField] private float _spotDistance = 50f;
    [SerializeField] private float _attackDistance = 5f;
    [SerializeField] private float _maxHealth = 20f;
    [SerializeField] private float _fleeHealth = 20f;

    [Header("Referecnes")]
    [SerializeField] private ParticleSystem _bloodParticles;

    private Transform _target;

    private NavMeshAgent _agent;
    private State _state;
    private bool _canAttack = true;
    private bool _canBeAttacked;
    private float _health;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _health = _maxHealth;

        _target = GameManager.Instance.Player;
    }

    private void Update()
    {
        //If no target found be idle and look for a target
        if (_target == null)
        {
            _state = State.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, _target.transform.position);
        if (distance > _spotDistance)
        {
            _state = State.Idle;
        }

        else if (_health <= _fleeHealth)
        {
            _state = State.Flee;
        }

        else
        {
            _state = State.Chase;
        }

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
    }

    //Idle state
    private void Idle()
    {
        _agent.SetDestination(transform.position);
    }

    //Chase state
    private void Chase()
    {
        _agent.SetDestination(_target.position);

        if (Vector3.Distance(transform.position, _target.position) < _attackDistance)
        {
            _state = State.Attack;
        }
    }

    //Flee state
    private void Flee()
    {
        //Flee is some direction this is some random calculation idk
        Vector3 fleeDirection = (transform.position - _target.position).normalized * _spotDistance;
        Vector3 destination = transform.position + fleeDirection;

        _agent.SetDestination(destination);
    }

    //Attack state
    private void Attack()
    {   //Raycast based attacking Physics.OverlapSphere can also work well here but the collider issue and it's present with raycasts too ofc.
        if (!_canAttack) return;
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

    private void TakeDamage(float damage)
    {
        if (!_canBeAttacked) return;
        _health -= damage;

        if (_health >= 0)
        {
            GameManager.Instance.Player.SetParent(null);
            Destroy(gameObject);
        }
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
        _canBeAttacked = true;
        yield return new WaitForSeconds(_attackSpeed);
        _canAttack = true;
        _canBeAttacked = false;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            TakeDamage(20);
        }
    }
}
