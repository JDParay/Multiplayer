using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_Text lobbyTitleText;
    public TMP_Text playerCountText;
    public Button actionButton;
    public TMP_Text actionButtonText;
    
    [Header("Player List Setup")]
    public GameObject playerEntryPrefab; 
    public Transform contentPanel;
    public TMP_InputField maxPlayersInput;       
    private Dictionary<int, GameObject> playerListEntries = new Dictionary<int, GameObject>();

    private void Start()
    {
        RefreshPlayerList();
        UpdateLobbyUI();
    }

    private void RefreshPlayerList()
    {
        foreach (GameObject obj in playerListEntries.Values) Destroy(obj);
        playerListEntries.Clear();

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            AddPlayerToList(p);
        }
    }

    // --- READY SYSTEM LOGIC ---

    public void OnClickActionButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {

            PhotonNetwork.LoadLevel("GameRoom");
        }
        else
        {

            bool isReady = false;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsReady", out object ready))
            {
                isReady = (bool)ready;
            }

            Hashtable props = new Hashtable { { "IsReady", !isReady } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("IsReady"))
        {
            if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out GameObject entry))
            {
                UpdateEntryDisplay(entry, targetPlayer);
            }
            UpdateLobbyUI();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"New player joined: {newPlayer.NickName}");

        RefreshPlayerList();
        
        UpdateLobbyUI();
    }

        public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playerListEntries.ContainsKey(otherPlayer.ActorNumber))
        {
            Destroy(playerListEntries[otherPlayer.ActorNumber]);
            playerListEntries.Remove(otherPlayer.ActorNumber);
        }

        UpdateLobbyUI(); 
    }

    private void UpdateEntryDisplay(GameObject entry, Player player)
    {
        TMP_Text nameText = entry.transform.Find("NameText").GetComponent<TMP_Text>();
        TMP_Text statusText = entry.transform.Find("StatusText").GetComponent<TMP_Text>();

        nameText.text = player.NickName;

        if (player.IsMasterClient)
        {
            statusText.text = "<color=yellow><b>Host</b></color>";
        }
        else
        {
            bool isReady = false;
            if (player.CustomProperties.TryGetValue("IsReady", out object ready))
            {
                isReady = (bool)ready;
            }

            statusText.text = isReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"New Master Client: {newMasterClient.NickName}");
        
        RefreshPlayerList();
        UpdateLobbyUI();
    }

    // --- EXISTING LOGIC ---

    private void UpdateLobbyUI()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        playerCountText.text = $"{currentPlayers} / {PhotonNetwork.CurrentRoom.MaxPlayers}";

        if (maxPlayersInput != null)
        {
            maxPlayersInput.text = PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
            maxPlayersInput.interactable = PhotonNetwork.IsMasterClient;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            actionButtonText.text = "Start Game";
            actionButton.interactable = CheckIfEveryoneIsReady() && currentPlayers >= 3;
        }
        else
        {
            bool isReady = false;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsReady", out object ready))
                isReady = (bool)ready;

            actionButtonText.text = isReady ? "Unready" : "Ready";
            actionButton.interactable = true;
        }
    }

        private bool CheckIfEveryoneIsReady()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.IsMasterClient) continue;

            if (p.CustomProperties.TryGetValue("IsReady", out object ready))
            {
                if (!(bool)ready) return false;
            }
            else 
            {
                return false;
            }
        }
        return true;
    }

    public override void OnJoinedRoom()
    {
        if (lobbyTitleText != null)
            lobbyTitleText.text = "Lobby #" + PhotonNetwork.CurrentRoom.Name;

        Hashtable props = new Hashtable { { "IsReady", false } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        foreach (GameObject obj in playerListEntries.Values) Destroy(obj);
        playerListEntries.Clear();

        RefreshPlayerList();
        UpdateLobbyUI();

        maxPlayersInput.interactable = PhotonNetwork.IsMasterClient;
    }
    private void AddPlayerToList(Player player)
    {
        GameObject entry = Instantiate(playerEntryPrefab, contentPanel);
        UpdateEntryDisplay(entry, player);
        playerListEntries.Add(player.ActorNumber, entry);
    }
    
    public void SetMaxPlayersFromInput(string input)
    {
        if (!PhotonNetwork.IsMasterClient) 
        {
            if (maxPlayersInput != null && PhotonNetwork.CurrentRoom != null)
                maxPlayersInput.text = PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
            return;
        }

        if (int.TryParse(input, out int newMax))
        {
            byte clampedMax = (byte)Mathf.Clamp(newMax, 3, 12);
            
            // This should trigger OnRoomPropertiesUpdate for everyone
            PhotonNetwork.CurrentRoom.MaxPlayers = clampedMax;

            // Force local update immediately
            UpdateLobbyUI();

            Debug.Log($"Max Players changed to: {clampedMax}");
        }
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        UpdateLobbyUI();

        if (lobbyTitleText != null && PhotonNetwork.CurrentRoom != null)
            lobbyTitleText.text = "Lobby #" + PhotonNetwork.CurrentRoom.Name;
    }
        public void OnClickLeaveLobby() => PhotonNetwork.LeaveRoom();
        public override void OnLeftRoom() => UnityEngine.SceneManagement.SceneManager.LoadScene("LandingPage");
    }