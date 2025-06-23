using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RecycleEffectData
{
    public enum RecycleFilterType
    {
        CardType,
        Reign,
        Role,
        Element
    }

    [Serializable]
    public class RecycleFilter
    {
        public RecycleFilterType filterType;
        public CardType cardType;
        public Realm reignFilter;
        public Role roleFilter;
        public ElementType elementFilter;
    }

    // 1) Qué reciclar del Void Zone
    public Target recycleTarget = Target.None;
    public int recycleAmount = 1;
    public Location recycleToLocation = Location.ShuffleToDeck;

    // 2) Filtros para lo que reciclas
    [SerializeField]
    public List<RecycleFilter> recycleFilters = new List<RecycleFilter>();

    // 3) Situational para reciclar
    public HighLowOption recycleHighLow;
    public StatType recycleStat;
    public TieBreaker recycleTieBreaker;

    // 4) Qué robar después
    public Target drawTarget = Target.None;
    public int drawAmount = 1;
    public Location drawFromLocation = Location.UserDeck;

    // 5) Filtros para lo que robas
    [SerializeField]
    public List<RecycleFilter> drawFilters = new List<RecycleFilter>();

    // 6) Situational para robar
    public HighLowOption drawHighLow;
    public StatType drawStat;
    public TieBreaker drawTieBreaker;
}