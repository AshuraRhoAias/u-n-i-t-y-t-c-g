using System.Xml.Linq;
using UnityEngine;

[System.Serializable]
public class EffectTargetFilter
{
    public TargetSide side;
    public bool useSummonCondition;
    public SummonCondition summonCondition;
    public bool useTypeFilter;
    public CardType typeFilter;
    public bool useAttributeFilter;
    public ElementType attributeFilter;
}

[System.Serializable]
public class EffectTargetSelection  // Cambiar nombre también
{
    public Target mode;
    public int quantity;
    public EffectTargetFilter filter;
    public bool useSituational;
    public bool highest;
    public StatType stat;
    public TieBreaker tieBreaker;
}