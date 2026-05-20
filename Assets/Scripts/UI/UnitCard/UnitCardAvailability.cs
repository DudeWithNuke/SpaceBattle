namespace UI.UnitCard
{
    public readonly struct UnitCardAvailability
    {
        public readonly bool BlocksRaycasts;
        public readonly bool CanUseMainButton;
        public readonly bool CanUseActions;

        public UnitCardAvailability(bool blocksRaycasts, bool canUseMainButton, bool canUseActions)
        {
            BlocksRaycasts = blocksRaycasts;
            CanUseMainButton = canUseMainButton;
            CanUseActions = canUseActions;
        }
    }
}
