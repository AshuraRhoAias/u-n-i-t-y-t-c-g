// Assets/WaifuSummoner/Scripts/Developer Data Manager/Data/Efectos/ControlEffectData.cs
using System.Collections.Generic;
using UnityEngine;

namespace WaifuSummoner.Effects
{
    /// <summary>
    /// Datos para el efecto "Control", con flujo condicional y filtros dinámicos.
    /// </summary>
    [System.Serializable]
    public class ControlEffectData
    {
        /// <summary>1) Tipo de acción de Control (None/…)</summary>
        public Control control = Control.None;

        /// <summary>2) Cómo elegimos el objetivo (None/All/…)</summary>
        public Target target = Target.None;

        /// <summary>3) Cantidad (solo para Select/Random/Situational; siempre ≥1)</summary>
        public int amount = 1;

        /// <summary>4) Extras si target == Situational</summary>
        public HighLowOption highLow;
        public StatType statType;
        public TieBreaker tieBreaker;

        /// <summary>5) De qué lado elegimos</summary>
        public TargetSide targetSide = TargetSide.Both;

        /// <summary>6) Duración del control</summary>
        public Duration duration = Duration.None;

        /// <summary>
        /// 7a) Para Duration == UntilTheNext: etapa objetivo
        /// 7b) Para ForNumberTurns / ForNumberOfYourTurns: cuántos turnos
        /// </summary>
        public Stages untilStage = Stages.None;
        public int durationTurns = 1;

        /// <summary>8) Filtros dinámicos usando la clase genérica.</summary>
        [SerializeField]
        public List<EffectFilterData> filters = new List<EffectFilterData>();
    }
}