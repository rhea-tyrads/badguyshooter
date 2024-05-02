using UnityEngine;

public class FirebaseSpawner : MonoBehaviour
{
    public FirebaseEvents prefab;

    void Start()
    {
        Invoke(nameof(Spawn), 0.05f);
    }

    void Spawn()
    {
        var fire = FindObjectOfType<FirebaseAdsRevenue>();
        if (!fire)
            Instantiate(prefab);

    }

}
