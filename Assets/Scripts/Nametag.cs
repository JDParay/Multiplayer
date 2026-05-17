using Photon.Pun;
using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviour
{
    [Header("Text References")]
    public TMP_Text nameText;     
    public TMP_Text statusText;     
    public TMP_Text actionText;      
    public TMP_Text hostText;     

    [Header("Positioning")]
    public Vector3 offset = new Vector3(0, 2.2f, 0);

    private PhotonView pv;
    private Camera cam;

    private float leaveTimer = 0f;
    private bool isLeaving = false;
    private bool isEditing = false;

    private void Awake()
    {
        pv = GetComponentInParent<PhotonView>();
        cam = Camera.main;
        transform.localPosition = offset;
    }

    private void Start()
{
    if (pv != null && pv.Owner != null && nameText != null)
    {
        nameText.text = pv.Owner.NickName;
    }

    UpdateHostIndicator();
}

// Separate this into a method so we can keep evaluating it if master client changes
public void UpdateHostIndicator()
{
    if (hostText == null || pv == null || pv.Owner == null) return;

    // Check if the owner of this specific network player clone is the Master Client
    if (pv.Owner.IsMasterClient)
    {
        hostText.text = "<color=yellow><b>HOST</b></color>";
    }
    else
    {
        hostText.text = "";
    }
}

    private void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam != null)
        {
            transform.LookAt(transform.position + cam.transform.forward);
        }

        if (pv != null && pv.Owner != null && nameText != null)
        {
            // Continuously matches Photon's synced nickname data
            nameText.text = pv.Owner.NickName; 
        }

        UpdateReadyStatus();
        UpdateActionText();
    }

    public void RefreshName()
    {
        if (pv != null && pv.Owner != null && nameText != null)
        {
            nameText.text = pv.Owner.NickName;
        }
    }

    private void UpdateReadyStatus()
    {
        if (statusText == null || pv?.Owner == null) return;

        bool isReady = false;
        if (pv.Owner.CustomProperties.TryGetValue("IsReady", out object ready))
            isReady = (bool)ready;

        statusText.text = isReady ? 
            "<color=green>[READY]</color>" : 
            "<color=red>[NOT READY]</color>";
    }

    private void UpdateActionText()
    {
        if (actionText == null) return;

        if (isLeaving && leaveTimer > 0)
        {
            actionText.text = $"<color=orange>Leaving in {Mathf.Ceil(leaveTimer)}s</color>";
        }
        else if (isEditing)
        {
            actionText.text = "<color=cyan>[Editing Name]</color>";
        }
        else
        {
            actionText.text = "";
        }
    }

    // ====================== PUBLIC METHODS ======================

    public void StartLeaveCountdown(float duration)
    {
        isLeaving = true;
        leaveTimer = duration;
        StartCoroutine(LeaveCountdown());
    }

    public void StopLeaveCountdown()
    {
        isLeaving = false;
        leaveTimer = 0;
    }

    public void SetEditingMode(bool editing)
    {
        isEditing = editing;
    }

    private System.Collections.IEnumerator LeaveCountdown()
    {
        while (leaveTimer > 0 && isLeaving)
        {
            leaveTimer -= Time.deltaTime;
            yield return null;
        }
        isLeaving = false;
    }
}