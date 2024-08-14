using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int rows = 2; // Sat�r say�s�
    public int columns = 2; // S�tun say�s�
    public float cellWidth = 2f; // H�cre geni�li�i
    public float cellHeight = 2f; // H�cre y�ksekli�i

    public Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    public Vector3 GetNextSpawnPosition(int playerIndex)
    {
        int row = playerIndex / columns;
        int column = playerIndex % columns;
        Vector3 offset = new Vector3(column * cellWidth, 0, row * cellHeight);

        // Kamera taraf�ndan daha iyi g�r�lebilecek bir konum elde etmek i�in hafif bir ofset ekleyebiliriz
        // �rne�in, her karakter aras�nda biraz bo�luk b�rakmak i�in
        return startPos + offset;
    }
}
