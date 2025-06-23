// Assets/WaifuSummoner/Scripts/Developer Data Manager/Data/Efectos/RecycleEffectData.cs
using System;

namespace WaifuSummoner.Effects
{
    [Serializable]
    public class RecycleEffectData
    {
        /// <summary>Tipo de carta a reciclar</summary>
        public CardType cardType = CardType.Waifu;

        /// <summary>Cómo seleccionar las cartas</summary>
        public Target target = Target.None;

        /// <summary>De qué lado reciclar</summary>
        public TargetSide targetSide = TargetSide.User;

        /// <summary>Cantidad de cartas a reciclar</summary>
        public int amount = 1;

        /// <summary>Desde qué ubicación reciclar</summary>
        public Location fromLocation = Location.VoidZone;  // ← Cambiar de UserVoid a VoidZone

        /// <summary>Hacia qué ubicación enviar</summary>
        public Location toLocation = Location.UserDeck;

        /// <summary>Si se baraja el mazo después del reciclaje</summary>
        public bool shuffleAfter = true;
    }
}