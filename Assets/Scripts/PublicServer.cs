using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable; // Required for Custom Properties

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_Text lobbyTitleText;
    public TMP_Text playerCountText;
    public Button actionButton; // Rename your "Start Game" button to this
    public TMP_Text actionButtonText; // The text inside the button
    
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
    // Clear existing entries first
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
            // Master Client starts the game
            PhotonNetwork.LoadLevel("GameRoom");
        }
        else
        {
            // Regular player toggles ready status
            bool isReady = false;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsReady", out object ready))
            {
                isReady = (bool)ready;
            }

            Hashtable props = new Hashtable { { "IsReady", !isReady } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }

    // This triggers whenever ANY player changes their properties (like IsReady)
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
{
    if (changedProps.ContainsKey("IsReady"))
    {
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out GameObject entry))
        {
            UpdateEntryDisplay(entry, targetPlayer);
        }
        UpdateLobbyUI(); // Re-check if Master can start the game now
    }
}

public override void OnPlayerEnteredRoom(Player newPlayer)
{
    Debug.Log($"New player joined: {newPlayer.NickName}");

    // Refresh the full player list for existing players
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

    // Set Player Name
    nameText.text = player.NickName;

    // === NEW LOGIC: Show "Host" for Master Client ===
    if (player.IsMasterClient)
    {
        statusText.text = "<color=yellow><b>Host</b></color>";
    }
    else
    {
        // Regular players show Ready status
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
    
    // Refresh all player entries so the new host shows "Host"
    RefreshPlayerList();
    UpdateLobbyUI();
}

    // --- EXISTING LOGIC UPDATED ---

 private void UpdateLobbyUI()
{
    if (PhotonNetwork.CurrentRoom == null) return;

    int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
    playerCountText.text = $"{currentPlayers} / {PhotonNetwork.CurrentRoom.MaxPlayers}";

    // === CRITICAL: Update Input Field for ALL players ===
    if (maxPlayersInput != null)
    {
        maxPlayersInput.text = PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
        maxPlayersInput.interactable = PhotonNetwork.IsMasterClient;
    }

    // Action button logic
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

    // Reset ready status
    Hashtable props = new Hashtable { { "IsReady", false } };
    PhotonNetwork.LocalPlayer.SetCustomProperties(props);

    // Refresh everything
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