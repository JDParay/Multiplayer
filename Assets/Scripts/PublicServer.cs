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
    private Dictionary<int, GameObject> playerListEntries = new Dictionary<int, GameObject>();

    private void Start()
{
    if (PhotonNetwork.InRoom)
    {
        lobbyTitleText.text = "Lobby #" + PhotonNetwork.CurrentRoom.Name;
        
        RefreshPlayerList(); 
        UpdateLobbyUI();
    }
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
            PhotonNetwork.LoadLevel("GameScene");
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
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out GameObject entry))
        {
            UpdateEntryDisplay(entry, targetPlayer);
        }
        UpdateLobbyUI();
    }

    private void UpdateEntryDisplay(GameObject entry, Player player)
    {
        // Assuming your prefab has Name text and Status text
        TMP_Text[] texts = entry.GetComponentsInChildren<TMP_Text>();
        texts[0].text = player.NickName;

        if (player.CustomProperties.TryGetValue("IsReady", out object ready) && (bool)ready)
        {
            texts[1].text = "<color=green>Ready</color>";
        }
        else
        {
            texts[1].text = "<color=red>Not Ready</color>";
        }
    }

    // --- EXISTING LOGIC UPDATED ---

    private void UpdateLobbyUI()
    {
        int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        playerCountText.text = $"{currentPlayers} / {PhotonNetwork.CurrentRoom.MaxPlayers}";

        if (PhotonNetwork.IsMasterClient)
        {
            actionButtonText.text = "Start Game";
            // Check if 3+ players AND all others are ready
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
            if (p.IsMasterClient) continue; // Master doesn't need to be "Ready"
            if (p.CustomProperties.TryGetValue("IsReady", out object ready))
            {
                if (!(bool)ready) return false;
            }
            else return false; // Property doesn't exist yet
        }
        return true;
    }

    public override void OnJoinedRoom()
    {
        // Reset local ready status when joining new room
        Hashtable props = new Hashtable { { "IsReady", false } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        foreach (GameObject obj in playerListEntries.Values) Destroy(obj);
        playerListEntries.Clear();

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            AddPlayerToList(p);
        }
        UpdateLobbyUI();
    }

    private void AddPlayerToList(Player player)
    {
        GameObject entry = Instantiate(playerEntryPrefab, contentPanel);
        UpdateEntryDisplay(entry, player);
        playerListEntries.Add(player.ActorNumber, entry);
    }
    
    public void SetMaxPlayersFromInput(string input)
{
    if (int.TryParse(input, out int newMax))
    {
        // Photon MaxPlayers uses 'byte' (0-255)
        byte clampedMax = (byte)Mathf.Clamp(newMax, 3, 12); 
        
        PhotonNetwork.CurrentRoom.MaxPlayers = clampedMax;
        
        // Update the local UI immediately
        UpdateLobbyUI();
        
        Debug.Log("Max Players updated to: " + clampedMax);
    }
}
public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
{
    // When the host changes MaxPlayers, this fires for EVERYONE
    UpdateLobbyUI(); 
}
    public void OnClickLeaveLobby() => PhotonNetwork.LeaveRoom();
    public override void OnLeftRoom() => UnityEngine.SceneManagement.SceneManager.LoadScene("LandingPage");
}