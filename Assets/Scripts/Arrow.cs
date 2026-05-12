using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Transform target;
    private float damage;

    [Header("Ok Ayarları")]
    public float speed; // Bu değer artık Unity Inspector yerine BowSystem'den atanacak
    public float lifeTime = 5f; // Ok bir yere çarpamazsa 5 saniye sonra yok olsun

    float counter;

    void Start()
    {
        // Belleği şişirmemek için güvenlik önlemi
        //Destroy(gameObject, lifeTime);
    }

    // BowSystem'den gelen hedef, hasar ve HIZ bilgisi
    public void Seek(Transform _target, float _damage, float _speed)
    {
        target = _target;
        damage = _damage;
        speed = _speed; // BowSystem'den gelen hızı okun hızına eşitliyoruz

        // Ok doğduğu an hedefe doğru dönsün
        if (target != null)
        {
            transform.LookAt(target);
        }
    }

    void Update()
    {
        // Hedef daha ok havadayken öldüyse oku yok et
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

        // Hedefe doğru ilerleme
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        transform.LookAt(target);

        // Hedefe yeterince yaklaştık mı? (0.2f mesafe çarpışma sayılır)
        if (Vector3.Distance(transform.position, target.position) <= 0.2f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        // 1. Düşmanın üzerindeki "Enemy" scriptini buluyoruz
        Enemy enemy = target.GetComponent<Enemy>();

        if (enemy != null)
        {
            // 2. Enemy scriptindeki TakeDamage fonksiyonunu çalıştırıp canını azaltıyoruz!
            enemy.TakeDamage(damage);

            Debug.Log("<color=green>Ok isabet etti!</color> Vurulan: " + target.name + " | Hasar: " + damage);
        }
        else
        {
            Debug.LogWarning("<color=orange>Ok çarptı ama " + target.name + " objesinde 'Enemy' scripti BULUNAMADI!</color>");
        }

        // Çarptıktan sonra oku yokediyoruz
        Destroy(gameObject);
    }
}