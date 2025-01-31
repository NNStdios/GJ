using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{   
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private List<Transform> _spawnPoints;
    [SerializeField] private int _minSpawnAmount;
    [SerializeField] private int _maxSpawnAmount;
    [SerializeField] private TMP_Text _enemiesLeftText;

    public List<GameObject> SpawnedEnemies = new();
    private bool _canSpawn = true;

    private void Start()
    {
        StartCoroutine(SpawnEnemiesRoutine());
    }

    private void Update()
    {
        if (SpawnedEnemies.Count <= 0)
        {
            _canSpawn = true;
        }

        if (_canSpawn)
        {
            StartCoroutine(SpawnEnemiesRoutine());
            _canSpawn = false;
        }

        _enemiesLeftText.text = SpawnedEnemies.Count.ToString();
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = _spawnPoints[Random.Range(0, _spawnPoints.Count)].position;

        var enemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
        SpawnedEnemies.Add(enemy);
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        if (!_canSpawn) yield return null;
        int enemiesToSpawn = Random.Range(_minSpawnAmount, _maxSpawnAmount);
        while (enemiesToSpawn > 0)
        {
            SpawnEnemy();
            enemiesToSpawn--;
        }

        yield return null;
    }
}
