using System;
using UnityEngine;

namespace CameraControls
{
    internal class BattlefieldSwitchController
    {
        public bool IsPlayerBattlefieldActive { get; private set; } = true;
        public bool IsSwitching { get; private set; }

        private readonly Vector3 _battlefieldOffset;
        private Vector3 _targetFocusPoint;

        public BattlefieldSwitchController(Vector3 playerOrigin, Vector3 enemyOrigin)
        {
            _battlefieldOffset = enemyOrigin - playerOrigin;
        }

        public void StartSwitch(Vector3 currentFocus, Func<Vector3, Vector3> clampToBounds)
        {
            IsPlayerBattlefieldActive = !IsPlayerBattlefieldActive;

            var shift = IsPlayerBattlefieldActive ? -_battlefieldOffset : _battlefieldOffset;
            _targetFocusPoint = clampToBounds(currentFocus + shift);
            IsSwitching = true;
        }

        public void UpdateTransition(ref Vector3 focusPoint, float moveSpeed, float deltaTime)
        {
            focusPoint = Vector3.MoveTowards(focusPoint, _targetFocusPoint, moveSpeed * deltaTime);

            if (Vector3.Distance(focusPoint, _targetFocusPoint) >= 0.01f)
                return;

            focusPoint = _targetFocusPoint;
            IsSwitching = false;
        }
    }
}
