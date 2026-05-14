namespace PlaceableObject.Events
{
    public readonly struct TurnStartedEvent
    {
        public readonly int TurnNumber;

        public TurnStartedEvent(int turnNumber)
        {
            TurnNumber = turnNumber;
        }
    }
}
