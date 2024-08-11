using UnityEngine;

public class SwingingAxe : MonoBehaviour
{
    public float swingAngle = 30f; // Balta sa�a ve sola ne kadar d�necek
    public float swingSpeed = 2f; // Baltan�n d�nece�i h�z

    private float startAngle;
    private bool swingingRight = true;

    void Start()
    {
        // Ba�lang�� a��s�n� kaydet
        startAngle = transform.eulerAngles.z;
    }

    void Update()
    {
        Swing();
    }

    private void Swing()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        transform.rotation = Quaternion.Euler(0, 0, startAngle + angle);
    }
}
