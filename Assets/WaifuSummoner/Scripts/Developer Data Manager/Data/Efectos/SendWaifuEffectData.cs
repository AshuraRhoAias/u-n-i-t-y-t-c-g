using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaifuSummoner.Effects
{
    public enum SendFilterType
    {
        SummonCondition,
        Role,
        Element
    }

    [Serializable]
    public class SendFilter
    {
        public SendFilterType filterType;
        public SummonCondition summonCondition;
        public Role roleFilter;
        public ElementType elementFilter;
    }

    [Serializable]
    public class SendWaifuEffectData
    {
        public Location location;
        public Target selectionType;
        public TargetSide targetSide;
        public int amount = 1;
        public List<SendFilter> filters = new List<SendFilter>();
        public HighLowOption highLow;
        public WaifuStats waifuStats;
        public TieBreaker tieBreaker;
    }
}