using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SocketIOClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class MatchmakingManager : MonoBehaviour
{
    [Header("UI Referencias")]
    public Button battleButton;
    public Button cancelQueueButton;
    public Text statusText;
    public Text debugText;
    public GameObject loadingPanel;
    public GameObject errorPanel;
    public Text errorMessageText;

    [Header("Configuración")]
    public string serverURL = "http://localhost:3000";
    public string gameSceneName = "GameScene";
    public string gameMode = "casual"; // casual, ranked

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Socket.IO
    private SocketIOUnity socket;

    // Estado
    private bool isConnected = false;
    private bool isInQueue = false;
    private bool isConnecting = false;
    private string currentGameId = null;
    private string jwtToken = null;

    // Información del deck activo
    private DeckInfo activeDeck = null;

    [System.Serializable]
    public class DeckInfo
    {
        public string _id;
        public string name;
        public int totalCards;
        public bool isComplete;
    }

    [System.Serializable]
    public class GameStartData
    {
        public string gameId;
        public string firstPlayer;
        public List<PlayerInfo> players;
    }

    [System.Serializable]
    public class PlayerInfo
    {
        public string playerId;
        public string username;
    }

    void Start()
    {
        InitializeUI();

        // Obtener JWT token del sistema de autenticación
        jwtToken = AuthManager.Instance?.GetJWT();

        if (string.IsNullOrEmpty(jwtToken))
        {
            ShowError("Error: No hay token de autenticación. Inicia sesión primero.");
            return;
        }

        ConnectToServer();
    }

    void InitializeUI()
    {
        // Configurar botones
        if (battleButton) battleButton.onClick.AddListener(OnBattleButtonClicked);
        if (cancelQueueButton)
        {
            cancelQueueButton.onClick.AddListener(OnCancelQueueClicked);
            cancelQueueButton.gameObject.SetActive(false);
        }

        // Estado inicial
        UpdateStatus("Conectando al servidor...");
        if (battleButton) battleButton.interactable = false;
        if (loadingPanel) loadingPanel.SetActive(true);
    }

    void ConnectToServer()
    {
        if (isConnecting) return;

        isConnecting = true;
        DebugLog("🔌 Conectando al servidor: " + serverURL);

        try
        {
            // Crear socket con autenticación
            var uri = new Uri(serverURL);
            socket = new SocketIOUnity(uri, new SocketIOOptions
            {
                Query = new Dictionary<string, string>(),
                ExtraHeaders = new Dictionary<string, string>(),
                Auth = new Dictionary<string, string>
                {
                    { "token", jwtToken }
                },
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            });

            // Configurar event listeners
            SetupSocketEvents();

            // Conectar
            socket.Connect();

        }
        catch (Exception ex)
        {
            DebugLog("❌ Error creando socket: " + ex.Message);
            ShowError("Error de conexión: " + ex.Message);
            isConnecting = false;
        }
    }

    void SetupSocketEvents()
    {
        // === EVENTOS DE CONEXIÓN ===
        socket.OnConnected += (sender, e) =>
        {
            DebugLog("✅ Conectado al servidor");
            isConnected = true;
            isConnecting = false;

            // El servidor enviará connection_established automáticamente
        };

        socket.OnDisconnected += (sender, e) =>
        {
            DebugLog("🔌 Desconectado del servidor: " + e);
            isConnected = false;
            isInQueue = false;
            UpdateStatus("Desconectado del servidor");
            if (battleButton) battleButton.interactable = false;
            if (loadingPanel) loadingPanel.SetActive(false);
        };

        socket.OnError += (sender, e) =>
        {
            DebugLog("❌ Error de socket: " + e);
            ShowError("Error de conexión: " + e);
            isConnecting = false;
        };

        // === EVENTOS DE AUTENTICACIÓN ===
        socket.On("connection_established", (response) =>
        {
            DebugLog("🔑 Autenticación exitosa");
            var data = response.GetValue<JObject>();
            DebugLog($"Usuario autenticado: {data["userId"]}");

            UpdateStatus("Conectado - Verificando deck...");

            // Verificar deck activo automáticamente
            CheckActiveDeck();
        });

        // === EVENTOS DE MATCHMAKING ===
        socket.On("queue_joined", (response) =>
        {
            var data = response.GetValue<JObject>();
            isInQueue = true;

            var position = data["position"]?.Value<int>() ?? 0;
            var gameMode = data["gameMode"]?.Value<string>() ?? "casual";

            // Información del deck activo
            if (data["activeDeck"] != null)
            {
                activeDeck = JsonConvert.DeserializeObject<DeckInfo>(data["activeDeck"].ToString());
                DebugLog($"🎴 Usando deck: {activeDeck.name} ({activeDeck.totalCards} cartas)");
            }

            UpdateStatus($"En cola de espera (Posición: {position})");
            DebugLog($"🎯 En cola - Posición: {position}, Modo: {gameMode}");

            // Actualizar UI
            if (battleButton) battleButton.gameObject.SetActive(false);
            if (cancelQueueButton) cancelQueueButton.gameObject.SetActive(true);
            if (loadingPanel) loadingPanel.SetActive(true);
        });

        socket.On("queue_left", (response) =>
        {
            isInQueue = false;
            UpdateStatus("Fuera de la cola");
            DebugLog("🚪 Saliste de la cola");

            // Restaurar UI
            if (battleButton) battleButton.gameObject.SetActive(true);
            if (cancelQueueButton) cancelQueueButton.gameObject.SetActive(false);
            if (loadingPanel) loadingPanel.SetActive(false);
        });

        socket.On("queue_error", (response) =>
        {
            var data = response.GetValue<JObject>();
            var message = data["message"]?.Value<string>() ?? "Error desconocido";
            var code = data["code"]?.Value<string>();

            DebugLog($"❌ Error de cola: {message}");

            if (code == "NO_ACTIVE_DECK")
            {
                ShowError("No tienes un deck activo configurado.\n\nVe a 'Decks' y activa un deck completo (40 cartas) para poder jugar.");
            }
            else
            {
                ShowError("Error de matchmaking: " + message);
            }

            isInQueue = false;
            if (battleButton) battleButton.gameObject.SetActive(true);
            if (cancelQueueButton) cancelQueueButton.gameObject.SetActive(false);
            if (loadingPanel) loadingPanel.SetActive(false);
        });

        // === EVENTOS DE JUEGO ===
        socket.On("game_started", (response) =>
        {
            var data = response.GetValue<JObject>();
            var gameData = JsonConvert.DeserializeObject<GameStartData>(data.ToString());

            currentGameId = gameData.gameId;

            DebugLog($"🎉 ¡Partida iniciada! ID: {currentGameId}");
            DebugLog($"👤 Primer jugador: {gameData.firstPlayer}");

            UpdateStatus("¡Partida encontrada! Cargando...");

            // Guardar información de la partida para la próxima escena
            GameSession.Instance?.SetGameData(currentGameId, gameData.players);

            // Cambiar a escena de juego
            StartCoroutine(LoadGameScene());
        });

        socket.On("opponent_connected", (response) =>
        {
            DebugLog("👥 Tu oponente se conectó");
        });

        socket.On("opponent_disconnected", (response) =>
        {
            DebugLog("👤 Tu oponente se desconectó");
            ShowError("Tu oponente se desconectó de la partida");
        });

        // === EVENTOS DE ERROR ===
        socket.On("game_error", (response) =>
        {
            var data = response.GetValue<JObject>();
            var message = data["message"]?.Value<string>() ?? "Error de juego";

            DebugLog($"❌ Error de juego: {message}");
            ShowError("Error de juego: " + message);
        });
    }

    void CheckActiveDeck()
    {
        // Simular verificación de deck activo
        // En tu implementación real, esto podría ser una llamada HTTP o Socket event
        DebugLog("🔍 Verificando deck activo...");

        // Por simplicidad, asumimos que el deck está OK
        // El servidor verificará esto cuando se una a la cola

        UpdateStatus("Listo para batallar");
        if (battleButton) battleButton.interactable = true;
        if (loadingPanel) loadingPanel.SetActive(false);
    }

    // === MÉTODOS DE UI ===

    public void OnBattleButtonClicked()
    {
        if (!isConnected)
        {
            ShowError("No estás conectado al servidor");
            return;
        }

        if (isInQueue)
        {
            DebugLog("⚠️ Ya estás en cola");
            return;
        }

        DebugLog($"🎯 Uniéndose a cola (modo: {gameMode})...");

        // Enviar evento para unirse a la cola
        var queueData = new Dictionary<string, object>
        {
            { "gameMode", gameMode }
        };

        socket.Emit("join_queue", queueData);

        UpdateStatus("Buscando oponente...");
    }

    public void OnCancelQueueClicked()
    {
        if (!isInQueue)
        {
            DebugLog("⚠️ No estás en cola");
            return;
        }

        DebugLog("🚪 Saliendo de la cola...");
        socket.Emit("leave_queue");
    }

    void UpdateStatus(string message)
    {
        if (statusText) statusText.text = message;
        DebugLog($"📊 Estado: {message}");
    }

    void ShowError(string message)
    {
        DebugLog($"❌ Error: {message}");

        if (errorPanel && errorMessageText)
        {
            errorMessageText.text = message;
            errorPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Error de Matchmaking: " + message);
        }
    }

    public void CloseErrorPanel()
    {
        if (errorPanel) errorPanel.SetActive(false);
    }

    void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[Matchmaking] {message}");
        }

        if (debugText)
        {
            debugText.text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";

            // Limitar líneas de debug
            var lines = debugText.text.Split('\n');
            if (lines.Length > 20)
            {
                var recentLines = new string[15];
                Array.Copy(lines, lines.Length - 15, recentLines, 0, 15);
                debugText.text = string.Join("\n", recentLines);
            }
        }
    }

    IEnumerator LoadGameScene()
    {
        yield return new WaitForSeconds(1f); // Pequeña pausa para mostrar mensaje

        DebugLog($"🎮 Cargando escena de juego: {gameSceneName}");

        // Mantener conexión para la próxima escena
        if (socket != null)
        {
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.LoadScene(gameSceneName);
    }

    // === MÉTODOS PÚBLICOS PARA OTRAS ESCENAS ===

    public SocketIOUnity GetSocket()
    {
        return socket;
    }

    public string GetCurrentGameId()
    {
        return currentGameId;
    }

    public bool IsConnected()
    {
        return isConnected && socket != null && socket.Connected;
    }

    // === CLEANUP ===

    void OnDestroy()
    {
        if (socket != null)
        {
            DebugLog("🔌 Desconectando socket...");
            socket.Disconnect();
            socket.Dispose();
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && socket != null && socket.Connected)
        {
            DebugLog("⏸️ Aplicación pausada - manteniendo conexión");
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && socket != null && socket.Connected)
        {
            DebugLog("👁️ Aplicación perdió foco - manteniendo conexión");
        }
    }
}

// === CLASES DE APOYO ===

// Singleton para mantener datos de sesión entre escenas
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [System.Serializable]
    public class GameSessionData
    {
        public string gameId;
        public List<MatchmakingManager.PlayerInfo> players;
        public DateTime startTime;
    }

    private GameSessionData currentSession;

    void Awake()
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

    public void SetGameData(string gameId, List<MatchmakingManager.PlayerInfo> players)
    {
        currentSession = new GameSessionData
        {
            gameId = gameId,
            players = players,
            startTime = DateTime.Now
        };

        Debug.Log($"[GameSession] Sesión creada: {gameId}");
    }

    public GameSessionData GetCurrentSession()
    {
        return currentSession;
    }

    public void ClearSession()
    {
        currentSession = null;
        Debug.Log("[GameSession] Sesión limpiada");
    }
}

// Placeholder para el sistema de autenticación
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [SerializeField] private string jwtToken = ""; // Configurar en Inspector para testing

    void Awake()
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

    public string GetJWT()
    {
        // En tu implementación real, esto vendría del login
        // Por ahora retorna el token configurado en el Inspector
        return jwtToken;
    }

    public void SetJWT(string token)
    {
        jwtToken = token;
    }
}   