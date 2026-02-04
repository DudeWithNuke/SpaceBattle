using UnityEngine;
using Utils;

namespace GameBoard
{
    public enum CellState
    {
        DisabledLayer,
        ActiveLayer,
        Hovered,
        Selected,
        HoveredSelected,
        EmptyAttacked,
        ShipAttacked
    }

    public class CellStateController
    {
        private readonly Renderer _renderer;
        public CellState CurrentState { get; private set; }
        public CellState PreviousState { get; private set; }

        public CellStateController(Renderer renderer)
        {
            _renderer = renderer;
            CurrentState = CellState.DisabledLayer;
        }

        private static Color GetColor(CellState state)
        {
            return state switch
            {
                CellState.DisabledLayer => Color.darkCyan,
                CellState.ActiveLayer => Color.cyan,
                CellState.Hovered => Color.greenYellow,
                CellState.Selected => Color.green,
                CellState.HoveredSelected => Color.gold,
                CellState.EmptyAttacked => Color.darkMagenta,
                CellState.ShipAttacked => Color.red,
                _ => Color.grey
            };
        }

        public void SetState(CellState newState)
        {
            if (!IsValidTransition(CurrentState, newState))
            {
                Log.Warn($"Invalid state transition from {CurrentState} to {newState}");
                return;
            }

            PreviousState = CurrentState;
            CurrentState = newState;
            if (_renderer && _renderer.material)
                _renderer.material.color = GetColor(newState);
        }

        private static bool IsValidTransition(CellState from, CellState to)
        {
            if (from == to) 
                return false;

            return (from, to) switch
            {
                (CellState.DisabledLayer, CellState.ActiveLayer) => true,
                (CellState.DisabledLayer, CellState.Hovered) => true,
                (CellState.ActiveLayer, CellState.DisabledLayer) => true,
                (CellState.ActiveLayer, CellState.Hovered) => true,
                (CellState.Hovered, CellState.ActiveLayer) => true,
                (CellState.Hovered, CellState.DisabledLayer) => true,
                (CellState.Hovered, CellState.Selected) => true,
                (CellState.Selected, CellState.HoveredSelected) => true,
                (CellState.HoveredSelected, CellState.Selected) => true,
                _ => false
            };
        }
        
        public void RestorePreviousState()
        {
            SetState(PreviousState);
        }
    }
}
