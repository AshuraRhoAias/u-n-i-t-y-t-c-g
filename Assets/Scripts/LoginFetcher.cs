using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LoginFetcher : MonoBehaviour
{
    [Header("Configuración de Login")]
    public string loginUrl = "http://localhost:3000/auth/login"; // ✅ URL CORREGIDA
    public CardFetcher cardFetcher; // Referencia directa al CardFetcher

    [Header("Credenciales de Prueba")]
    public string testEmail = "test@example.com";
    public string testPassword = "password123";

    [Header("Debug")]
    public bool autoLoginOnStart = true;

    void Start()
    {
        if (autoLoginOnStart)
        {
            StartCoroutine(Login(testEmail, testPassword));
        }
    }

    // ✅ Método público para login manual
    public void StartLogin(string email, string password)
    {
        StartCoroutine(Login(email, password));
    }

    IEnumerator Login(string email, string password)
    {
        Debug.Log($"[LoginFetcher] Intentando login con email: {email}");

        // ✅ CORREGIDO: Usar 'email' en lugar de 'username'
        LoginData loginData = new LoginData { email = email, password = password };
        string jsonData = JsonUtility.ToJson(loginData);

        Debug.Log($"[LoginFetcher] JSON enviado: {jsonData}");
        Debug.Log($"[LoginFetcher] URL: {loginUrl}");

        using (UnityWebRequest request = new UnityWebRequest(loginUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"[LoginFetcher] Respuesta completa: {responseText}");

                try
                {
                    // ✅ CORREGIDO: Usar estructura de respuesta del backend
                    LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(responseText);

                    if (loginResponse != null && loginResponse.success)
                    {
                        Debug.Log($"[LoginFetcher] Login exitoso! Usuario: {loginResponse.user.username}");
                        Debug.Log($"[LoginFetcher] Token recibido: {loginResponse.token.Substring(0, 20)}...");

                        // ✅ Guardar datos completos del usuario
                        SaveLoginData(loginResponse);

                        // Ahora que tenemos el token, buscamos las cartas
                        if (cardFetcher != null)
                        {
                            Debug.Log("[LoginFetcher] Iniciando CardFetcher...");
                            cardFetcher.StartFetching();
                        }
                        else
                        {
                            Debug.LogWarning("[LoginFetcher] No se encontró la referencia a CardFetcher. Asigna el componente desde el Inspector.");
                        }
                    }
                    else
                    {
                        string errorMessage = loginResponse != null ? loginResponse.message : "Error desconocido";
                        Debug.LogError($"[LoginFetcher] Login fallido: {errorMessage}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[LoginFetcher] Error parseando respuesta JSON: {e.Message}");
                    Debug.LogError($"[LoginFetcher] Respuesta recibida: {responseText}");
                }
            }
            else
            {
                Debug.LogError($"[LoginFetcher] Login failed: {request.error}");
                Debug.LogError($"[LoginFetcher] Código de respuesta: {request.responseCode}");
                Debug.LogError($"[LoginFetcher] Respuesta del servidor: {request.downloadHandler.text}");

                // ✅ Manejo específico de errores
                switch (request.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        Debug.LogError("[LoginFetcher] Error de conexión. ¿Está el servidor ejecutándose?");
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        if (request.responseCode == 404)
                        {
                            Debug.LogError("[LoginFetcher] Error 404: Endpoint no encontrado. Verifica la URL del servidor.");
                        }
                        else if (request.responseCode == 401)
                        {
                            Debug.LogError("[LoginFetcher] Error 401: Credenciales incorrectas.");
                        }
                        break;
                }
            }
        }
    }

    // ✅ Guardar datos completos del login
    private void SaveLoginData(LoginResponse response)
    {
        if (response.user != null)
        {
            PlayerPrefs.SetString("jwtToken", response.token);
            PlayerPrefs.SetString("UserId", response.user.id);
            PlayerPrefs.SetString("Username", response.user.username);
            PlayerPrefs.SetString("UserEmail", response.user.email);
            PlayerPrefs.SetInt("UserLevel", response.user.level);
            PlayerPrefs.SetInt("UserExperience", response.user.experience);
            PlayerPrefs.SetInt("UserGold", response.user.gold);
            PlayerPrefs.SetInt("UserGems", response.user.gems);
            PlayerPrefs.Save();

            Debug.Log($"[LoginFetcher] Datos del usuario guardados:");
            Debug.Log($"  - Username: {response.user.username}");
            Debug.Log($"  - Level: {response.user.level}");
            Debug.Log($"  - Gold: {response.user.gold}");
            Debug.Log($"  - Gems: {response.user.gems}");
        }
    }

    // ✅ Método para verificar si hay token guardado
    public bool HasValidToken()
    {
        string token = PlayerPrefs.GetString("jwtToken", "");
        return !string.IsNullOrEmpty(token);
    }

    // ✅ Método para obtener el token actual
    public string GetCurrentToken()
    {
        return PlayerPrefs.GetString("jwtToken", "");
    }

    // ✅ Método para logout
    public void Logout()
    {
        PlayerPrefs.DeleteKey("jwtToken");
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("UserEmail");
        PlayerPrefs.DeleteKey("UserLevel");
        PlayerPrefs.DeleteKey("UserExperience");
        PlayerPrefs.DeleteKey("UserGold");
        PlayerPrefs.DeleteKey("UserGems");
        PlayerPrefs.Save();

        Debug.Log("[LoginFetcher] Logout completado - datos eliminados");
    }

    // ✅ ESTRUCTURA DE DATOS CORREGIDA
    [System.Serializable]
    public class LoginData
    {
        public string email;    // ✅ CORREGIDO: 'email' en lugar de 'username'
        public string password;
    }

    // ✅ RESPUESTA COMPLETA DEL BACKEND
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
        public string id;       // ✅ String para MongoDB ObjectId
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

    // ✅ MÉTODOS DE DEBUG PARA EL EDITOR
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(loginUrl))
        {
            loginUrl = "http://localhost:3000/auth/login";
        }
    }
}

// ✅ EDITOR PERSONALIZADO PARA FACILITAR TESTING
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(LoginFetcher))]
public class LoginFetcherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LoginFetcher fetcher = (LoginFetcher)target;
        
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Herramientas de Testing", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Estado del Token:", fetcher.HasValidToken() ? "✅ Válido" : "❌ No encontrado");
            
            if (fetcher.HasValidToken())
            {
                EditorGUILayout.LabelField("Usuario:", PlayerPrefs.GetString("Username", "N/A"));
                EditorGUILayout.LabelField("Nivel:", PlayerPrefs.GetInt("UserLevel", 0).ToString());
            }

            EditorGUILayout.Space();
            
            if (GUILayout.Button("🔑 Test Login"))
            {
                fetcher.StartLogin(fetcher.testEmail, fetcher.testPassword);
            }
            
            if (GUILayout.Button("🚪 Logout"))
            {
                fetcher.Logout();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Entra en Play Mode para usar las herramientas de testing", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Configuración del Servidor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Asegúrate de que tu servidor esté ejecutándose en:\nhttp://localhost:3000\n\nY que tengas un usuario registrado con las credenciales de prueba.", MessageType.Info);
    }
}
#endif