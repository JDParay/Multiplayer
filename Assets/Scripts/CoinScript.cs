using UnityEngine;
using Photon.Pun;

public class CoinScript : MonoBehaviourPun
{
    private bool isCollected = false;
    public AudioClip sfx;

    private void OnTriggerEnter(Collider other)
    {
        // Ensure only the local player hitting it triggers the collection logic
        if (!isCollected && other.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine)
        {
            isCollected = true;
            
            // 1. Add score to custom properties
            int currentScore = 0;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Score", out object scoreObj))
            {
                currentScore = (int)scoreObj;
            }
            
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable { { "Score", currentScore + 1 } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            AudioSource.PlayClipAtPoint(sfx, transform.position);

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                // Send an RPC command or request owner destruction
                photonView.RPC("RequestDestroyCoin", RpcTarget.MasterClient);
            }
        }
    }

    [PunRPC]
    private void RequestDestroyCoin()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}