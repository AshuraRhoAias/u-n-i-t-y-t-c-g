using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("UI References")]
    public Button battleButton;
    public Button cancelButton;

    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
    }

    void InitializeUI()
    {
        // Validar que los botones estén asignados
        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(false);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Cancel button no está asignado en " + gameObject.name);
        }
    }

    void SetupButtonListeners()
    {
        // Asignar eventos con validación
        if (battleButton != null)
        {
            battleButton.onClick.AddListener(OnBattleClick);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Battle button no está asignado en " + gameObject.name);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClick);
        }
    }

    void OnBattleClick()
    {
        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(true);
            // Opcional: Desactivar el botón de batalla para evitar múltiples clics
            if (battleButton != null)
            {
                battleButton.interactable = false;
            }
        }
    }

    void OnCancelClick()
    {
        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(false);
            // Reactivar el botón de batalla
            if (battleButton != null)
            {
                battleButton.interactable = true;
            }
        }
    }

    // Limpiar listeners al destruir el objeto
    void OnDestroy()
    {
        if (battleButton != null)
        {
            battleButton.onClick.RemoveListener(OnBattleClick);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(OnCancelClick);
        }
    }
}