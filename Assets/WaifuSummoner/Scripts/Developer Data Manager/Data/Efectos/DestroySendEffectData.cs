// ==================================================
// DestroySendEffectData.cs - Versión Corregida
// ==================================================
using System;
using System.Diagnostics;
using UnityEngine;
using WaifuSummoner.Effects;

namespace WaifuSummoner.Effects
{
    [Serializable]
    public enum DestroySendAction
    {
        Destroy,    // send to void
        Send        // send to chosen location
    }

    [Serializable]
    public class DestroySendEffectData
    {
        public Target targetType;            // None/All/Select/Random/Situational/Self
        public CardType cardType;            // Waifu/Enchantment/Mood/Accessory
        public DestroySendAction actionType; // Destroy vs Send

        // — Solo si actionType == Send —
        public Location location;            // chosen location

        // — Filtro extra para encantamientos que destruyes —
        public EnchantmentPosition enchantmentPosition = EnchantmentPosition.Any;

        // — Duración —
        public Duration duration;            // enum Duration
        public int durationTurns;            // para ForNumberTurns / ForNumberOfYourTurns

        // → Para UntilTheNext:
        public Stages untilStage = Stages.None;  // etapa de turno
        public int stageCount = 1;               // cuántas de esas etapas
    }
}

// ==================================================
// Ejemplo de uso
// ==================================================
namespace WaifuSummoner.GameLogic
{
    public class DestroySendEffectManager : MonoBehaviour
    {
        [SerializeField] private DestroySendEffectData destroySendEffect;

        void ProcessDestroySendEffect()
        {
            // Verificar el tipo de acción
            switch (destroySendEffect.actionType)
            {
                case DestroySendAction.Destroy:
                    // Lógica para destruir (enviar al void)
                    DestroyCard();
                    break;

                case DestroySendAction.Send:
                    // Lógica para enviar a ubicación específica
                    SendCardToLocation(destroySendEffect.location);
                    break;
            }

            // Verificar duración si es necesario
            if (destroySendEffect.duration != Duration.None)
            {
                ApplyDurationEffect();
            }
        }

        private void DestroyCard()
        {
            // Implementar lógica de destrucción
            Debug.Log($"Destroying {destroySendEffect.cardType} card");
        }

        private void SendCardToLocation(Location targetLocation)
        {
            // Implementar lógica de envío
            Debug.Log($"Sending {destroySendEffect.cardType} to {targetLocation}");
        }

        private void ApplyDurationEffect()
        {
            // Implementar lógica de duración
            switch (destroySendEffect.duration)
            {
                case Duration.ForNumberTurns:
                case Duration.ForNumberOfYourTurns:
                    Debug.Log($"Effect lasts for {destroySendEffect.durationTurns} turns");
                    break;

                case Duration.UntilTheNext:
                    Debug.Log($"Effect lasts until {destroySendEffect.untilStage} (x{destroySendEffect.stageCount})");
                    break;
            }
        }
    }
}