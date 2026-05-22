using System;
using Network.Match;
using Utils;

namespace Network.Turn
{
    public sealed class TurnSubmitService
    {
        private readonly Func<MatchPhase> _getCurrentPhase;
        private readonly Func<int> _getCurrentTurnNumber;
        private readonly Func<bool> _hasPickedObject;
        private readonly Action<MatchPhase, int> _submitLocalFieldSnapshot;
        private readonly Action _requestEndTurn;

        public TurnSubmitService(
            Func<MatchPhase> getCurrentPhase,
            Func<int> getCurrentTurnNumber,
            Func<bool> hasPickedObject,
            Action<MatchPhase, int> submitLocalFieldSnapshot,
            Action requestEndTurn)
        {
            _getCurrentPhase = getCurrentPhase;
            _getCurrentTurnNumber = getCurrentTurnNumber;
            _hasPickedObject = hasPickedObject;
            _submitLocalFieldSnapshot = submitLocalFieldSnapshot;
            _requestEndTurn = requestEndTurn;
        }

        public void SubmitTurn()
        {
            var phase = _getCurrentPhase();
            if (phase != MatchPhase.Battle)
                return;

            if (_hasPickedObject())
            {
                Log.Warn("[NetworkTurnSubmitController] Submit rejected. A picked object must be placed or cancelled first.");
                return;
            }

            _submitLocalFieldSnapshot(phase, _getCurrentTurnNumber());
            _requestEndTurn();
        }
    }
}
