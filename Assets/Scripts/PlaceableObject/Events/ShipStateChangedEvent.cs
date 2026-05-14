using PlaceableObject.Ships;

namespace PlaceableObject.Events
{
    public readonly struct ShipStateChangedEvent
    {
        public readonly Ship Ship;

        public ShipStateChangedEvent(Ship ship)
        {
            Ship = ship;
        }
    }
}
