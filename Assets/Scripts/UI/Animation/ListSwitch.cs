using UnityEngine;
using UnityEngine.UI;

namespace UI.Animation
{
    [RequireComponent(typeof(Toggle), typeof(Animator))]
    public class ListSwitch : MonoBehaviour
    {
        private Toggle _toggle;
        private Animator _animator;
    
        [SerializeField] private string isOnParameter = "IsOn";

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _animator = GetComponent<Animator>();
            
            _toggle.onValueChanged.AddListener(SyncValue);
        
            SyncValue(_toggle.isOn);
        }

        private void SyncValue(bool value)
        {
            _animator.SetBool(isOnParameter, value);
        }
    }
}