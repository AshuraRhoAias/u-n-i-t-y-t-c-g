// ==================================================
// TODAS LAS DECLARACIONES USING AL PRINCIPIO
// ==================================================
using System;
using System.Collections.Generic;
using UnityEngine;
using WaifuSummoner.Effects;

// ==================================================
// CollectionData.cs
// ==================================================
namespace WaifuSummoner.Data
{
    public enum FormatType { Collection, Deck }

    [CreateAssetMenu(menuName = "WaifuSummoner/Collection Data", fileName = "NewCollectionData")]
    public class CollectionData : ScriptableObject
    {
        public FormatType format = FormatType.Deck;
        public string displayName = "NewDeck";
        public string identifier = "NEW";
    }
}

// ==================================================
// EffectFilterData.cs - Clase genérica para filtros
// ==================================================
namespace WaifuSummoner.Effects
{
    /// <summary>
    /// Tipos de filtro disponibles para efectos.
    /// </summary>
    public enum EffectFilterType
    {
        SummonCondition,
        Role,
        Element,
        Reign
    }

    /// <summary>
    /// Datos de un único filtro para efectos. Clase genérica reutilizable.
    /// </summary>
    [Serializable]
    public class EffectFilterData
    {
        public EffectFilterType filterType;
        public SummonCondition summonCondition;
        public Role roleFilter;
        public ElementType elementFilter;
        public Realm reignFilter;
    }
}

// ==================================================
// ChangePositionEffectData.cs
// ==================================================
namespace WaifuSummoner.Effects
{
    /// <summary>
    /// Datos para el efecto "Change Position":
    ///   0) Nueva posición (WaifuPosition)
    ///   1) Target selection (Target)
    ///   2) Amount (solo para Select/Random/Situational)
    ///   3) Situational extras (HighLowOption, StatType, TieBreaker)
    ///   4) Target Side
    ///   5) Duration (Duration + Stages/Turns)
    ///   6) Filtros dinámicos
    /// </summary>
    [Serializable]
    public class ChangePositionEffectData
    {
        /// <summary>0) La posición a la que cambiamos la waifu.</summary>
        public WaifuPosition newPosition;

        /// <summary>1) Cómo seleccionamos objetivos.</summary>
        public Target target = Target.None;

        /// <summary>2) Cuántos (solo Select/Random/Situational; siempre ≥1).</summary>
        public int amount = 1;

        /// <summary>3) Extras situational (solo si target == Situational).</summary>
        public HighLowOption highLow;
        public StatType statType;
        public TieBreaker tieBreaker;

        /// <summary>4) De qué lado elegimos (Both/Enemy/User).</summary>
        public TargetSide targetSide = TargetSide.Both;

        /// <summary>5) Duración del cambio de posición.</summary>
        public Duration duration = Duration.None;
        public Stages untilStage = Stages.None;
        public int durationTurns = 1;

        /// <summary>
        /// 6) Filtros dinámicos usando la clase genérica.
        /// </summary>
        [SerializeField]
        public List<EffectFilterData> filters = new List<EffectFilterData>();
    }
}

// ==================================================
// ControlEffectData.cs
// ==================================================
namespace WaifuSummoner.Effects
{
    /// <summary>
    /// Datos para el efecto "Control", con flujo condicional y filtros dinámicos.
    /// </summary>
    [Serializable]
    public class ControlEffectData
    {
        /// <summary>1) Tipo de acción de Control (None/…)</summary>
        public Control control = Control.None;

        /// <summary>2) Cómo elegimos el objetivo (None/All/…)</summary>
        public Target target = Target.None;

        /// <summary>3) Cantidad (solo para Select/Random/Situational; siempre ≥1)</summary>
        public int amount = 1;

        /// <summary>4) Extras si target == Situational</summary>
        public HighLowOption highLow;
        public StatType statType;
        public TieBreaker tieBreaker;

        /// <summary>5) De qué lado elegimos</summary>
        public TargetSide targetSide = TargetSide.Both;

        /// <summary>6) Duración del control</summary>
        public Duration duration = Duration.None;

        /// <summary>
        /// 7a) Para Duration == UntilTheNext: etapa objetivo
        /// 7b) Para ForNumberTurns / ForNumberOfYourTurns: cuántos turnos
        /// </summary>
        public Stages untilStage = Stages.None;
        public int durationTurns = 1;

        /// <summary>8) Filtros dinámicos usando la clase genérica.</summary>
        [SerializeField]
        public List<EffectFilterData> filters = new List<EffectFilterData>();
    }
}

// ==================================================
// Ejemplo de uso en otros archivos
// ==================================================
namespace WaifuSummoner.GameLogic
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private ChangePositionEffectData changePositionEffect;
        [SerializeField] private ControlEffectData controlEffect;
        [SerializeField] private WaifuSummoner.Data.CollectionData collectionData;

        void Start()
        {
            // Ejemplo de uso
            if (changePositionEffect.filters.Count > 0)
            {
                foreach (var filter in changePositionEffect.filters)
                {
                    switch (filter.filterType)
                    {
                        case EffectFilterType.SummonCondition:
                            // Lógica para SummonCondition
                            break;
                        case EffectFilterType.Role:
                            // Lógica para Role
                            break;
                        case EffectFilterType.Element:
                            // Lógica para Element
                            break;
                        case EffectFilterType.Reign:
                            // Lógica para Reign
                            break;
                    }
                }
            }
        }
    }
}