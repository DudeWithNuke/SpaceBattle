using Network.Match;
using Network.Player;

namespace Network.Turn
{
    public readonly struct NetworkTurnInteractionState
    {
        public readonly bool CanInteract;
        public readonly bool IsDeploymentPhase;
        public readonly bool IsBattlePhase;
        public readonly bool ShouldForceReturnPickedObject;

        public NetworkTurnInteractionState(
            bool canInteract,
            bool isDeploymentPhase,
            bool isBattlePhase,
            bool shouldForceReturnPickedObject)
        {
            CanInteract = canInteract;
            IsDeploymentPhase = isDeploymentPhase;
            IsBattlePhase = isBattlePhase;
            ShouldForceReturnPickedObject = shouldForceReturnPickedObject;
        }
    }

    public sealed class TurnInputGateService
    {
        private readonly PlayerContext _playerContext;

        private bool _hasMatchState;
        private int _activePlayerIndex;
        private MatchPhase _phase;

        public TurnInputGateService(PlayerContext playerContext)
        {
            _playerContext = playerContext;
        }

        public NetworkTurnInteractionState ApplyMatchState(MatchStateDto state)
        {
            var previousActivePlayerIndex = _activePlayerIndex;
            var previousPhase = _phase;
            _hasMatchState = state.phase is MatchPhase.Deployment or MatchPhase.Battle;
            _phase = state.phase;
            _activePlayerIndex = state.activePlayerIndex;

            return CreateInteractionState(ShouldForceReturnPickedObject(
                state,
                previousPhase,
                previousActivePlayerIndex));
        }

        public NetworkTurnInteractionState CreateCurrentState()
        {
            return CreateInteractionState(false);
        }

        private NetworkTurnInteractionState CreateInteractionState(bool shouldForceReturnPickedObject)
        {
            var isDeploymentPhase = _phase == MatchPhase.Deployment;
            var isBattlePhase = _phase == MatchPhase.Battle;
            var canInteract = _hasMatchState &&
                              _playerContext.HasAssignedPlayer &&
                              (isDeploymentPhase || _playerContext.LocalPlayerIndex == _activePlayerIndex);

            return new NetworkTurnInteractionState(
                canInteract,
                isDeploymentPhase,
                isBattlePhase,
                shouldForceReturnPickedObject);
        }

        private bool ShouldForceReturnPickedObject(
            MatchStateDto state,
            MatchPhase previousPhase,
            int previousActivePlayerIndex)
        {
            if (previousPhase == MatchPhase.Deployment &&
                state.phase == MatchPhase.Battle)
                return true;

            return state.status == MatchStatus.TimerExpired &&
                   _playerContext.LocalPlayerIndex == previousActivePlayerIndex &&
                   previousActivePlayerIndex != state.activePlayerIndex;
        }
    }
}
