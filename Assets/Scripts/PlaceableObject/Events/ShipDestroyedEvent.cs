using PlaceableObject.Ships;

namespace PlaceableObject.Events
{
    public readonly struct ShipDestroyedEvent
    {
        public readonly Ship DestroyedShip;
        public readonly Ship Attacker;

        public ShipDestroyedEvent(Ship destroyedShip, Ship attacker)
        {
            DestroyedShip = destroyedShip;
            Attacker = attacker;
        }
    }
}
