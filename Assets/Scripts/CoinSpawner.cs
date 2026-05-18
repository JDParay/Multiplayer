using UnityEngine;
using Photon.Pun;

public class CoinSpawner : MonoBehaviour
{
    public string coinPrefabName = "Coin"; 
    public Transform[] spawnPoints;
    public float spawnInterval = 2.5f;

    private float timer = 0f;
    private bool canSpawn = false;

    public void StartSpawning() => canSpawn = true;
    public void StopSpawning() => canSpawn = false;

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !canSpawn || spawnPoints.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnCoin();
        }
    }

    private void SpawnCoin()
    {
        Transform targetPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        PhotonNetwork.Instantiate(coinPrefabName, targetPoint.position, Quaternion.identity);
    }
}