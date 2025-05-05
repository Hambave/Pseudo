using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public TurnManager TurnManager { get; private set; }
    private int m_FoodAmount = 10;
    public UIDocument UIDoc;
    private Label m_FoodLabel;
    private Label m_LevelLabel;
    private int m_CurrentLevel = 1;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;
    private int m_AmmoAmount = 5;
    private Label m_AmmoLabel;
    private VisualElement m_PausePanel;
    private Button m_ContinueButton;
    private Button m_PauseQuitToMenuButton;
    private Button m_DeadQuitToMenuButton;
    private Button m_RestartGameButton;
    private Button m_DeadRestartGameButton;
    private bool m_IsPaused;
    private bool m_IsGameOver = false;
    public PlayerRangedAttack PlayerRangedAttack;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_LevelLabel = UIDoc.rootVisualElement.Q<Label>("LevelLabel");
        m_AmmoLabel = UIDoc.rootVisualElement.Q<Label>("AmmoLabel");

        m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");
        m_DeadQuitToMenuButton = m_GameOverPanel.Q<Button>("DeadQuitButton");
        m_DeadRestartGameButton = m_GameOverPanel.Q<Button>("DeadRestartButton");

        m_PausePanel = UIDoc.rootVisualElement.Q<VisualElement>("PausePanel");
        m_ContinueButton = m_PausePanel.Q<Button>("ContinueButton");
        m_PauseQuitToMenuButton = m_PausePanel.Q<Button>("PauseQuitButton");
        m_RestartGameButton = m_PausePanel.Q<Button>("RestartButton");

        m_PausePanel.style.display = DisplayStyle.None;

        m_ContinueButton.clicked += () => TogglePause(false);
        m_PauseQuitToMenuButton.clicked += () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        };

        m_DeadQuitToMenuButton.clicked += () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        };

        m_RestartGameButton.clicked += () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Scene");
        };

        m_DeadRestartGameButton.clicked += () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Scene");
        };

        UpdateAmmoUI();
        StartNewGame();
    }

    void Update()
    {
        if (!m_IsGameOver && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause(!m_IsPaused);
        }
    }

    void TogglePause(bool pause)
    {
        m_IsPaused = pause;
        Time.timeScale = pause ? 0f : 1f;
        m_PausePanel.style.display = pause ? DisplayStyle.Flex : DisplayStyle.None;
        PlayerRangedAttack.SetPaused(pause);
    }

    public void ChangeAmmo(int amount)
    {
        m_AmmoAmount += amount;
        m_AmmoAmount = Mathf.Max(0, m_AmmoAmount);
        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (m_AmmoLabel != null)
            m_AmmoLabel.text = "Ammo: " + m_AmmoAmount;
    }

    public bool HasAmmo() => m_AmmoAmount > 0;
    public void UseAmmo() => ChangeAmmo(-1);

    public void StartNewGame()
    {
        m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_CurrentLevel = 1;
        m_FoodAmount = 100;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        m_LevelLabel.text = "Level: " + m_CurrentLevel;

        LevelConfig config = new LevelConfig(m_CurrentLevel);
        BoardManager.Clean();
        BoardManager.Init(config);

        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
    }

    public void NewLevel()
    {
        m_CurrentLevel++;

        m_LevelLabel.text = "Level: " + m_CurrentLevel;

        LevelConfig config = new LevelConfig(m_CurrentLevel);
        BoardManager.Clean();
        BoardManager.Init(config);
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
    }


    void OnTurnHappen()
    {
        ChangeFood(-1);
    }

    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        if (m_FoodAmount <= 0)
        {
            PlayerController.GameOver();
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = "Game Over!\n\nSurvived " + m_CurrentLevel + " levels";
            m_IsGameOver = true;
        }
    }
}
