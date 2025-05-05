using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 5.0f;
    private bool m_IsMoving;
    private Vector3 m_MoveTarget;
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private bool m_IsGameOver;
    private Animator m_Animator;
    public Vector2Int Cell => m_CellPosition;
    private SpriteRenderer m_SpriteRenderer;
    public AudioSource PunchSound;

    public void Init()
    {
        m_IsGameOver = false;
        m_IsMoving = false;
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        m_CellPosition = cell;

        transform.position = m_Board.CellToWorld(cell);
    }

    public void MoveTo(Vector2Int cell)
    {
        m_CellPosition = cell;

        m_IsMoving = true;
        m_Animator.SetBool("Moving", m_IsMoving);
        m_MoveTarget = m_Board.CellToWorld(m_CellPosition);
    }


    private void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }
            return;
        }

        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);

            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
                m_Animator.SetBool("Moving", false);
                var cellData = m_Board.GetCellData(m_CellPosition);
                if (cellData.ContainedObject != null)
                    cellData.ContainedObject.PlayerEntered();
            }

            return;
        }

        Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
        {
            newCellTarget.y += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            newCellTarget.y -= 1;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
        {
            newCellTarget.x += 1;
            hasMoved = true;
            if (m_SpriteRenderer != null) m_SpriteRenderer.flipX = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
        {
            newCellTarget.x -= 1;
            hasMoved = true;
            if (m_SpriteRenderer != null) m_SpriteRenderer.flipX = false;
        }

        if (hasMoved)
        {
            var cellData = m_Board.GetCellData(newCellTarget);

            if (cellData != null && cellData.Passable)
            {
                GameManager.Instance.TurnManager.Tick();

                if (cellData.ContainedObject == null || cellData.ContainedObject.PlayerWantsToEnter())
                {
                    MoveTo(newCellTarget);
                }
                else
                {
                    if (PunchSound != null && !PunchSound.isPlaying)
                    {
                        AudioSource.PlayClipAtPoint(PunchSound.clip, transform.position);
                    }
                    m_Animator.SetTrigger("Attack");
                }
            }
        }
    }


    public void GameOver()
    {
        m_IsGameOver = true;
    }

}

