using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaifuSummoner.Effects
{
    /// <summary>
    /// Tipos de filtro disponibles para MultiAttacks.
    /// </summary>
    public enum MultiAttackFilterType
    {
        SummonCondition,
        Role,
        Element,
        Reign
    }

    /// <summary>
    /// Datos de un único filtro de MultiAttacks.
    /// </summary>
    [Serializable]
    public class MultiAttackFilterData
    {
        public MultiAttackFilterType filterType;
        public SummonCondition summonCondition;
        public Role roleFilter;
        public ElementType elementFilter;
        public Realm reignFilter;
    }

    /// <summary>
    /// Datos para el efecto "Multi Attacks", con flujo condicional y filtros dinámicos.
    /// </summary>
    [Serializable]
    public class MultiAttacksEffectData
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

        /// <summary>5) Número de ataques adicionales</summary>
        public int additionalAttacks = 1;

        /// <summary>6) Duración del efecto</summary>
        public Duration duration = Duration.None;

        /// <summary>7) Para UntilTheNext: etapa objetivo</summary>
        public Stages untilStage = Stages.None;

        /// <summary>8) Para ForNumberTurns / ForNumberOfYourTurns: cuántos turnos</summary>
        public int durationTurns = 1;

        /// <summary>9) Filtros dinámicos</summary>
        [SerializeField]
        public List<MultiAttackFilterData> filters = new List<MultiAttackFilterData>();
    }
}