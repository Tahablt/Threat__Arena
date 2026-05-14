using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed;
    public float lifeTime = 5f;
    private float counter;

    public void Seek(Transform _target, float _damage, float _speed)
    {
        target = _target;
        damage = _damage;
        speed = _speed;
        counter = 0f;

        // Yan yatmasın diye LookAt komutunu buradan tamamen sildik!
    }

    void Update()
    {
        // Hedef yoksa yok et
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        counter += Time.deltaTime;
        if (counter > lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // --- GÖRÜNTÜ DÜZELTMESİ 1: Yerde Sürünmeyi Engelle ---
        // Düşmanın ayaklarına (0) değil, gövdesine (0.5f) doğru uçsun
        Vector3 targetPos = new Vector3(target.position.x, 0.5f, target.position.z);

        // Oraya doğru hareket et
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        // --- GÖRÜNTÜ DÜZELTMESİ 2: Havalı Dönüş ---
        // Bomba yuvarlak olduğu için fırlatılırken kendi etrafında döne döne gitsin (Görsel şölen!)
        // transform.Rotate(0, 360 * Time.deltaTime, 0);

        // Hedefe ulaştı mı?
        if (Vector3.Distance(transform.position, targetPos) <= 0.2f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy == null) enemy = target.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}