using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Animation
{
    [RequireComponent(typeof(Toggle), typeof(Animator))]
    public class SortToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Toggle _toggle;
        private Animator _animator;
        
        [SerializeField] private string isSelectedParameter = "IsSelected";
        [SerializeField] private string isHoveredParameter = "IsHovered";

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _animator = GetComponent<Animator>();

            _toggle.onValueChanged.AddListener(HandleToggleChange);
        
            _animator.SetBool(isSelectedParameter, _toggle.isOn);
        }

        private void HandleToggleChange(bool isOn)
        {
            _animator.SetBool(isSelectedParameter, isOn);
        }

        public void OnPointerEnter(PointerEventData eventData) 
        {
            _animator.SetBool(isHoveredParameter, true);
        }

        public void OnPointerExit(PointerEventData eventData) 
        {
            _animator.SetBool(isHoveredParameter, false);
        }
    }
}