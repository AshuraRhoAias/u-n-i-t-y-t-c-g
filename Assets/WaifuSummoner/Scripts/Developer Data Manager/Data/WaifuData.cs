using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaifuSummoner.Data
{
    [CreateAssetMenu(fileName = "New Waifu Data", menuName = "Waifu Summoner/Waifu Data")]
    public class WaifuData : ScriptableObject
    {
        [Header("Basic Info")]
        public string waifuName;
        public string description;
        public Sprite artwork;

        [Header("Stats")]
        public int attack;
        public int level;
        public int ambush;

        [Header("Classification")]
        public ElementType element;
        public Role role;
        public Realm realm;
        public Rarity rarity;

        [Header("Summon Info")]
        public SummonType summonType;
        public SummonCondition summonCondition;

        [Header("Effects")]
        public List<WaifuEffectGroup> effectGroups = new List<WaifuEffectGroup>();
    }

    public enum TimesPerTurn
    {
        Once,
        Twice,
        Unlimited
    }

    [Serializable]
    public class WaifuEffectGroup
    {
        public string groupName;
        public Trigger trigger;
        public TimesPerTurn timesPerTurn = TimesPerTurn.Once;
        public EffectType effectType;

        [Header("Effect Data")]
        // Aquí irían los datos específicos del efecto según el tipo
        [SerializeField] public string effectDataJson; // Para serializar datos complejos
    }
}