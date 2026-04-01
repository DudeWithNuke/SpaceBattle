using System.Collections.Generic;
using PlaceableObject;
using PlaceableObject.Ships;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UnitCardVitalityBars : MonoBehaviour
    {
        [SerializeField] private GameObject healthStackRoot;
        [SerializeField] private GameObject healthExtraStackRoot;
        [SerializeField] private GameObject energyStackRoot;
        [SerializeField] private GameObject energyExtraStackRoot;

        private readonly List<GameObject> _healthPoints = new();
        private readonly List<GameObject> _healthExtraPoints = new();
        private readonly List<GameObject> _energyPoints = new();
        private readonly List<GameObject> _energyExtraPoints = new();

        private void Awake()
        {
            CachePoints(healthStackRoot != null ? healthStackRoot.transform : null, _healthPoints);
            CachePoints(healthExtraStackRoot != null ? healthExtraStackRoot.transform : null, _healthExtraPoints);
            CachePoints(energyStackRoot != null ? energyStackRoot.transform : null, _energyPoints);
            CachePoints(energyExtraStackRoot != null ? energyExtraStackRoot.transform : null, _energyExtraPoints);
        }

        public void SetFromShip(Ship ship)
        {
            if (!ship)
                return;

            ship.EnsureShapeInitialized();
            var healthCount = ship.Shape != null ? ship.Shape.GetIntactCellsCount() : 0;
            SetHealthPoints(healthCount);
        }

        public void SetHealthPoints(int count)
        {
            ApplyPoints(_healthPoints, _healthExtraPoints, count);
        }

        public void SetEnergyPoints(int count)
        {
            ApplyPoints(_energyPoints, _energyExtraPoints, count);
        }

        private static void CachePoints(Transform root, List<GameObject> points)
        {
            points.Clear();
            if (root == null)
                return;

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.GetComponent<Image>() == null)
                    continue;

                points.Add(child.gameObject);
            }
        }

        private static void ApplyPoints(List<GameObject> stackPoints, List<GameObject> extraPoints, int count)
        {
            if (count < 0)
                count = 0;

            var baseCount = stackPoints.Count;
            var extraCount = extraPoints.Count;
            var clamped = Mathf.Min(count, baseCount + extraCount);
            var extraNeeded = Mathf.Max(0, clamped - baseCount);

            for (var i = 0; i < baseCount; i++)
                stackPoints[i].SetActive(i < clamped);

            for (var i = 0; i < extraCount; i++)
                extraPoints[i].SetActive(i < extraNeeded);
        }
    }
}
