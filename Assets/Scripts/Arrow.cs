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
        counter = 0f; // Havuzdan çıkınca zamanlayıcıyı sıfırla

        if (target != null) transform.LookAt(target);
    }

    void Update()
    {
        // Hedef yoksa veya süre dolduysa havuza dön
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            ArrowPool.Instance.ReturnArrow(gameObject);
            return;
        }

        counter += Time.deltaTime;
        if (counter > lifeTime)
        {
            ArrowPool.Instance.ReturnArrow(gameObject);
            return;
        }

        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        transform.LookAt(target);

        if (Vector3.Distance(transform.position, target.position) <= 0.2f)
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

        // Çarptıktan sonra havuza geri gönder
        ArrowPool.Instance.ReturnArrow(gameObject);
    }
}