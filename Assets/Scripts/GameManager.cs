using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.Instantiate("PlayerPrefab", new Vector3(0,1,0), Quaternion.identity);
    }
}
