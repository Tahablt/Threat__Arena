using UnityEngine;

public class XPDrop : MonoBehaviour
{
    public float xpAmount = 20f;
    public float pickupDistance = 1.5f; // Yari capi biraz daha buyutelim
    
    private bool isCollected = false;
    private Transform playerTransform;
    private PlayerXP playerXP;

    private float spawnTime;
    public float collectDelay = 0.6f;

    private void OnEnable()
    {
        isCollected = false;
        spawnTime = Time.time;
    }

    private void Update()
    {
        if (isCollected) return;
        if (Time.time < spawnTime + collectDelay) return;

        if (playerTransform == null || playerXP == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerXP = playerObj.GetComponentInChildren<PlayerXP>();
            }
            if (playerTransform == null || playerXP == null) return;
        }

        // X ve Z duzlemindeki gercek mesafeye bak (Y eksenindeki yukseklik farki hataya sebep olmasin!)
        Vector3 diff = transform.position - playerTransform.position;
        diff.y = 0; // Yuksekligi sifirla, silindir seklinde bir mesafe ölcumu!

        float distanceSqr = diff.sqrMagnitude;

        if (distanceSqr <= pickupDistance * pickupDistance)
        {
            isCollected = true;
            playerXP.AddXP(xpAmount);
            
            if (XPPool.Instance != null)
            {
                XPPool.Instance.ReturnXP(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
