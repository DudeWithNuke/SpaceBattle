using PlaceableObject.Ships;

namespace PlaceableObject.Events
{
    public readonly struct ShipAttackedEvent
    {
        public readonly Ship Attacker;
        public readonly Ship Target;

        public ShipAttackedEvent(Ship attacker, Ship target)
        {
            Attacker = attacker;
            Target = target;
        }
    }
}
