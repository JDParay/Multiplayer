using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Lobby3DManager : MonoBehaviourPunCallbacks
{
    public static Lobby3DManager Instance;
    private GameMode currentGameMode;

    [Header("UI")]
    public TMP_Text gameModeText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Load game mode from room properties
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameMode", out object modeObj))
        {
            currentGameMode = (GameMode)System.Convert.ToByte(modeObj);
            
            if (gameModeText != null)
                gameModeText.text = currentGameMode.ToString().Replace("TheButton", " the Button"); // nicer display
        }

        SetReady(false);
    }

    public void SetReady(bool ready)
    {
        Hashtable props = new Hashtable { { "IsReady", ready } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    // Call this from your platform trigger
    public void StartLeaveProcess() => StartCoroutine(LeaveAfterDelay(5f));
    private IEnumerator LeaveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LandingPage");
    }
}