// Assets/WaifuSummoner/Scripts/Developer Data Manager/Data/Efectos/HealBurnEffectData.cs
using System;

namespace WaifuSummoner.Effects
{
    [Serializable]
    public class HealBurnEffectData
    {
        public IncreaseDecrease action = IncreaseDecrease.None;
        public TargetSide targetSide = TargetSide.User;
        public int amount = 0;
    }
}