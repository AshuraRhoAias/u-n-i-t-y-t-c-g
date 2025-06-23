using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Qu� puede filtrar el MultiAttacksEffect.
/// </summary>
public enum MultiAttackFilterType
{
    SummonCondition,
    Role,
    Element,
    Reign,
}

/// <summary>
/// Un �nico filtro para MultiAttacks.
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
/// Datos para el efecto �Multiple Attacks�:
///
///  0) Tipo de multi-ataque (NumberOfTimes/NumberVsWaifus/AllWaifus)
///  1) C�mo seleccionamos objetivos (Target)
///  2) Cantidad (solo si NumberOfTimes o NumberVsWaifus)
///  3) Si Situational: Highest/Lowest + Stat a comparar
///  4) De qu� lado (TargetSide)
///  5) Duraci�n (Duration + hasta etapa / turns)
///  6) Filtros din�micos (SummonCondition/Role/Element/Reign)
/// </summary>
[Serializable]
public class MultiAttacksEffectData
{
    // 0)
    public MultiAttackTypes multiAttackType = MultiAttackTypes.None;

    // 1)
    public Target target = Target.None;

    // 2)
    public int amount = 1;

    // 3) Situational extras
    public HighLowOption highLow;
    public StatType situationalStat;

    // 4)
    public TargetSide targetSide = TargetSide.Both;

    // 5) Duraci�n
    public Duration duration = Duration.None;
    public Stages untilStage = Stages.None;
    public int durationTurns = 1;

    // 6) Filtros din�micos
    [SerializeField]
    public List<MultiAttackFilterData> filters = new List<MultiAttackFilterData>();
}
