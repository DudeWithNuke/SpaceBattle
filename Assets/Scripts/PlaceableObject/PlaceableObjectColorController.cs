using System.Collections.Generic;
using UnityEngine;

namespace PlaceableObject
{
    public enum PlaceableObjectVisualState
    {
        Default,
        InvalidPlacement
    }

    public class PlaceableObjectColorController
    {
        private readonly Material[] _materials;
        private readonly Dictionary<PlaceableObjectVisualState, Color> _palette;
        private PlaceableObjectVisualState _currentState;

        public PlaceableObjectColorController(Renderer[] renderers)
        {
            var materials = new List<Material>();
            foreach (var renderer in renderers)
            {
                if (!renderer)
                    continue;

                var rendererMaterials = renderer.materials;
                foreach (var material in rendererMaterials)
                {
                    if (material)
                        materials.Add(material);
                }
            }

            _materials = materials.ToArray();
            _palette = new Dictionary<PlaceableObjectVisualState, Color>
            {
                { PlaceableObjectVisualState.Default, PlaceableObjectConfig.DefaultColor },
                { PlaceableObjectVisualState.InvalidPlacement, PlaceableObjectConfig.InvalidPlacementColor }
            };

            _currentState = PlaceableObjectVisualState.Default;
            ApplyColor(_palette[_currentState]);
        }

        public void SetState(PlaceableObjectVisualState state)
        {
            if (_currentState == state)
                return;

            _currentState = state;
            if (_palette.TryGetValue(state, out var color))
                ApplyColor(color);
        }

        private void ApplyColor(Color color)
        {
            foreach (var material in _materials)
                material.color = color;
        }
    }
}
