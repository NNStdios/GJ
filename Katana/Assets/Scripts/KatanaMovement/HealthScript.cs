using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthScript : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _maxHealth;

    private float _health;
    
    private void Start() => _health = _maxHealth;

    public void TakeDamage(float damage)
    {
        _health -= damage;
        Debug.Log("Took Damage: " + damage);

        if (_health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
