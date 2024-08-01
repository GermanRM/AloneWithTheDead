using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public GameState gameState;

    [Header("Scene Changer")]
    [SerializeField] private string lobbyScene;
    [SerializeField] private string gameScene;

    [Header("GameLoop Properties")]
    [SerializeField] private float timeToComplete;
    [SerializeField] private GameObject gameLoopCanvas;
    [SerializeField] private GameLoopManager gameLoopManager;
    public float zombiesKilled;
    public float zombiesKilledGoal;

    [Header("Player Properties")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerRef;
    [SerializeField] private Vector2 playerSens;
    [SerializeField] private float musicLevel;
    [SerializeField] private float soundLevel;
    bool playerSpawned = false;

    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (gameState == GameState.Paused) { Cursor.lockState = CursorLockMode.None; }

        if (gameState == GameState.Playing)
        {
            CheckDeath();
            CheckWin();
        }
    }

    #region Player

    public GameObject GetPlayerRef() { return playerRef; }

    private void SpawnPlayer(Transform spawnPoint)
    {
        GameObject go = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        playerRef = go;
    }

    public void SetSensX(float x)
    {
        playerSens.x = x;
    }

    public void SetSensY(float y)
    {
        playerSens.y = y;
    }

    #endregion

    #region Game Manager

    private void CheckDeath()
    {
        if (playerRef != null)
        {
            PlayerStats stats = playerRef.GetComponent<PlayerStats>();

            if (stats.isDeath)
            {
                gameLoopManager.EnableDisableGameLoopHolder(false);
                gameState = GameState.Paused;
                playerSpawned = false;
                gameLoopManager.SetGameOverText("Game Over");
                gameLoopManager.EnableDisableGameOverHolder(true);
            }
        }
    }

    private void CheckWin()
    {
        if (zombiesKilled >= zombiesKilledGoal)
        {
            gameLoopManager.EnableDisableGameLoopHolder(false);
            gameState = GameState.Paused;
            playerSpawned = false;
            gameLoopManager.SetGameOverText("You win!!"); gameLoopManager.EnableDisableGameOverHolder(true);
        }
    }

    public void AddZombieKilled(float zombieKilled)
    {
        this.zombiesKilled += zombieKilled;
    }

    #endregion

    #region Scenes

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameScene)
        {
            gameState = GameState.Playing;
            Transform spawnPoint = GameObject.Find("SpawnPoint").transform;
            
            if (!playerSpawned)
            {
                GameObject go = Instantiate(gameLoopCanvas);
                gameLoopManager = go.GetComponent<GameLoopManager>();
                gameLoopManager.EnableDisableGameLoopHolder(true);
                playerSpawned = true;
                SpawnPlayer(spawnPoint);
            }
        }
        else if (scene.name == lobbyScene)
        {
            gameState = GameState.Lobby;
            playerSpawned = false;
            gameLoopManager = null;
            zombiesKilled = 0;
        }
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    #endregion

    #region Options Menu

    public void ChangeSensX(float sens)
    {
        playerSens.x = sens;
    }

    public void ChangeSensY(float sens)
    {
        playerSens.y = sens;
    }

    public void ChangeMusicLevel(float level)
    {
        musicLevel = level;
    }

    public void ChangeSoundsLevel(float level)
    {
        soundLevel = level;
    }

    #endregion
}