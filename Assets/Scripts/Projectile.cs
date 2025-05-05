using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Speed = 10f;
    public int Damage = 1;
    public float MaxDistance = 10f;
    public AudioSource HitSound;

    private Vector3 m_StartPosition;
    private Vector3 m_Direction;

    public void Init(Vector3 direction)
    {
        m_StartPosition = transform.position;
        m_Direction = direction.normalized;

        float angle = Mathf.Atan2(m_Direction.y, m_Direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);
    }


    void Update()
    {
        transform.position += m_Direction * Speed * Time.deltaTime;

        float distanceTraveled = Vector3.Distance(m_StartPosition, transform.position);
        if (distanceTraveled >= MaxDistance)
        {
            Destroy(gameObject);
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.1f); foreach (var hit in hits)
        {
            if (hit == null) continue;

            var enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (HitSound != null)
                {
                    AudioSource.PlayClipAtPoint(HitSound.clip, transform.position);
                }

                enemy.TakeDamage(Damage);
                Destroy(gameObject);
                return;
            }

            var wall = hit.GetComponent<WallObject>();
            if (wall != null)
            {
                if (HitSound != null)
                {
                    AudioSource.PlayClipAtPoint(HitSound.clip, transform.position);
                }

                Destroy(gameObject);
                return;
            }
        }
    }
}