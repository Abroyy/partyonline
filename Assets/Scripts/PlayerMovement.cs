using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    private Joystick joystick;
    private Button jumpButton;
    private Rigidbody rb;
    private bool isGrounded;

    private PhotonView photonView;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        // Sahnedeki joystick ve jump butonunu bul
        joystick = FindObjectOfType<Joystick>();
        jumpButton = GameObject.Find("JumpButton")?.GetComponent<Button>();

        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpButtonClicked);
        }
        else
        {
            Debug.LogError("JumpButton not found in the scene.");
        }

        if (joystick != null)
        {
            Debug.Log("Joystick bulundu");
        }
        else
        {
            Debug.LogError("Joystick not found in the scene.");
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (joystick != null)
            {
                // Karakterin y�n�n� smooth �ekilde d�nd�r
                Vector3 moveDirection = new Vector3(joystick.Horizontal(), 0, joystick.Vertical()).normalized;
                if (moveDirection != Vector3.zero)
                {
                    Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                    transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 10f);
                    animator.SetBool("Walking", true);
                }
                else
                {
                    animator.SetBool("Walking", false);
                }

                rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
            }

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                Jump();
            }
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        animator.SetTrigger("Jump");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnJumpButtonClicked()
    {
        if (isGrounded)
        {
            Jump();
        }
    }

    public void SetJoystickActive(bool active)
    {
        if (joystick != null)
        {
            joystick.gameObject.SetActive(active);
        }
    }

    public void SetJumpButtonActive(bool active)
    {
        if (jumpButton != null)
        {
            jumpButton.interactable = active;
        }
    }
}
