using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaifuSummoner.Effects
{
    [Serializable]
    public class ModifyStatsEffectData
    {
        public enum StatToModify { Attack, Ambush, Level }

        public StatToModify stat = StatToModify.Attack;
        public IncreaseDecrease action = IncreaseDecrease.Increase;
        public int value = 1;
        public Target targetType = Target.None;
        public int amount = 1;
        public HighLowOption highLow;
        public WaifuStats situationalStat;
        public TieBreaker tieBreaker;
        public TargetSide targetSide = TargetSide.Both;
        public Duration duration = Duration.None;
        public Stages untilStage = Stages.None;
        public int durationTurns = 1;

        [SerializeField]
        public List<EffectFilterData> filters = new List<EffectFilterData>();
    }
}