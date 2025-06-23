// EffectTrigger.cs
namespace WaifuSummoner.Battle
{
    public enum Trigger
    {
        None,
        OnSummon,
        OnDefeat,
        OnAttack,
        OnDamage,
        OnTurnStart,
        OnTurnEnd,
        OnDraw,
        Continuous
    }

    public static class TriggerDescriptions
    {
        public static string GetDescription(Trigger trigger)
        {
            return trigger switch
            {
                Trigger.OnSummon => "Activates when the card is summoned",
                Trigger.OnDefeat => "Activates when the card is defeated",
                Trigger.OnAttack => "Activates when the card attacks",
                Trigger.OnDamage => "Activates when the card takes damage",
                Trigger.OnTurnStart => "Activates at the start of turn",
                Trigger.OnTurnEnd => "Activates at the end of turn",
                Trigger.OnDraw => "Activates when drawn",
                Trigger.Continuous => "Always active while card is in play",
                _ => "Unknown trigger"
            };
        }
    }
}

// EffectType.cs
namespace WaifuSummoner.Battle
{
    public enum EffectType
    {
        None,
        ModifyStats,
        DrawSearch,
        Control,
        Protection,
        Stun,
        Negate,
        HealBurn,
        SendHand,
        SendWaifu,
        DestroySend,
        Recycle,
        MultiAttacks,
        SummonAid,
        EnchantSummon,
        ChangePosition,
        Defeat
    }
}

// SummonType.cs
namespace WaifuSummoner.Battle
{
    public enum SummonType
    {
        Normal,
        Turn,
        Seduction,
        Special
    }
}