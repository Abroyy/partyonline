using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int rows = 1; // Sat�r say�s�
    public int columns = 4; // S�tun say�s�
    public float cellWidth = 2f; // H�cre geni�li�i
    public float cellHeight = 2f; // H�cre y�ksekli�i

    public Vector3 startPos;

    private HashSet<int> occupiedCells = new HashSet<int>();

    private void Start()
    {
        startPos = transform.position;
    }

    public Vector3 GetNextAvailableSpawnPosition(Transform cameraTransform)
    {
        // Bo� h�creyi bul
        for (int i = 0; i < rows * columns; i++)
        {
            if (!occupiedCells.Contains(i))
            {
                // Bu h�creyi i�aretle
                occupiedCells.Add(i);

                // Sat�r ve s�tunu hesapla
                int column = i % columns;
                int row = i / columns;

                // Grid'deki pozisyonu hesapla
                Vector3 localOffset = new Vector3(column * cellWidth, 0, row * cellHeight);

                // Kameran�n y�n�ne g�re pozisyonu ayarla
                Vector3 worldOffset = cameraTransform.rotation * localOffset;

                // Pozisyonu d�nd�r
                return startPos + worldOffset;
            }
        }

        // E�er t�m h�creler doluysa
        Debug.LogWarning("T�m h�creler dolu.");
        return startPos; // Yedek pozisyon d�nd�rebilirsiniz
    }

    public void FreeCell(Vector3 position, Transform cameraTransform)
    {
        // Pozisyondan sat�r ve s�tun hesapla ve h�creyi bo�alt
        Vector3 localOffset = Quaternion.Inverse(cameraTransform.rotation) * (position - startPos);
        int column = Mathf.RoundToInt(localOffset.x / cellWidth);
        int row = Mathf.RoundToInt(localOffset.z / cellHeight);
        int cellIndex = row * columns + column;

        // H�creyi tekrar kullan�labilir hale getir
        occupiedCells.Remove(cellIndex);
    }
}
