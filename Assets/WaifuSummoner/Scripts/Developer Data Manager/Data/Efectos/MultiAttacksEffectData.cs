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
/// Datos de un �nico filtro de MultiAttacks.
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
/// Datos para el efecto "Multi Attacks", con filtros din�micos.
/// </summary>
[Serializable]
public class MultiAttacksEffectData
{
    /// <summary>1) C�mo seleccionamos el objetivo (None/All/Select/Random/Situational/Self)</summary>
    public Target target = Target.None;

    /// <summary>2) De qu� lado elegimos</summary>
    public TargetSide targetSide = TargetSide.Both;

    /// <summary>3) Tipo de ataque m�ltiple</summary>
    public MultiAttackTypes multiAttackType = MultiAttackTypes.None;

    /// <summary>4) Cantidad (solo para ciertos tipos de ataque m�ltiple)</summary>
    public int amount = 1;

    /// <summary>5) Extras si target == Situational</summary>
    public HighLowOption highLow;
    public WaifuStats situationalStat;
    public TieBreaker tieBreaker;

    /// <summary>6) Duraci�n del efecto</summary>
    public Duration duration = Duration.None;

    /// <summary>7) Etapa objetivo para UntilTheNext</summary>
    public Stages untilStage = Stages.None;

    /// <summary>8) N�mero de turnos</summary>
    public int durationTurns = 1;

    /// <summary>9) Filtros din�micos</summary>
    [SerializeField]
    public List<MultiAttackFilterData> filters = new List<MultiAttackFilterData>();
}