using PlaceableObject.Ships;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UnitCard
{
    public class UnitCardView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private UnitCardStatsBars statsBars;
        [SerializeField] private UnitCardVitalityBars vitalityBars;

        public void SetShip(Ship ship)
        {
            SetIcon(ship);
            SetStatsBars(ship);
            SetVitalityBars(ship);
        }

        public void Clear()
        {
            iconImage.enabled = false;

            ClearBars();
        }

        private void SetIcon(Ship ship)
        {
            if (ship.Icon)
            {
                iconImage.enabled = true;
                iconImage.sprite = ship.Icon;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        private void SetStatsBars(Ship ship)
        {
            statsBars.SetFromShip(ship);
        }

        private void SetVitalityBars(Ship ship)
        {
            vitalityBars.SetFromShip(ship);
        }

        private void ClearBars()
        {
            statsBars.SetValues(0, 0, 0, 0);
            vitalityBars.SetHealthPoints(0);
            vitalityBars.SetEnergyPoints(0);
        }
    }
}
