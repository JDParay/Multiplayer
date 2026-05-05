using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    public float speed = 5f;
    public float jumpForce = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 1. MUST check ownership first
        if (!photonView.IsMine) return; 

        // 2. Old Input System movement (WASD / Arrows)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(moveX, 0, moveZ) * speed;
        rb.MovePosition(transform.position + move * Time.deltaTime);

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }

    void Jump()
    {
        // No extra check needed here because it's called inside the IsMine check above
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
}
}