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
        return startPos + new Vector3(column * cellWidth, 0, row * cellHeight);
    }
}
