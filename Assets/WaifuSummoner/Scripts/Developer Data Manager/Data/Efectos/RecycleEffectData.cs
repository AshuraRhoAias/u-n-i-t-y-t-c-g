// Assets/WaifuSummoner/Scripts/Developer Data Manager/Data/Efectos/RecycleEffectData.cs
using System;

namespace WaifuSummoner.Effects
{
    [Serializable]
    public class RecycleEffectData
    {
        /// <summary>Tipo de carta a reciclar</summary>
        public CardType cardType = CardType.Waifu;

        /// <summary>C�mo seleccionar las cartas</summary>
        public Target target = Target.None;

        /// <summary>De qu� lado reciclar</summary>
        public TargetSide targetSide = TargetSide.User;

        /// <summary>Cantidad de cartas a reciclar</summary>
        public int amount = 1;

        /// <summary>Desde qu� ubicaci�n reciclar</summary>
        public Location fromLocation = Location.UserVoid;

        /// <summary>Hacia qu� ubicaci�n enviar</summary>
        public Location toLocation = Location.UserDeck;

        /// <summary>Si se baraja el mazo despu�s del reciclaje</summary>
        public bool shuffleAfter = true;
    }
}