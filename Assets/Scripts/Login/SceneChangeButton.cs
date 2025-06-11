using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Tu clase para el botón
public class SceneChangeButton : MonoBehaviour
{
    public Button button;

    [System.Serializable]
    public class SceneOption
    {
        public string sceneName;

#if UNITY_EDITOR
        public Object sceneAsset; // Referencia directa a la escena (solo en editor)
#endif
    }

    public SceneOption targetScene;

    public void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(CambiarEscena);
    }

    public void CambiarEscena()
    {
        if (!string.IsNullOrEmpty(targetScene.sceneName))
        {
            Debug.Log("Cambiando a la escena: " + targetScene.sceneName);
            SceneManager.LoadScene(targetScene.sceneName);
        }
    }
}

#if UNITY_EDITOR
// Editor personalizado - solo esto es lo que necesitas
[CustomEditor(typeof(SceneChangeButton))]
public class SceneChangeButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SceneChangeButton sceneButton = (SceneChangeButton)target;
        
        // Dibujar el inspector normal
        DrawDefaultInspector();
        
        // Actualizar el nombre de la escena basado en el asset arrastrado
        if (sceneButton.targetScene.sceneAsset != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(sceneButton.targetScene.sceneAsset);
            if (assetPath.EndsWith(".unity"))
            {
                string newSceneName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                if (sceneButton.targetScene.sceneName != newSceneName)
                {
                    sceneButton.targetScene.sceneName = newSceneName;
                    EditorUtility.SetDirty(sceneButton);
                }
            }
        }
    }
}
#endif