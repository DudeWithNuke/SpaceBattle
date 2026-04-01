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

        private void SyncValue(bool value)
        {
            _animator.SetBool(isOnParameter, value);
            OnValueChanged?.Invoke(value);
        }
    }
}
