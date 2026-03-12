using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaceableObject
{
    [Serializable]
    public struct PlaceableObjectStatView
    {
        [SerializeField] private string label;
        [SerializeField] private string value;

        public string Label => label;
        public string Value => value;
    }

    [Serializable]
    public struct PlaceableObjectActionView
    {
        [SerializeField] private string actionId;
        [SerializeField] private string displayName;

        public string ActionId => actionId;
        public string DisplayName => displayName;
    }

    public class PlaceableObjectUiProfile : MonoBehaviour
    {
        [SerializeField] private Sprite icon;
        [SerializeField] private string displayName;
        [SerializeField] private List<PlaceableObjectStatView> stats = new();
        [SerializeField] private List<PlaceableObjectActionView> actions = new();

        public Sprite Icon => icon;
        public string DisplayName => displayName;
        public IReadOnlyList<PlaceableObjectStatView> Stats => stats;
        public IReadOnlyList<PlaceableObjectActionView> Actions => actions;
    }
}
