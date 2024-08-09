using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafMovement : MonoBehaviour
{
    public float windStrength = 0.5f;  // R�zgar�n �iddeti
    public float windSpeed = 1.0f;     // R�zgar�n h�z�
    public Vector3 windDirection = new Vector3(1, 0, 0); // R�zgar�n y�n� (x ve z y�n�nde)

    private MeshFilter meshFilter;
    private Vector3[] originalVertices;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            originalVertices = meshFilter.mesh.vertices;
        }
    }

    void Update()
    {
        if (meshFilter != null)
        {
            Mesh mesh = meshFilter.mesh;
            Vector3[] vertices = new Vector3[originalVertices.Length];
            originalVertices.CopyTo(vertices, 0);

            float time = Time.time * windSpeed;

            for (int i = 0; i < vertices.Length; i++)
            {
                // R�zgar�n etkisini yatay d�zlemde hesaplay�n
                float windEffect = Mathf.Sin(Vector3.Dot(vertices[i], windDirection) + time) * windStrength;

                // Yaln�zca x ve z eksenlerindeki hareketi hesaplay�n
                vertices[i].x += windEffect * windDirection.x;
                vertices[i].z += windEffect * windDirection.z;
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
        }
    }
}
