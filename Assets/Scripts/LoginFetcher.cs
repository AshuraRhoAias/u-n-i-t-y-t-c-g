using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

public class LoginFetcher : MonoBehaviour
{
    [Header("Configuración de Login")]
    public string loginUrl = "http://localhost:3000/auth/login"; // ✅ URL CORRECTA según tu servidor
    public CardFetcher cardFetcher; // Referencia directa al CardFetcher

    [Header("Credenciales de Prueba")]
    public string testEmail = "jugador4@tcg.com";
    public string testPassword = "123456";

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
        UnityEngine.Debug.Log($"[LoginFetcher] 🚀 === INICIO DE LOGIN ===");
        UnityEngine.Debug.Log($"[LoginFetcher] Intentando login con email: {email}");
        UnityEngine.Debug.Log($"[LoginFetcher] URL completa: {loginUrl}");

        LoginData loginData = new LoginData { email = email, password = password };
        string jsonData = JsonUtility.ToJson(loginData);

        UnityEngine.Debug.Log($"[LoginFetcher] JSON enviado: {jsonData}");

        // ✅ VALIDAR URL ANTES DE ENVIAR
        if (string.IsNullOrEmpty(loginUrl))
        {
            UnityEngine.Debug.LogError("[LoginFetcher] ❌ URL de login está vacía!");
            yield break;
        }

        using (UnityWebRequest request = new UnityWebRequest(loginUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            UnityEngine.Debug.Log($"[LoginFetcher] 📤 Enviando petición...");
            UnityEngine.Debug.Log($"[LoginFetcher] Método: {request.method}");
            UnityEngine.Debug.Log($"[LoginFetcher] Headers: Content-Type: application/json");
            UnityEngine.Debug.Log($"[LoginFetcher] Body size: {bodyRaw.Length} bytes");

            yield return request.SendWebRequest();

            UnityEngine.Debug.Log($"[LoginFetcher] 📥 Respuesta recibida");
            UnityEngine.Debug.Log($"[LoginFetcher] Resultado: {request.result}");
            UnityEngine.Debug.Log($"[LoginFetcher] Código HTTP: {request.responseCode}");
            UnityEngine.Debug.Log($"[LoginFetcher] URL final: {request.url}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                UnityEngine.Debug.Log($"[LoginFetcher] ✅ ÉXITO - Respuesta completa: {responseText}");

                try
                {
                    LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(responseText);

                    if (loginResponse != null && loginResponse.success)
                    {
                        UnityEngine.Debug.Log($"[LoginFetcher] 🎉 Login exitoso! Usuario: {loginResponse.user.username}");
                        UnityEngine.Debug.Log($"[LoginFetcher] Token recibido: {loginResponse.token.Substring(0, 20)}...");

                        SaveLoginData(loginResponse);

                        if (cardFetcher != null)
                        {
                            UnityEngine.Debug.Log("[LoginFetcher] Iniciando CardFetcher...");
                            cardFetcher.StartFetching();
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning("[LoginFetcher] No se encontró la referencia a CardFetcher. Asigna el componente desde el Inspector.");
                        }
                    }
                    else
                    {
                        string errorMessage = loginResponse != null ? loginResponse.message : "Error desconocido";
                        UnityEngine.Debug.LogError($"[LoginFetcher] Login fallido: {errorMessage}");
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"[LoginFetcher] Error parseando respuesta JSON: {e.Message}");
                    UnityEngine.Debug.LogError($"[LoginFetcher] Respuesta recibida: {responseText}");
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"[LoginFetcher] ❌ FALLO - Resultado: {request.result}");
                UnityEngine.Debug.LogError($"[LoginFetcher] Error: {request.error}");
                UnityEngine.Debug.LogError($"[LoginFetcher] Código de respuesta: {request.responseCode}");
                UnityEngine.Debug.LogError($"[LoginFetcher] URL utilizada: {request.url}");
                UnityEngine.Debug.LogError($"[LoginFetcher] Respuesta del servidor: {request.downloadHandler.text}");

                // ✅ ANÁLISIS DETALLADO DEL ERROR
                if (request.responseCode == 404)
                {
                    UnityEngine.Debug.LogError("[LoginFetcher] 🔍 ANÁLISIS DEL 404:");
                    UnityEngine.Debug.LogError($"[LoginFetcher]   • URL enviada: {loginUrl}");
                    UnityEngine.Debug.LogError($"[LoginFetcher]   • URL procesada: {request.url}");
                    UnityEngine.Debug.LogError("[LoginFetcher]   • Servidor funcionando ✅ (según logs)");
                    UnityEngine.Debug.LogError("[LoginFetcher]   • Problema posible: Proxy, CORS, o URL");
                }

                switch (request.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        UnityEngine.Debug.LogError("[LoginFetcher] 🌐 Error de conexión. ¿Firewall o proxy?");
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        UnityEngine.Debug.LogError("[LoginFetcher] 📡 Error de protocolo HTTP");
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        UnityEngine.Debug.LogError("[LoginFetcher] 📊 Error procesando datos");
                        break;
                }
            }
        }

        UnityEngine.Debug.Log($"[LoginFetcher] 🏁 === FIN DE LOGIN ===");
    }

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

            UnityEngine.Debug.Log($"[LoginFetcher] Datos del usuario guardados:");
            UnityEngine.Debug.Log($"  - Username: {response.user.username}");
            UnityEngine.Debug.Log($"  - Level: {response.user.level}");
            UnityEngine.Debug.Log($"  - Gold: {response.user.gold}");
            UnityEngine.Debug.Log($"  - Gems: {response.user.gems}");
        }
    }

    public bool HasValidToken()
    {
        string token = PlayerPrefs.GetString("jwtToken", "");
        return !string.IsNullOrEmpty(token);
    }

    public string GetCurrentToken()
    {
        return PlayerPrefs.GetString("jwtToken", "");
    }

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

        UnityEngine.Debug.Log("[LoginFetcher] Logout completado - datos eliminados");
    }

    [System.Serializable]
    public class LoginData
    {
        public string email;
        public string password;
    }

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
        public string id;
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

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(loginUrl))
        {
            loginUrl = "http://localhost:3000/auth/login"; // ✅ URL correcta según tu servidor
        }
    }

    // ✅ MÉTODO PARA VERIFICAR CONEXIÓN AL SERVIDOR
    public void TestServerConnection()
    {
        StartCoroutine(TestServerHealth());
    }

    IEnumerator TestServerHealth()
    {
        UnityEngine.Debug.Log("[LoginFetcher] 🔍 Probando conexión al servidor...");

        // Primero probar el endpoint de salud
        using (UnityWebRequest healthRequest = UnityWebRequest.Get("http://localhost:3000/health"))
        {
            yield return healthRequest.SendWebRequest();

            if (healthRequest.result == UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log("[LoginFetcher] ✅ Servidor conectado!");
                UnityEngine.Debug.Log($"[LoginFetcher] Respuesta health: {healthRequest.downloadHandler.text}");
            }
            else
            {
                UnityEngine.Debug.LogError($"[LoginFetcher] ❌ Servidor no responde: {healthRequest.error}");
                UnityEngine.Debug.LogError("[LoginFetcher] 🔧 Verifica que el servidor esté corriendo con 'npm start'");
                yield break; // No continuar si el servidor no responde
            }
        }

        // Probar el endpoint raíz para ver información del servidor
        using (UnityWebRequest rootRequest = UnityWebRequest.Get("http://localhost:3000/"))
        {
            yield return rootRequest.SendWebRequest();

            if (rootRequest.result == UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log("[LoginFetcher] 📋 Info del servidor:");
                UnityEngine.Debug.Log($"[LoginFetcher] {rootRequest.downloadHandler.text}");
            }
        }
    }

    // ✅ MÉTODO PARA PROBAR DIFERENTES ENDPOINTS
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void TestDifferentEndpoints()
    {
        string[] possibleUrls = {
            "http://localhost:3000/login",
            "http://localhost:3000/auth/login",
            "http://localhost:3000/api/login",
            "http://localhost:3000/api/auth/login"
        };

        UnityEngine.Debug.Log("[LoginFetcher] 🔧 URLs a probar:");
        for (int i = 0; i < possibleUrls.Length; i++)
        {
            UnityEngine.Debug.Log($"[LoginFetcher] {i + 1}. {possibleUrls[i]}");
        }
    }
}

#if UNITY_EDITOR
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
            
            // ✅ Botón para probar conexión al servidor
            if (GUILayout.Button("🔍 Probar Conexión al Servidor"))
            {
                fetcher.TestServerConnection();
            }
            
            // ✅ Botones para probar diferentes URLs
            if (GUILayout.Button("🔧 Mostrar URLs Posibles"))
            {
                fetcher.TestDifferentEndpoints();
            }
            
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
        EditorGUILayout.LabelField("🔧 Pasos de Diagnóstico", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("1. Usar '🔍 Probar Conexión al Servidor' primero\n2. Verificar que tu servidor esté corriendo: npm start\n3. Revisar el archivo authRoutes.js\n4. Verificar que tengas un usuario registrado", MessageType.Info);
    }
}
#endif