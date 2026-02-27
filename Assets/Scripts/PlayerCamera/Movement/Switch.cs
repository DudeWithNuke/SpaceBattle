using System;
using UnityEngine;

namespace PlayerCamera.Movement
{
    internal class Switch
    {
        private readonly CameraSettings _cameraSettings;
        private readonly Vector3 _battlefieldOffset;
        
        public Switch(CameraSettings cameraSettings, Vector3 playerOrigin, Vector3 enemyOrigin)
        {
            _cameraSettings = cameraSettings;
            _battlefieldOffset = enemyOrigin - playerOrigin;
        }

        public void StartSwitch(CameraState state, Func<Vector3, Vector3> clampToBounds)
        {
            state.IsPlayerBattlefieldActive = !state.IsPlayerBattlefieldActive;

            var shift = state.IsPlayerBattlefieldActive ? -_battlefieldOffset : _battlefieldOffset;
            state.TargetFocusPoint = clampToBounds(state.FocusPoint + shift);
            state.IsSwitching = true;
        }

        public void UpdateTransition(CameraState state, float deltaTime)
        {
            state.FocusPoint = Vector3.MoveTowards(state.FocusPoint, state.TargetFocusPoint, _cameraSettings.switchFocusSpeed * deltaTime);

            if (Vector3.Distance(state.FocusPoint, state.TargetFocusPoint) >= 0.01f)
                return;

            state.FocusPoint = state.TargetFocusPoint;
            state.IsSwitching = false;
        }
    }
}
