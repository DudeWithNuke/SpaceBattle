namespace PlaceableObject.Events
{
    public readonly struct TurnEndedEvent
    {
        public readonly int TurnNumber;

        public TurnEndedEvent(int turnNumber)
        {
            TurnNumber = turnNumber;
        }
    }
}
