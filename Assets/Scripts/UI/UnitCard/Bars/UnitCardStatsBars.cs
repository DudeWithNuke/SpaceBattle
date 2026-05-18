using PlaceableObject.Ships;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UnitCardStatsBars : MonoBehaviour
    {
        [SerializeField] private Image accuracyBar;
        [SerializeField] private Image stealthBar;
        [SerializeField] private Image detectingBar;
        [SerializeField] private Image resistanceBar;
        
        public void SetValues(float accuracy, float stealth, float detecting, float resistance)
        {
            SetFillAmount(accuracyBar, accuracy);
            SetFillAmount(stealthBar, stealth);
            SetFillAmount(detectingBar, detecting);
            SetFillAmount(resistanceBar, resistance);
        }

        public void SetFromShip(Ship ship)
        {
            if (!ship)
                return;

            SetValues(ship.Accuracy, ship.Stealth, ship.Detection, ship.Resistance);
        }

        private static void SetFillAmount(Image bar, float value)
        {
            if (!bar)
                return;

            bar.fillAmount = Mathf.Clamp01(value);
        }
    }
}
