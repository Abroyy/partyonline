using UnityEngine;
using Photon.Pun;

public class SawMovement : MonoBehaviourPun, IPunObservable
{
    public float moveDistance = 5f;
    public float moveSpeed = 2f; 
    public float rotateSpeed = 100f; 
    public bool moveRightInitially = true; 

    private Vector3 startPos;
    private bool isMoving = true;
    private bool isReturning = false;

    private Vector3 networkPosition;
    private Quaternion networkRotation;

    void Start()
    {
        startPos = transform.position; // Ba�lang�� pozisyonunu kaydet
        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    void Update()
    {
        if (photonView.IsMine)
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
        else
        {
            // E�er bu testerenin sahibi de�ilse, pozisyonu ve rotasyonu senkronize et
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * rotateSpeed);
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

    // Bu metot, testere pozisyonu ve rotasyonunu di�er oyunculara g�ndermek i�in kullan�l�r
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // E�er bu objenin sahibi ise, pozisyon ve rotasyon verilerini g�nder
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // E�er bu objenin sahibi de�ilse, pozisyon ve rotasyon verilerini al
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
