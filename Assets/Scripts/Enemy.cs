using UnityEngine;
using System.Linq;

public class Enemy : CellObject
{
    public int Health = 3;
    public float MoveSpeed = 5.0f;
    private bool m_IsMoving;
    private Animator m_Animator;
    private int m_CurrentHealth;
    private Vector3 m_MoveTarget;
    public int DetectionRadius = 3;

    private void Awake()
    {
        GameManager.Instance.TurnManager.OnTick += TurnHappened;
        m_Animator = GetComponent<Animator>();
    }

    private void OnDestroy()
    {
        GameManager.Instance.TurnManager.OnTick -= TurnHappened;
    }

    private void Update()
    {
        m_Animator.SetBool("Moving", m_IsMoving);
        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);

            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        m_CurrentHealth -= amount;

        if (m_CurrentHealth <= 0)
        {
            DropLoot();
            Destroy(gameObject);
        }
    }

    void DropLoot()
    {
        var board = GameManager.Instance.BoardManager;

        if (Random.value < 0.5f && board.FoodLootPrefabs.Length > 0)
        {
            int index = Random.Range(0, board.FoodLootPrefabs.Length);
            var food = GameObject.Instantiate(board.FoodLootPrefabs[index]);
            board.AddObject(food, m_Cell);
        }
        else if (Random.value < 0.3f && board.AmmoLootPrefabs.Length > 0)
        {
            int index = Random.Range(0, board.AmmoLootPrefabs.Length);
            var ammo = GameObject.Instantiate(board.AmmoLootPrefabs[index]);
            board.AddObject(ammo, m_Cell);
        }
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = Health;
    }

    public override bool PlayerWantsToEnter()
    {
        TakeDamage(1);
        return false;
    }

    bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null || !targetCell.Passable || targetCell.ContainedObject != null)
        {
            return false;
        }

        int directionX = coord.x - m_Cell.x;
        if (directionX != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Sign(directionX) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // Remove from current cell
        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        // Assign to new cell
        targetCell.ContainedObject = this;
        m_Cell = coord;

        // Begin moving
        m_MoveTarget = board.CellToWorld(coord);
        m_IsMoving = true;

        return true;
    }

    void TurnHappened()
    {
        m_Animator.SetBool("Attack", false);
        var playerCell = GameManager.Instance.PlayerController.Cell;

        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        if (absXDist + absYDist <= DetectionRadius)
        {
            if ((xDist == 0 && absYDist == 1) || (yDist == 0 && absXDist == 1))
            {
                GameManager.Instance.ChangeFood(-3);
                m_Animator.SetBool("Attack", true);
            }
            else
            {
                if (absXDist > absYDist)
                {
                    if (!TryMoveInX(xDist))
                        TryMoveInY(yDist);
                }
                else
                {
                    if (!TryMoveInY(yDist))
                        TryMoveInX(xDist);
                }
            }
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        directions = directions.OrderBy(x => Random.value).ToArray();

        foreach (var dir in directions)
        {
            if (MoveTo(m_Cell + dir))
            {
                break;
            }
        }
    }

    bool TryMoveInX(int xDist)
    {
        //player to our right
        if (xDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.right);
        }

        //player to our left
        return MoveTo(m_Cell + Vector2Int.left);
    }

    bool TryMoveInY(int yDist)
    {
        //player on top
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }
        //player below
        return MoveTo(m_Cell + Vector2Int.down);
    }
}