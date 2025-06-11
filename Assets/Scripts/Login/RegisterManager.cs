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

public class RegisterManager : MonoBehaviour
{
    [Header("Campos de Entrada Requeridos")]
    public TMP_InputField usernameField;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;

    [Header("Campos de Entrada Opcionales")]
    public TMP_InputField firstNameField;
    public TMP_InputField lastNameField;
    public TMP_InputField phoneField;

    [Header("Configuración de Envío")]
    public Button submitButton;
    public string apiUrl = "http://localhost:3000/auth/register"; // ✅ URL Corregida

    [Header("Configuración de Escena")]
    public TMP_Dropdown targetSceneDropdown;

    [Header("UI Feedback")]
    public TMP_Text statusText; // Para mostrar mensajes al usuario
    public GameObject loadingPanel; // Panel de carga opcional
    public Slider passwordStrengthSlider; // Indicador de fortaleza de contraseña

    [System.Serializable]
    public class SceneOption
    {
        public string sceneName;

#if UNITY_EDITOR
        public Object sceneAsset;
#endif
    }

    // ✅ Estructura de datos corregida para el backend
    [System.Serializable]
    public class RegistrationData
    {
        public string username;
        public string email;
        public string password;
        public string firstName;  // Opcional
        public string lastName;   // Opcional
        public string phone;      // Opcional
    }

    // ✅ Respuesta actualizada según el backend
    [System.Serializable]
    public class RegisterResponse
    {
        public bool success;
        public string message;
        public string token;
        public UserData user;
        public GameData gameData;
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

    // ✅ Nueva clase para datos de juego
    [System.Serializable]
    public class GameData
    {
        public string message;
        public CollectionData collection;
        public DeckData starterDeck;
    }

    [System.Serializable]
    public class CollectionData
    {
        public string _id;
        public int totalCards;
        public int uniqueCards;
    }

    [System.Serializable]
    public class DeckData
    {
        public string _id;
        public string name;
        public int totalCards;
        public bool isComplete;
    }

    public SceneOption[] availableScenes;
    private string targetSceneName = "";

    private void Start()
    {
        Debug.Log("[RegisterManager] Inicializando sistema de registro...");

        // Configurar botón principal
        submitButton.onClick.AddListener(EnviarDatos);

        // Configurar dropdown de escenas
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
            targetSceneName = availableScenes[0].sceneName;
        }
        else
        {
            targetSceneName = "SampleScene";
        }

        // ✅ Configurar validación en tiempo real
        if (passwordField != null)
        {
            passwordField.onValueChanged.AddListener(OnPasswordChanged);
        }

        if (emailField != null)
        {
            emailField.onValueChanged.AddListener(OnEmailChanged);
        }

        // Inicializar UI
        UpdateStatusText("");
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    private void OnSceneDropdownValueChanged(int index)
    {
        if (index >= 0 && index < availableScenes.Length)
        {
            targetSceneName = availableScenes[index].sceneName;
            Debug.Log("Escena destino cambiada a: " + targetSceneName);
        }
    }

    // ✅ Validación de fortaleza de contraseña
    private void OnPasswordChanged(string password)
    {
        if (passwordStrengthSlider != null)
        {
            float strength = CalculatePasswordStrength(password);
            passwordStrengthSlider.value = strength;
        }
    }

    // ✅ Validación de email
    private void OnEmailChanged(string email)
    {
        if (!string.IsNullOrEmpty(email) && IsValidEmail(email))
        {
            if (emailField != null && emailField.image != null)
            {
                emailField.image.color = Color.green;
            }
        }
        else if (!string.IsNullOrEmpty(email))
        {
            if (emailField != null && emailField.image != null)
            {
                emailField.image.color = Color.red;
            }
        }
    }

    private float CalculatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password)) return 0f;

        float strength = 0f;

        // Longitud
        if (password.Length >= 6) strength += 0.2f;
        if (password.Length >= 8) strength += 0.2f;
        if (password.Length >= 12) strength += 0.1f;

        // Complejidad
        if (System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]")) strength += 0.1f;
        if (System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]")) strength += 0.1f;
        if (System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]")) strength += 0.1f;
        if (System.Text.RegularExpressions.Regex.IsMatch(password, @"[^a-zA-Z0-9]")) strength += 0.2f;

        return Mathf.Clamp01(strength);
    }

    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".") && email.Length > 5;
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
        Debug.Log("[RegisterManager] Iniciando proceso de registro...");

        // ✅ Validaciones mejoradas
        if (!ValidateFields())
        {
            return;
        }

        UpdateStatusText("Registrando usuario...");
        if (loadingPanel != null) loadingPanel.SetActive(true);

        StartCoroutine(EnviarDatosCoroutine());
    }

    // ✅ Validaciones completas
    private bool ValidateFields()
    {
        // Campos requeridos
        if (string.IsNullOrEmpty(usernameField.text.Trim()))
        {
            UpdateStatusText("El nombre de usuario es requerido");
            return false;
        }

        if (usernameField.text.Trim().Length < 3)
        {
            UpdateStatusText("El nombre de usuario debe tener al menos 3 caracteres");
            return false;
        }

        if (string.IsNullOrEmpty(emailField.text.Trim()))
        {
            UpdateStatusText("El email es requerido");
            return false;
        }

        if (!IsValidEmail(emailField.text.Trim()))
        {
            UpdateStatusText("Por favor ingresa un email válido");
            return false;
        }

        if (string.IsNullOrEmpty(passwordField.text))
        {
            UpdateStatusText("La contraseña es requerida");
            return false;
        }

        if (passwordField.text.Length < 6)
        {
            UpdateStatusText("La contraseña debe tener al menos 6 caracteres");
            return false;
        }

        return true;
    }

    IEnumerator EnviarDatosCoroutine()
    {
        // ✅ Preparar datos completos
        RegistrationData data = new RegistrationData
        {
            username = usernameField.text.Trim(),
            email = emailField.text.Trim().ToLower(),
            password = passwordField.text,
            firstName = firstNameField != null ? firstNameField.text.Trim() : "",
            lastName = lastNameField != null ? lastNameField.text.Trim() : "",
            phone = phoneField != null ? phoneField.text.Trim() : ""
        };

        string jsonData = JsonUtility.ToJson(data);
        Debug.Log("JSON enviado: " + jsonData);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        // Ocultar panel de carga
        if (loadingPanel != null) loadingPanel.SetActive(false);

        // ✅ Variables para controlar el flujo fuera del try-catch
        bool registrationSuccessful = false;
        string successMessage = "";

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Respuesta del servidor: " + request.downloadHandler.text);

            try
            {
                // ✅ Parsear respuesta del backend
                RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);

                if (response != null && response.success)
                {
                    Debug.Log("¡Registro exitoso! Usuario: " + response.user.username);
                    successMessage = "¡Registro exitoso! Iniciando juego...";
                    UpdateStatusText(successMessage);

                    // ✅ Guardar datos del usuario registrado
                    SaveUserData(response);

                    // ✅ Mostrar información de setup de juego
                    if (response.gameData != null)
                    {
                        Debug.Log("Setup de juego: " + response.gameData.message);

                        if (response.gameData.collection != null)
                        {
                            Debug.Log($"Colección creada: {response.gameData.collection.totalCards} cartas");
                        }

                        if (response.gameData.starterDeck != null)
                        {
                            Debug.Log($"Deck inicial: {response.gameData.starterDeck.name} ({response.gameData.starterDeck.totalCards} cartas)");
                        }
                    }

                    registrationSuccessful = true;
                }
                else
                {
                    string errorMessage = response != null ? response.message : "Error desconocido";
                    Debug.LogWarning("Registro fallido: " + errorMessage);
                    UpdateStatusText("Error: " + errorMessage);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parseando respuesta JSON: " + e.Message);
                Debug.LogError("Respuesta recibida: " + request.downloadHandler.text);
                UpdateStatusText("Error procesando respuesta del servidor");
            }
        }
        else
        {
            Debug.LogError("Error de conexión: " + request.error);
            Debug.LogError("Código de respuesta: " + request.responseCode);
            Debug.LogError("Respuesta del servidor: " + request.downloadHandler.text);

            // ✅ Manejo específico de errores
            string errorMessage = "";
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    errorMessage = "Error de conexión. Verifica tu internet.";
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    if (request.responseCode == 400)
                    {
                        errorMessage = "Usuario o email ya existe. Usa datos diferentes.";
                    }
                    else if (request.responseCode == 422)
                    {
                        errorMessage = "Datos inválidos. Verifica tu información.";
                    }
                    else
                    {
                        errorMessage = "Error del servidor: " + request.responseCode;
                    }
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

        // ✅ MOVER yield return FUERA del try-catch
        if (registrationSuccessful)
        {
            // Pequeña pausa para mostrar mensaje de éxito
            yield return new WaitForSeconds(1f);

            // Cargar escena después del registro exitoso
            if (SceneExists(targetSceneName))
            {
                Debug.Log("Cargando escena: " + targetSceneName);
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError("Error: La escena '" + targetSceneName + "' no existe en Build Settings.");
                UpdateStatusText("Registro exitoso, pero error cargando escena");
            }
        }
    }

    // ✅ Guardar datos completos del usuario
    private void SaveUserData(RegisterResponse response)
    {
        if (response.user != null)
        {
            PlayerPrefs.SetString("AuthToken", response.token);
            PlayerPrefs.SetString("UserId", response.user.id);
            PlayerPrefs.SetString("Username", response.user.username);
            PlayerPrefs.SetString("UserEmail", response.user.email);
            PlayerPrefs.SetInt("UserLevel", response.user.level);
            PlayerPrefs.SetInt("UserExperience", response.user.experience);
            PlayerPrefs.SetInt("UserGold", response.user.gold);
            PlayerPrefs.SetInt("UserGems", response.user.gems);

            // ✅ Guardar datos de juego si están disponibles
            if (response.gameData != null)
            {
                if (response.gameData.collection != null)
                {
                    PlayerPrefs.SetString("CollectionId", response.gameData.collection._id);
                    PlayerPrefs.SetInt("CollectionCards", response.gameData.collection.totalCards);
                }

                if (response.gameData.starterDeck != null)
                {
                    PlayerPrefs.SetString("StarterDeckId", response.gameData.starterDeck._id);
                    PlayerPrefs.SetString("StarterDeckName", response.gameData.starterDeck.name);
                    PlayerPrefs.SetInt("StarterDeckCards", response.gameData.starterDeck.totalCards);
                }
            }

            PlayerPrefs.Save();
            Debug.Log("Datos del usuario guardados exitosamente");
        }
    }

    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }

    // ✅ Método para limpiar campos
    public void ClearFields()
    {
        usernameField.text = "";
        emailField.text = "";
        passwordField.text = "";
        if (firstNameField != null) firstNameField.text = "";
        if (lastNameField != null) lastNameField.text = "";
        if (phoneField != null) phoneField.text = "";
        UpdateStatusText("");
    }

    // ✅ Método para verificar si hay sesión activa
    public bool HasActiveSession()
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString("AuthToken", ""));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RegisterManager))]
public class RegisterManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RegisterManager manager = (RegisterManager)target;
        
        // Dibujar inspector normal
        DrawDefaultInspector();

        // ✅ Información de debug mejorada
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Información de Debug", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Sesión activa: " + manager.HasActiveSession());
            
            if (manager.HasActiveSession())
            {
                EditorGUILayout.LabelField("Usuario: " + PlayerPrefs.GetString("Username", "N/A"));
                EditorGUILayout.LabelField("Email: " + PlayerPrefs.GetString("UserEmail", "N/A"));
                EditorGUILayout.LabelField("Nivel: " + PlayerPrefs.GetInt("UserLevel", 0));
                EditorGUILayout.LabelField("Oro: " + PlayerPrefs.GetInt("UserGold", 0));
            }
        }

        // ✅ Botones de utilidad
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Utilidades", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Limpiar Campos"))
        {
            if (Application.isPlaying)
            {
                manager.ClearFields();
            }
        }
        
        if (GUILayout.Button("Limpiar Datos Guardados"))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Todos los datos de usuario limpiados");
        }

        // Actualizar nombres de escena automáticamente
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