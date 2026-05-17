using Photon.Pun;
using UnityEngine;

public class LobbyPlatform : MonoBehaviour
{
    public enum PlatformType { Ready, ChangeName, Leave }

    public PlatformType type;

    private PhotonView localPlayerPV;
    private float standingTime;
    private bool playerOnPlatform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine)
        {
            localPlayerPV = other.GetComponent<PhotonView>();
            playerOnPlatform = true;
            standingTime = 0f;

            if (type == PlatformType.ChangeName)
                ShowNameChangeUI();   // Implement your UI popup
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine)
        {
            playerOnPlatform = false;
        }
    }

    private void Update()
    {
        if (!playerOnPlatform) return;

        standingTime += Time.deltaTime;

        if (type == PlatformType.Ready && standingTime > 0.5f)
        {
            Lobby3DManager.Instance.SetReady(true);
            playerOnPlatform = false;
        }

        if (type == PlatformType.Leave && standingTime >= 5f)
        {
            Lobby3DManager.Instance.StartLeaveProcess();
        }
    }

    private void ShowNameChangeUI()
    {
        // On confirm: PhotonNetwork.NickName = newName;
    }
}