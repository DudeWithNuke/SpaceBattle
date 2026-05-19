namespace Network
{
    public sealed class PlayerConnectionInfo
    {
        public ulong ClientId { get; }
        public int PlayerIndex { get; }
        public bool IsHost { get; }
        public bool HasSubmittedFleet { get; set; }
        public FleetPresetDto FleetPreset { get; set; }

        public PlayerConnectionInfo(ulong clientId, int playerIndex, bool isHost)
        {
            ClientId = clientId;
            PlayerIndex = playerIndex;
            IsHost = isHost;
        }
    }
}
