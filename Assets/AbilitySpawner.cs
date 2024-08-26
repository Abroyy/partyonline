using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySpawner : MonoBehaviour
{
    // Colliderlar�n yer ald��� bo� gameObject'lerin transform'lar�
    public Transform[] spawnPoints;

    // Yetenek prefab'lar�
    public GameObject[] abilities;  // Temel yetenekler (�rne�in H�z, Can, Sald�r�)

    // Her 10 saniyede bir rastgele iki yetene�i spawn eder.
    public float spawnInterval = 10f;

    private void Start()
    {
        // Spawn i�lemini ba�lat
        StartCoroutine(SpawnAbilities());
    }

    private IEnumerator SpawnAbilities()
    {
        while (true)
        {
            // Collider'lar�n oldu�u noktalardan rastgele iki tane se�
            List<Transform> chosenSpawnPoints = ChooseRandomSpawnPoints(2);

            // Temel yeteneklerden rastgele iki tanesini se�
            List<GameObject> chosenAbilities = ChooseRandomAbilities(2);

            // Se�ilen yetenekleri, se�ilen collider'lar�n bulundu�u noktalara spawn et
            for (int i = 0; i < chosenAbilities.Count; i++)
            {
                Instantiate(chosenAbilities[i], chosenSpawnPoints[i].position, Quaternion.identity);
            }

            // 10 saniye bekle
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Rastgele iki tane spawn point se�er
    private List<Transform> ChooseRandomSpawnPoints(int count)
    {
        List<Transform> chosenPoints = new List<Transform>();
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            chosenPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex);  // Ayn� spawn point'in tekrar se�ilmemesi i�in ��kar�yoruz.
        }

        return chosenPoints;
    }

    // Rastgele iki tane yetenek prefab'� se�er
    private List<GameObject> ChooseRandomAbilities(int count)
    {
        List<GameObject> chosenAbilities = new List<GameObject>();
        List<GameObject> availableAbilities = new List<GameObject>(abilities);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableAbilities.Count);
            chosenAbilities.Add(availableAbilities[randomIndex]);
            availableAbilities.RemoveAt(randomIndex);  // Ayn� yetene�in tekrar se�ilmemesi i�in ��kar�yoruz.
        }

        return chosenAbilities;
    }
}
