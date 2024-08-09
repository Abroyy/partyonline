using UnityEngine;

public class SawMovement : MonoBehaviour
{
    public float moveDistance = 5f; // Testerenin hareket edece�i mesafe
    public float moveSpeed = 2f; // Testerenin hareket etme h�z�
    public float rotateSpeed = 100f; // Testerenin d�nme h�z�
    public bool moveRightInitially = true; // Testerenin ba�lang��ta sa�a m� yoksa sola m� hareket edece�ini belirler

    private Vector3 startPos;
    private bool isMoving = true;
    private bool isReturning = false;

    void Start()
    {
        startPos = transform.position; // Ba�lang�� pozisyonunu kaydet
    }

    void Update()
    {
        RotateSaw();

        if (isMoving)
        {
            MoveSaw();
        }
        else if (isReturning)
        {
            ReturnSaw();
        }
    }

    private void MoveSaw()
    {
        // Testerenin sa�a m� yoksa sola m� hareket edece�ine g�re hareket ettir
        if (moveRightInitially)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            if (transform.position.x >= startPos.x + moveDistance)
            {
                isMoving = false;
                isReturning = true;
            }
        }
        else
        {
            transform.position -= Vector3.right * moveSpeed * Time.deltaTime;
            if (transform.position.x <= startPos.x - moveDistance)
            {
                isMoving = false;
                isReturning = true;
            }
        }
    }

    private void ReturnSaw()
    {
        // Testerenin geri d�nmesini sa�la
        if (moveRightInitially)
        {
            transform.position -= Vector3.right * moveSpeed * Time.deltaTime;
            if (transform.position.x <= startPos.x)
            {
                isReturning = false;
                isMoving = true;
            }
        }
        else
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            if (transform.position.x >= startPos.x)
            {
                isReturning = false;
                isMoving = true;
            }
        }
    }

    private void RotateSaw()
    {
        // Testerenin d�nmesini sa�la
        transform.Rotate(Vector3.right * rotateSpeed * Time.deltaTime);
    }
}
