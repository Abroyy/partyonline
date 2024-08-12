using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public ParticleSystem particleSystem;

    void Awake()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop(); // Ba�lang��ta partik�l sistemini durdurur
        }
    }
}
