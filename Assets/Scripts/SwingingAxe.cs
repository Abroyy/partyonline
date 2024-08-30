using UnityEngine;
using Photon.Pun;

public class SwingingAxe : MonoBehaviourPun, IPunObservable
{
    public float swingAngle = 30f; // Balta sa�a ve sola ne kadar d�necek
    public float swingSpeed = 2f; // Baltan�n d�nece�i h�z

    private float startAngle;
    private float currentAngle;

    private float networkAngle;

    void Start()
    {
        // Ba�lang�� a��s�n� kaydet
        startAngle = transform.eulerAngles.z;
        currentAngle = startAngle;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            Swing();
        }
        else
        {
            // Di�er oyuncular i�in senkronize bir �ekilde a�� de�i�imini uygula
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(currentAngle, networkAngle, Time.deltaTime * swingSpeed));
            currentAngle = transform.eulerAngles.z;
        }
    }

    private void Swing()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        transform.rotation = Quaternion.Euler(0, 0, startAngle + angle);
        currentAngle = startAngle + angle;
    }

    // Photon'un senkronizasyon metodunu kullanarak a��y� g�ncelleriz
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // E�er bu objenin sahibi ise, a��y� di�er oyunculara g�nder
            stream.SendNext(currentAngle);
        }
        else
        {
            // E�er bu objenin sahibi de�ilse, a��y� al ve g�ncelle
            networkAngle = (float)stream.ReceiveNext();
        }
    }
}
