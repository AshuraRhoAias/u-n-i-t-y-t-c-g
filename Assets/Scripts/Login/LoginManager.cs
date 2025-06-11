using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoginManager : MonoBehaviour
{
    [Header("Campos de Entrada")]
    public TMP_InputField usernameField; // En realidad será email
    public TMP_InputField passwordField;

    [Header("Configuración de Envío")]
    public Button submitButton;
    public string apiUrl = "http://localhost:3000/auth/login"; // ✅ Corregida la URL

    [Header("Configuración de Escena")]
    public TMP_Dropdown targetSceneDropdown;

    [Header("UI Feedback")]
    public TMP_Text statusText; // Para mostrar mensajes al usuario
    public GameObject loadingPanel; // Panel de carga opcional

    [System.Serializable]
    public class SceneOption
    {
        public string sceneName;

#if UNITY_EDITOR
        public Object sceneAsset; // Referencia directa a la escena (solo en editor)
#endif
    }

    // ✅ Clases para deserializar la respuesta JSON corregidas
    [System.Serializable]
    public class LoginResponse
    {
        public bool success;
        public string message;
        public string token;
        public UserData user;
    }

    [System.Serializable]
    public class UserData
    {
        public string id; // MongoDB usa string para _id
        public string username;
        public string email;
        public string avatar_url;
        public int level;
        public int experience;
        public int gold;
        public int gems;
        public string created_at;
        public string updated_at;
    }

    public SceneOption[] availableScenes;

    private string targetSceneName = "";

    private void Start()
    {
        // Configurar el botón
        submitButton.onClick.AddListener(EnviarDatos);

        // Inicializar el dropdown con las escenas disponibles
        if (targetSceneDropdown != null && availableScenes != null && availableScenes.Length > 0)
        {
            targetSceneDropdown.ClearOptions();

            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (SceneOption scene in availableScenes)
            {
                options.Add(new TMP_Dropdown.OptionData(scene.sceneName));
            }

            targetSceneDropdown.AddOptions(options);
            targetSceneDropdown.onValueChanged.AddListener(OnSceneDropdownValueChanged);

            // Establecer la escena inicial
            targetSceneName = availableScenes[0].sceneName;
        }
        else
        {
            // Escena por defecto si no hay dropdown configurado
            targetSceneName = "SampleScene";
        }

        // Inicializar UI
        UpdateStatusText("");
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    private void OnSceneDropdownValueChanged(int dropdownIndex)
    {
        if (dropdownIndex >= 0 && dropdownIndex < availableScenes.Length)
        {
            targetSceneName = availableScenes[dropdownIndex].sceneName;
            Debug.Log("Escena de destino cambiada a: " + targetSceneName);
        }
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    public void EnviarDatos()
    {
        // Validación de campos
        if (string.IsNullOrEmpty(usernameField.text) || string.IsNullOrEmpty(passwordField.text))
        {
            UpdateStatusText("Por favor completa todos los campos");
            return;
        }

        Debug.Log("Enviando datos de login...");
        UpdateStatusText("Iniciando sesión...");

        if (loadingPanel != null) loadingPanel.SetActive(true);

        StartCoroutine(EnviarDatosCoroutine());
    }

    IEnumerator EnviarDatosCoroutine()
    {
        string email = usernameField.text;
        string password = passwordField.text;

        // ✅ CORREGIDO: Enviar email en el campo correcto
        string jsonData = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\"}";

        Debug.Log("JSON enviado: " + jsonData);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        // Ocultar panel de carga
        if (loadingPanel != null) loadingPanel.SetActive(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Respuesta del servidor: " + request.downloadHandler.text);

            try
            {
                // ✅ MEJORADO: Parsear respuesta JSON correctamente
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

                if (response != null && response.success)
                {
                    Debug.Log("Login exitoso. Usuario: " + response.user.username);
                    UpdateStatusText("¡Login exitoso! Cargando juego...");

                    // Guardar datos del usuario
                    if (!string.IsNullOrEmpty(response.token))
                    {
                        PlayerPrefs.SetString("AuthToken", response.token);
                        PlayerPrefs.SetString("UserId", response.user.id);
                        PlayerPrefs.SetString("Username", response.user.username);
                        PlayerPrefs.SetString("UserEmail", response.user.email);
                        PlayerPrefs.SetInt("UserLevel", response.user.level);
                        PlayerPrefs.SetInt("UserExperience", response.user.experience);
                        PlayerPrefs.SetInt("UserGold", response.user.gold);
                        PlayerPrefs.SetInt("UserGems", response.user.gems);
                        PlayerPrefs.Save();
                    }

                    // Verificar si la escena existe en el Build Settings
                    if (SceneExistsInBuildSettings(targetSceneName))
                    {
                        Debug.Log("Cargando escena: " + targetSceneName);
                        SceneManager.LoadScene(targetSceneName);
                    }
                    else
                    {
                        Debug.LogError("Error: La escena '" + targetSceneName + "' no existe en el Build Settings.");
                        UpdateStatusText("Error: Escena no encontrada");
                    }
                }
                else
                {
                    string errorMessage = response != null ? response.message : "Error desconocido";
                    Debug.LogWarning("Login fallido: " + errorMessage);
                    UpdateStatusText("Error: " + errorMessage);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parseando respuesta JSON: " + e.Message);
                UpdateStatusText("Error procesando respuesta del servidor");
            }
        }
        else
        {
            Debug.LogError("Error de conexión: " + request.error);

            // Manejar diferentes tipos de errores
            string errorMessage = "";
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    errorMessage = "Error de conexión. Verifica tu internet.";
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    errorMessage = "Error del servidor: " + request.responseCode;
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    errorMessage = "Error procesando datos.";
                    break;
                default:
                    errorMessage = "Error desconocido: " + request.error;
                    break;
            }

            UpdateStatusText(errorMessage);
        }
    }

    private bool SceneExistsInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameFromBuild == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    // ✅ NUEVO: Método para limpiar datos de usuario (logout)
    public void Logout()
    {
        PlayerPrefs.DeleteKey("AuthToken");
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("UserEmail");
        PlayerPrefs.DeleteKey("UserLevel");
        PlayerPrefs.DeleteKey("UserExperience");
        PlayerPrefs.DeleteKey("UserGold");
        PlayerPrefs.DeleteKey("UserGems");
        PlayerPrefs.Save();

        Debug.Log("Usuario desconectado");
        UpdateStatusText("Sesión cerrada");
    }

    // ✅ NUEVO: Método para verificar si hay sesión activa
    public bool HasActiveSession()
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString("AuthToken", ""));
    }

    // ✅ NUEVO: Obtener datos del usuario guardados
    public UserData GetSavedUserData()
    {
        if (!HasActiveSession()) return null;

        return new UserData
        {
            id = PlayerPrefs.GetString("UserId", ""),
            username = PlayerPrefs.GetString("Username", ""),
            email = PlayerPrefs.GetString("UserEmail", ""),
            level = PlayerPrefs.GetInt("UserLevel", 1),
            experience = PlayerPrefs.GetInt("UserExperience", 0),
            gold = PlayerPrefs.GetInt("UserGold", 0),
            gems = PlayerPrefs.GetInt("UserGems", 0)
        };
    }
}

#if UNITY_EDITOR
// ✅ Editor script mejorado
[CustomEditor(typeof(LoginManager))]
public class LoginManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LoginManager manager = (LoginManager)target;
        
        // Dibujar el inspector normal
        DrawDefaultInspector();
        
        // Agregar información útil
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Información de Debug", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Sesión activa: " + manager.HasActiveSession());
            
            if (manager.HasActiveSession())
            {
                var userData = manager.GetSavedUserData();
                if (userData != null)
                {
                    EditorGUILayout.LabelField("Usuario: " + userData.username);
                    EditorGUILayout.LabelField("Email: " + userData.email);
                    EditorGUILayout.LabelField("Nivel: " + userData.level);
                }
            }
        }
        
        // Botones de utilidad
        EditorGUILayout.Space();
        if (GUILayout.Button("Limpiar Datos de Usuario"))
        {
            manager.Logout();
            Debug.Log("Datos de usuario limpiados");
        }
        
        // Actualizar los nombres de escena basados en los assets arrastrados
        if (manager.availableScenes != null)
        {
            for (int i = 0; i < manager.availableScenes.Length; i++)
            {
                if (manager.availableScenes[i].sceneAsset != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(manager.availableScenes[i].sceneAsset);
                    if (assetPath.EndsWith(".unity"))
                    {
                        string newSceneName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                        if (manager.availableScenes[i].sceneName != newSceneName)
                        {
                            manager.availableScenes[i].sceneName = newSceneName;
                            EditorUtility.SetDirty(manager);
                        }
                    }
                }
            }
        }
    }
}
#endif