using UnityEngine;

namespace GameBoard
{
    public static class CellVisualManager
    {
        private static readonly Color DisabledColor = Color.black;
        private static readonly Color ActiveColor = Color.white;
        private static readonly Color HoveredColor = Color.yellow;
        private static readonly Color SelectedColor = Color.green;
        private static readonly Color HoveredSelectedColor = Color.cyan;

        public static Color GetColorForState(CellState state)
        {
            return state switch
            {
                CellState.DisabledLayer => DisabledColor,
                CellState.ActiveLayer => ActiveColor,
                CellState.Hovered => HoveredColor,
                CellState.Selected => SelectedColor,
                CellState.HoveredSelected => HoveredSelectedColor,
                _ => Color.white
            };
        }

        public static void ApplyVisualState(Renderer renderer, CellState state)
        {
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = GetColorForState(state);
            }
        }
    }
}
