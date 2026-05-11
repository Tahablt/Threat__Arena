using UnityEngine;

public class XPMagnet : MonoBehaviour
{
    public float magnetSpeed = 5f;
    private Transform playerTransform;
    private bool isFollowing = false;
    private float startFollowDistance = 4f; 

    private float spawnTime;
    public float magnetDelay = 0.4f;

    private void OnEnable()
    {
        isFollowing = false;
        spawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time < spawnTime + magnetDelay) return;
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            if (playerTransform == null) return;
        }

        if (!isFollowing)
        {
            Vector3 diff = transform.position - playerTransform.position;
            diff.y = 0; // Yukseklik farkini goz ardi et! Silindirik (2D benzeri) alan olusturur.

            if (diff.sqrMagnitude < startFollowDistance * startFollowDistance)
            {
                isFollowing = true;
            }
        }

        if (isFollowing)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
        }
    }
}
