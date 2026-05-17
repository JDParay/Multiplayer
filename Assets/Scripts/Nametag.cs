using Photon.Pun;
using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviour
{
    [Header("References")]
    public TMP_Text nameText;
    public TMP_Text statusText;

    [Header("Position")]
    public Vector3 offset = new Vector3(0, 2.8f, 0);   // Tweak this

    private PhotonView pv;
    private Camera cam;

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
    }

    private void LateUpdate()
    {
        if (cam == null) cam = Camera.main;

        if (cam != null)
        {
            transform.LookAt(transform.position + cam.transform.forward);
        }

        if (statusText != null && pv != null && pv.Owner != null)
        {
            bool isReady = false;
            if (pv.Owner.CustomProperties.TryGetValue("IsReady", out object ready))
            {
                isReady = (bool)ready;
            }

            statusText.text = isReady ? 
                "<color=green>[Ready]</color>" : 
                "<color=red>[Not Ready]</color>";
        }
    }
}