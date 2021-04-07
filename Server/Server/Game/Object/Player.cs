using System;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Player : GameObject
    {
        
        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Speed = 10.0f;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            Console.WriteLine($" damage : {damage}    from {attacker}");
        }
    }
}