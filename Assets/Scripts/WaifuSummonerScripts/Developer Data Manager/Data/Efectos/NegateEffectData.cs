using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Qu� puede filtrar el NegateEffect.
/// </summary>
public enum NegateFilterType
{
    SummonCondition,
    Role,
    Element,
    Reign
}

/// <summary>
/// Un �nico filtro para NegateEffect.
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
/// Datos para el efecto �Negate�:
///  0) D�nde aplica (FieldCardType)
///  1) C�mo seleccionamos el objetivo (Target)
///  2) Cantidad (solo Select/Random/Situational)
///  3) Si Situational: Highest/Lowest + Stat a comparar
///  4) De qu� lado (TargetSide)
///  5) Duraci�n (Duration + hasta etapa / turns)
///  6) Filtros din�micos
/// </summary>
[Serializable]
public class NegateEffectData
{
    // 0) Primera selecci�n
    public FieldCardType fieldCardType = FieldCardType.Any;

    // 1)
    public Target target = Target.None;
    public int amount = 1;

    // 2) Situational extras
    public HighLowOption highLow;
    public StatType statType;

    // 3)
    public TargetSide targetSide = TargetSide.Both;

    // 4) Duraci�n
    public Duration duration = Duration.None;
    public Stages untilStage = Stages.None;
    public int durationTurns = 1;

    // 5) Filtros din�micos
    [SerializeField]
    public List<NegateFilterData> filters = new List<NegateFilterData>();
}
