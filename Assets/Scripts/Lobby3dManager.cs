using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Lobby3DManager : MonoBehaviourPunCallbacks
{
    public static Lobby3DManager Instance;

    [Header("Environments")]
    public GameObject lobbyEnvironment;
    public GameObject minigameEnvironment;

    [Header("Name Change UI")]
    public GameObject nameChangePanel;
    public TMP_InputField nameInputField;

    private void Awake()
    {
        Instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        SetReady(false);
    }

    // ====================== NAME CHANGE ======================
    public void ShowNameChangeUI()
    {
        if (nameChangePanel == null || nameInputField == null) return;

        nameInputField.text = PhotonNetwork.NickName;
        nameChangePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        nameInputField.ActivateInputField();
    }

    public void HideNameChangeUI()
    {
        if (nameChangePanel != null) nameChangePanel.SetActive(false);
    }

    public void SetEditingStatus(bool isEditing)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "IsEditing", isEditing } });
    }

    // ====================== READY ======================
    public void SetReady(bool ready)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "IsReady", ready } });
    }

    public void ToggleLocalReady()
    {
        bool current = false;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsReady", out object r) && r is bool b)
            current = b;

        SetReady(!current);
    }

    // ====================== LEAVE ======================
    public void StartLeaveProcess()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LandingPage");
    }

    // ====================== SAFE READY CHECK ======================
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!changedProps.ContainsKey("IsReady") || !PhotonNetwork.IsMasterClient)
            return;

        Invoke(nameof(CheckIfAllReadySafe), 0.2f); // Give more time for properties to settle
    }

    private void CheckIfAllReadySafe()
    {
        try
        {
            if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.PlayerList == null)
                return;

            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount <= 0) return;

            int readyCount = 0;

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p == null) continue;

                bool isReady = false;
                var props = p.CustomProperties;

                if (props != null)
                {
                    if (props.TryGetValue("IsReady", out object obj) && obj is bool b)
                        isReady = b;
                }

                if (isReady)
                    readyCount++;
            }

            if (readyCount >= playerCount)
            {
                Debug.Log("✅ ALL PLAYERS READY - Starting game!");
                photonView.RPC("RPC_TransitionToMinigame", RpcTarget.All);
            }
        }
        catch (System.Exception ex)
        {
            // Silent fail during join phase - this is expected sometimes
            // Debug.LogWarning("CheckIfAllReadySafe failed: " + ex.Message);
        }
    }

    [PunRPC]
    private void RPC_TransitionToMinigame()
    {
        Debug.Log("🔄 Transitioning to Minigame!");

        if (lobbyEnvironment != null) lobbyEnvironment.SetActive(false);
        if (minigameEnvironment != null) minigameEnvironment.SetActive(true);

        var spawner = FindFirstObjectByType<GameplaySpawner>();
        if (spawner != null)
            spawner.MoveExistingPlayerToMatch();
    }
}