using PlaceableObject.Ships;

namespace PlaceableObject.Events
{
    public readonly struct ShipDamagedEvent
    {
        public readonly Ship Target;
        public readonly int Damage;
        public readonly int RemainingHealth;

        public ShipDamagedEvent(Ship target, int damage, int remainingHealth)
        {
            Target = target;
            Damage = damage;
            RemainingHealth = remainingHealth;
        }
    }
}
