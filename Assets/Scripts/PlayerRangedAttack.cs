using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRangedAttack : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public float MaxProjectileDistance = 10f;
    private bool m_IsPaused;
    private Animator m_Animator;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !m_IsPaused)
        {
            ShootTowardMouse();
        }
    }

    public void SetPaused(bool paused)
    {
        m_IsPaused = paused;
    }

    void ShootTowardMouse()
    {
        if (!GameManager.Instance.HasAmmo())
            return;

        if (m_Animator != null)
        {
            m_Animator.SetTrigger("Attack");
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0;

        Vector3 shootDirection = mouseWorldPos - transform.position;

        GameObject projectileGO = Instantiate(ProjectilePrefab, transform.position, Quaternion.identity);
        Projectile proj = projectileGO.GetComponent<Projectile>();
        proj.MaxDistance = MaxProjectileDistance;
        proj.Init(shootDirection);

        GameManager.Instance.UseAmmo();
    }
}