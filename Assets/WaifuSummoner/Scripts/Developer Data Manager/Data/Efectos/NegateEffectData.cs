using System;
using System.Collections.Generic;
using UnityEngine;

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
/// Datos para el efecto "Negate Effect", con filtros dinámicos.
/// </summary>
[Serializable]
public class NegateEffectData
{
    /// <summary>1) Tipo de carta en el campo</summary>
    public FieldCardType fieldCardType = FieldCardType.Any;

    /// <summary>2) Cómo seleccionamos el objetivo</summary>
    public Target target = Target.None;

    /// <summary>3) Cantidad (solo para Select/Random/Situational)</summary>
    public int amount = 1;

    /// <summary>4) Extras si target == Situational</summary>
    public HighLowOption highLow;
    public StatType statType;
    public TieBreaker tieBreaker;

    /// <summary>5) De qué lado elegimos</summary>
    public TargetSide targetSide = TargetSide.Both;

    /// <summary>6) Duración del efecto</summary>
    public Duration duration = Duration.None;

    /// <summary>7) Etapa objetivo para UntilTheNext</summary>
    public Stages untilStage = Stages.None;

    /// <summary>8) Número de turnos</summary>
    public int durationTurns = 1;

    /// <summary>9) Filtros dinámicos</summary>
    [SerializeField]
    public List<NegateFilterData> filters = new List<NegateFilterData>();
}