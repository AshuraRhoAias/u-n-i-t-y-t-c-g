// ==================================================
// OPCIÓN 1: Usando la clase genérica EffectFilterData (RECOMENDADO)
// ==================================================
using System.Collections.Generic;
using UnityEngine;
using WaifuSummoner.Effects;

namespace WaifuSummoner.Effects
{
    /// <summary>
    /// Datos para el efecto "Defeat", usando la clase genérica de filtros.
    /// </summary>
    [System.Serializable]
    public class DefeatEffectData
    {
        public Target targetType;
        public TargetSide targetSide;
        public int amount = 1;                     // Solo para Select o Random

        /// <summary>
        /// Filtros usando la clase genérica EffectFilterData
        /// </summary>
        public List<EffectFilterData> filters = new List<EffectFilterData>();

        // Para Situational:
        public HighLowOption situationalHighLow;
        public StatType situationalStat;
        public TieBreaker situationalTieBreaker;
    }

    /// <summary>
    /// Clase genérica para filtros de efectos (añadir si no existe)
    /// </summary>
    [System.Serializable]
    public class EffectFilterData
    {
        public EffectFilterType filterType;
        public SummonCondition summonCondition;
        public Role roleFilter;
        public ElementType elementFilter;
        // Añadir si es necesario:
        // public RealmType reignFilter;
    }

    /// <summary>
    /// Tipos de filtro para efectos
    /// </summary>
    public enum EffectFilterType
    {
        SummonCondition,
        Role,
        Element,
        Reign
    }
}

// ==================================================
// OPCIÓN 2: Si quieres mantener clases específicas
// ==================================================
namespace WaifuSummoner.Effects
{
    /// <summary>
    /// Tipos de filtro específicos para Defeat
    /// </summary>
    public enum DefeatFilterType
    {
        SummonCondition,
        Role,
        Element
    }

    /// <summary>
    /// Filtro específico para efectos de Defeat
    /// </summary>
    [System.Serializable]
    public class DefeatFilter
    {
        public DefeatFilterType filterType;
        public SummonCondition summonConditionFilter;  // ← Nombre correcto
        public Role roleFilter;
        public ElementType elementFilter;
    }

    /// <summary>
    /// Datos para el efecto "Defeat" con filtros específicos
    /// </summary>
    [System.Serializable]
    public class DefeatEffectDataSpecific
    {
        public Target targetType;
        public TargetSide targetSide;
        public int amount = 1;                     // Solo para Select o Random
        public List<DefeatFilter> filters = new List<DefeatFilter>();

        // Para Situational:
        public HighLowOption situationalHighLow;
        public StatType situationalStat;
        public TieBreaker situationalTieBreaker;
    }
}

// ==================================================
// EJEMPLO DE USO CORREGIDO
// ==================================================
namespace WaifuSummoner.GameLogic
{
    public class DefeatEffectManager : MonoBehaviour
    {
        [SerializeField] private DefeatEffectData defeatEffect;

        void ProcessDefeatEffect()
        {
            // Usando la clase genérica EffectFilterData
            foreach (var filter in defeatEffect.filters)
            {
                switch (filter.filterType)
                {
                    case EffectFilterType.SummonCondition:
                        // Procesar filtro de SummonCondition
                        var condition = filter.summonCondition;  // ← Propiedad correcta
                        break;
                    case EffectFilterType.Role:
                        // Procesar filtro de Role
                        var role = filter.roleFilter;
                        break;
                    case EffectFilterType.Element:
                        // Procesar filtro de Element
                        var element = filter.elementFilter;
                        break;
                    case EffectFilterType.Reign:
                        // Procesar filtro de Reign (si es necesario)
                        // var reign = filter.reignFilter;  // ← Comentar si no existe
                        break;
                }
            }
        }
    }
}