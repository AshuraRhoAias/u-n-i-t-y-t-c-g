using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaifuSummoner.Effects
{
    /// <summary>
    /// Tipos de filtro disponibles para Negate.
    /// </summary>
    public enum NegateFilterType
    {
        SummonCondition,
        Role,
        Element,
        Reign
    }

    /// <summary>
    /// Datos de un único filtro de Negate.
    /// </summary>
    [Serializable]
    public class NegateFilterData
    {
        public NegateFilterType filterType;
        public SummonCondition summonCondition;
        public Role roleFilter;
        public ElementType elementFilter;
        public Realm reignFilter;
    }

    /// <summary>
    /// Datos para el efecto "Negate", con flujo condicional y filtros dinámicos.
    /// </summary>
    [Serializable]
    public class NegateEffectData
    {
        /// <summary>1) Cómo elegimos el objetivo (None/All/Select/Random/Situational)</summary>
        public Target target = Target.None;

        /// <summary>2) Cantidad (solo para Select/Random/Situational; siempre ≥1)</summary>
        public int amount = 1;

        /// <summary>3) Extras si target == Situational</summary>
        public HighLowOption highLow;
        public StatType statType;
        public TieBreaker tieBreaker;

        /// <summary>4) De qué lado elegimos</summary>
        public TargetSide targetSide = TargetSide.Both;

        /// <summary>5) Duración del efecto de negación</summary>
        public Duration duration = Duration.None;

        /// <summary>6) Para UntilTheNext: etapa objetivo</summary>
        public Stages untilStage = Stages.None;

        /// <summary>7) Para ForNumberTurns / ForNumberOfYourTurns: cuántos turnos</summary>
        public int durationTurns = 1;

        /// <summary>8) Filtros dinámicos</summary>
        [SerializeField]
        public List<NegateFilterData> filters = new List<NegateFilterData>();
    }
}