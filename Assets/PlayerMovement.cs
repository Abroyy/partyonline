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
    private bool isDead = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        // Sahnedeki joystick ve jump butonunu bul
        joystick = FindObjectOfType<Joystick>();
        jumpButton = GameObject.Find("JumpButton")?.GetComponent<Button>();

        // Jump butonuna t�klama olay�n� ba�la
        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpButtonClicked);
        }
        else
        {
            Debug.LogError("JumpButton not found in the scene.");
        }
    }

    private void Update()
    {
        // Sadece kendi karakterimiz i�in kontrolleri yap
        if (photonView.IsMine)
        {
            if (joystick != null)
            {
                // Karakterin y�n�n� smooth �ekilde d�nd�r
                Vector3 moveDirection = new Vector3(joystick.Horizontal(), 0, joystick.Vertical()).normalized;
                if (moveDirection != Vector3.zero)
                {
                    // Karakterin y�n�n� d�nd�r
                    Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                    transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 10f);

                    // Y�r�y�� animasyonunu tetikle
                    animator.SetBool("Walking", true);
                }
                else
                {
                    // Y�r�y�� animasyonunu durdur
                    animator.SetBool("Walking", false);
                }

                rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
            }

            // Jump butonuna t�klan�p t�klanmad���n� kontrol et
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

        // Z�plama animasyonunu tetikle
        animator.SetTrigger("Jump");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    // Jump butonuna t�klama i�lemini i�leyen metod
    private void OnJumpButtonClicked()
    {
        if (isGrounded)
        {
            Jump();
        }
    }
}
