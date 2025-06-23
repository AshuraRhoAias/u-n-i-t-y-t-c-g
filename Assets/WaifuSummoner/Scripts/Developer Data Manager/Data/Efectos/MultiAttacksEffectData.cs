using System;
using System.Collections.Generic;
using UnityEngine;

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
/// Datos para el efecto "Multi Attacks", con filtros dinámicos.
/// </summary>
[Serializable]
public class MultiAttacksEffectData
{
    /// <summary>1) Cómo seleccionamos el objetivo (None/All/Select/Random/Situational/Self)</summary>
    public Target target = Target.None;

    /// <summary>2) De qué lado elegimos</summary>
    public TargetSide targetSide = TargetSide.Both;

    /// <summary>3) Tipo de ataque múltiple</summary>
    public MultiAttackTypes multiAttackType = MultiAttackTypes.None;

    /// <summary>4) Cantidad (solo para ciertos tipos de ataque múltiple)</summary>
    public int amount = 1;

    /// <summary>5) Extras si target == Situational</summary>
    public HighLowOption highLow;
    public WaifuStats situationalStat;
    public TieBreaker tieBreaker;

    /// <summary>6) Duración del efecto</summary>
    public Duration duration = Duration.None;

    /// <summary>7) Etapa objetivo para UntilTheNext</summary>
    public Stages untilStage = Stages.None;

    /// <summary>8) Número de turnos</summary>
    public int durationTurns = 1;

    /// <summary>9) Filtros dinámicos</summary>
    [SerializeField]
    public List<MultiAttackFilterData> filters = new List<MultiAttackFilterData>();
}