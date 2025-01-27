using System.Collections;
using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{   
    [SerializeField] private GameObject _enemyPrefab;

    [SerializeField] private float _waves;
    [SerializeField] private float _spawnRate;
    [SerializeField] private float _enemiesPerSpawn;
    [SerializeField] private float _difficultyScaleRate;
    [SerializeField] private float _spawnRadius;

    private bool _spawning;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = Random.insideUnitSphere * _spawnRadius;
        spawnPosition.y = 0.2f;

        Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
    }

    private IEnumerator SpawnRoutine()
    {
        if (_spawning) yield return null;
        _spawning = true;

        while (_enemiesPerSpawn > 0)
        {
            SpawnEnemy();
            _enemiesPerSpawn--;
            Debug.Log(_enemiesPerSpawn);

            yield return null;
        }

        _spawning = false;
    }
}
