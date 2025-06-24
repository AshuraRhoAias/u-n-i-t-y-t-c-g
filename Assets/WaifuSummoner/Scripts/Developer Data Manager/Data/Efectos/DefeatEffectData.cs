using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using WaifuSummoner.Effects;

namespace WaifuSummoner.Effects
{
    /// <summary>
    /// Datos para el efecto "Defeat"
    /// </summary>
    [System.Serializable]
    public class DefeatEffectData
    {
        public Target targetType;
        public TargetSide targetSide;
        public int amount = 1;

        /// <summary>
        /// Filtros específicos para Defeat
        /// </summary>
        public List<DefeatFilter> filters = new List<DefeatFilter>();

        // Para Situational:
        public HighLowOption situationalHighLow;
        public StatType situationalStat;
        public TieBreaker situationalTieBreaker;
    }
}

// Ejemplo de uso corregido
namespace WaifuSummoner.GameLogic
{
    public class DefeatEffectManager : MonoBehaviour
    {
        [SerializeField] private DefeatEffectData defeatEffect;

        void ProcessDefeatEffect()
        {
            foreach (var filter in defeatEffect.filters)
            {
                switch (filter.filterType)
                {
                    case DefeatFilterType.SummonCondition:
                        var condition = filter.summonCondition;
                        // Procesar condición de invocación
                        break;

                    case DefeatFilterType.Role:
                        var role = filter.roleFilter;
                        // Procesar filtro de rol
                        break;

                    case DefeatFilterType.Element:
                        var element = filter.elementFilter;
                        // Procesar filtro de elemento
                        break;

                    case DefeatFilterType.Reign:
                        var reign = filter.reignFilter;
                        // Procesar filtro de reino
                        break;

                    default:
                        Debug.LogWarning($"Tipo de filtro no manejado: {filter.filterType}");
                        break;
                }
            }
        }
    }
}
