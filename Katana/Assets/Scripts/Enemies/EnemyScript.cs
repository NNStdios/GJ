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
        Attack
    }

    [Header("Enemy Stats")]
    [SerializeField] private float _attackSpeed = 1.6f;
    [SerializeField] private float _minDamage = 10;
    [SerializeField] private float _maxDamage = 20;
    [SerializeField] private float _attackDistance = 5f;
    [SerializeField] private float _maxHealth = 20f;

    [Header("Referecnes")]
    [SerializeField] private ParticleSystem _bloodParticles;

    private Transform _target;

    private NavMeshAgent _agent;
    private State _state;
    private bool _canAttack = true;
    private bool _canBeAttacked;
    private float _health;
    private Animator _animator;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        _health = _maxHealth;

        _target = GameManager.Instance.Player;
        _state = State.Chase;
    }

    private void Update()
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
        }
    }

    private void Idle()
    {
        SetDestination(transform.position);
    }

    //Chase state
    private void Chase()
    {
        SetDestination(_target.position);

        if (Vector3.Distance(transform.position, _target.position) < _attackDistance)
        {
            _state = State.Attack;
        }
    }

    //Attack state
    private void Attack()
    {
        if (!_canAttack) return;
        float distance = Vector3.Distance(transform.position, _target.position);
        if (distance < _attackDistance)
        {

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _attackDistance);
            foreach (var hitCollider in hitColliders)
            {
                Debug.Log(hitCollider.transform);
                if (hitCollider.TryGetComponent<HealthScript>(out HealthScript healthScript))
                {
                    StartCoroutine(DamageRoutine(healthScript));
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

    private void SetDestination(Vector3 destination)
    {
        Vector3 movePosition = destination;
        movePosition.y = 0;
        _agent.SetDestination(movePosition);
    }

    private void TakeDamage(float damage)
    {
        if (!_canBeAttacked) return;
        _health -= damage;

        if (_health >= 0)
        {
            GameManager.Instance.Player.SetParent(null);
            GameManager.EnemySpawnerScript.SpawnedEnemies.Remove(gameObject);
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
        Debug.Log("Weak");
        _state = State.Idle;
        _animator.SetTrigger("Attacking");
        yield return new WaitForSecondsRealtime(_attackSpeed);
        _canAttack = true;
        _state = State.Chase;
        Debug.Log("Not weak");
        _canBeAttacked = false;
    }

    private IEnumerator DamageRoutine(HealthScript healthScript)
    {
        yield return new WaitForSeconds(0.2f);
        healthScript.TakeDamage(CalculateDamage());

    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            if (rb.linearVelocity.magnitude > 10f)
            {
                if (collision.gameObject.tag == "Player")
                {
                    TakeDamage(20);
                }
            }
        }
    }
}
