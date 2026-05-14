using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Animation
{
    public class ListSwitch : MonoBehaviour
    {
        public event Action<bool> OnValueChanged;

        private Toggle _toggle;
        private Animator _animator;

        [SerializeField] private string isOnParameter = "IsOn";

        public bool IsOn => _toggle != null && _toggle.isOn;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _animator = GetComponent<Animator>();

            _toggle.onValueChanged.AddListener(SyncValue);

            SyncValue(_toggle.isOn);
        }

        public void SetInteractable(bool interactable)
        {
            if (!_toggle)
                return;

            _toggle.interactable = interactable;
        }

        public void SetValue(bool value, bool notify)
        {
            if (!_toggle)
                return;

            if (notify)
            {
                _toggle.isOn = value;
                return;
            }

            _toggle.SetIsOnWithoutNotify(value);
            if (_animator)
                _animator.SetBool(isOnParameter, value);
        }

        private void SyncValue(bool value)
        {
            _animator.SetBool(isOnParameter, value);
            OnValueChanged?.Invoke(value);
        }
    }
}
