using System.Collections.Generic;
using UnityEngine;
using WaifuSummoner.Effects;

namespace WaifuSummoner.Effects
{
    /// <summary>
    /// Tipos de filtro disponibles para Defeat.
    /// </summary>
    public enum DefeatFilterType
    {
        SummonCondition,
        Role,
        Element,
        Reign
    }

    /// <summary>
    /// Datos de un único filtro de Defeat.
    /// </summary>
    [System.Serializable]
    public class DefeatFilter
    {
        public DefeatFilterType filterType;
        public SummonCondition summonCondition;
        public Role roleFilter;
        public ElementType elementFilter;
        public Realm reignFilter;
    }

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
                        UnityEngine.Debug.Log("Procesando condición de invocación");
                        break;

                    case DefeatFilterType.Role:
                        var role = filter.roleFilter;
                        // Procesar filtro de rol
                        UnityEngine.Debug.Log("Procesando filtro de rol");
                        break;

                    case DefeatFilterType.Element:
                        var element = filter.elementFilter;
                        // Procesar filtro de elemento
                        UnityEngine.Debug.Log("Procesando filtro de elemento");
                        break;

                    case DefeatFilterType.Reign:
                        var reign = filter.reignFilter;
                        // Procesar filtro de reino
                        UnityEngine.Debug.Log("Procesando filtro de reino");
                        break;

                    default:
                        UnityEngine.Debug.LogWarning($"Tipo de filtro no manejado: {filter.filterType}");
                        break;
                }
            }
        }
    }
}