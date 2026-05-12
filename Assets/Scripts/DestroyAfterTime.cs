using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float destroyTime = 0.5f; // Efekt yarım saniye sonra yok olsun

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}