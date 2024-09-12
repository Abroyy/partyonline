using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public List<Transform> spawnPoints;

    private HashSet<Transform> usedSpawnPoints = new HashSet<Transform>();

    public Transform GetRandomSpawnPoint()
    {
        List<Transform> availableSpawnPoints = new List<Transform>();

        // Kullan�lmayan spawn noktalar�n� bul
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (!usedSpawnPoints.Contains(spawnPoint))
            {
                availableSpawnPoints.Add(spawnPoint);
            }
        }

        // E�er kullan�lmayan bir nokta varsa, rastgele bir tane se�
        if (availableSpawnPoints.Count > 0)
        {
            int index = Random.Range(0, availableSpawnPoints.Count);
            Transform selectedSpawnPoint = availableSpawnPoints[index];
            usedSpawnPoints.Add(selectedSpawnPoint);
            return selectedSpawnPoint;
        }
        else
        {
            Debug.LogWarning("T�m spawn noktalar� dolu!");
            return null;
        }
    }

    public void ReleaseSpawnPoint(Transform spawnPoint)
    {
        if (usedSpawnPoints.Contains(spawnPoint))
        {
            usedSpawnPoints.Remove(spawnPoint);
        }
    }
}

