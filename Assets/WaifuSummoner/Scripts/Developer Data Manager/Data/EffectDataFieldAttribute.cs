using System;

namespace WaifuSummoner.Data
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EffectDataFieldAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public EffectDataFieldAttribute(string displayName = "", string description = "")
        {
            DisplayName = displayName;
            Description = description;
        }
    }
}