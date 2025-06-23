// Assets/WaifuSummoner/Scripts/Developer Data Manager/Data/EffectData.cs
using System;

namespace WaifuSummoner.Effects
{
    /// <summary>
    /// Tipos de filtro genéricos que pueden ser usados por diferentes efectos
    /// </summary>
    public enum EffectFilterType
    {
        SummonCondition,
        Role,
        Element,
        Reign
    }

    /// <summary>
    /// Clase genérica de filtro que puede ser reutilizada por diferentes efectos
    /// </summary>
    [Serializable]
    public class EffectFilterData
    {
        public EffectFilterType filterType;
        public SummonCondition summonCondition;
        public Role roleFilter;
        public ElementType elementFilter;
        public Realm reignFilter;
    }

    /// <summary>
    /// Clase base para todos los datos de efectos
    /// </summary>
    [Serializable]
    public abstract class EffectData
    {
        /// <summary>Tipo de efecto</summary>
        public EffectType effectType;

        /// <summary>Si el efecto está activo</summary>
        public bool isActive = true;

        /// <summary>ID único del efecto</summary>
        public string effectId;
    }
}