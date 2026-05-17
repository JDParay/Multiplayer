using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI; // Added for Button interaction
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Lobby3DManager : MonoBehaviourPunCallbacks
{
    public static Lobby3DManager Instance;
    private GameMode currentGameMode;

    [Header("UI General")]
    public TMP_Text gameModeText;

    [Header("Name Change UI")]
    public GameObject nameChangePanel;
    public TMP_InputField nameInputField;
    public Button confirmButton;
    public Button cancelButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameMode", out object modeObj))
        {
            currentGameMode = (GameMode)System.Convert.ToByte(modeObj);
        }

        SetReady(false);

        // Setup UI Button listeners
        if (confirmButton != null) confirmButton.onClick.AddListener(SubmitNameChange);
        if (cancelButton != null) cancelButton.onClick.AddListener(HideNameChangeUI);
        
        if (nameChangePanel != null) nameChangePanel.SetActive(false);
    }

    // --- LEAVE LOGIC ---
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

    // --- NAME CHANGE UI LOGIC ---
    public void ShowNameChangeUI()
    {
        if (nameChangePanel == null || nameInputField == null) return;

        // Pre-populate input field with current nickname
        nameInputField.text = PhotonNetwork.NickName;
        nameChangePanel.SetActive(true);

        // Unlock cursor so player can type/click
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        nameInputField.ActivateInputField(); // Auto-focus text box
    }

    public void HideNameChangeUI()
    {
        if (nameChangePanel == null) return;
        
        nameChangePanel.SetActive(false);
        SetEditingStatus(false);
    }

    public void SetEditingStatus(bool isEditing)
    {
        Hashtable props = new Hashtable { { "IsEditing", isEditing } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void SubmitNameChange()
    {
        if (nameInputField == null || string.IsNullOrWhiteSpace(nameInputField.text)) return;

        string newName = nameInputField.text.Trim();
        
        // 1. Update local Photon Nickname
        PhotonNetwork.NickName = newName;

        Hashtable prop = new Hashtable { { "UpdateName", Random.Range(0, 10000) } }; 
        PhotonNetwork.LocalPlayer.SetCustomProperties(prop);

        HideNameChangeUI();
    }

    // --- READY LOGIC ---
    public void SetReady(bool ready)
    {
        Hashtable props = new Hashtable { { "IsReady", ready } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void ToggleLocalReady()
    {
        bool current = false;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsReady", out object ready))
            current = (bool)ready;

        SetLocalPlayerReady(!current);
    }

    public void SetLocalPlayerReady(bool isReady)
    {
        Hashtable props = new Hashtable { { "IsReady", isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}