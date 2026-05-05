using Photon.Pun;
using UnityEngine;

public class GameRoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("❌ Player Prefab is not assigned in GameRoomManager!");
            return;
        }

        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        Vector3 spawnPos = Vector3.zero;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = PhotonNetwork.LocalPlayer.ActorNumber % spawnPoints.Length;
            spawnPos = spawnPoints[index].position;
        }

        GameObject playerObj = PhotonNetwork.Instantiate("Prefabs/PlayerPrefab", spawnPos, Quaternion.identity);

        Debug.Log($"✅ Spawned player: {PhotonNetwork.LocalPlayer.NickName} at {spawnPos}");
    }
}