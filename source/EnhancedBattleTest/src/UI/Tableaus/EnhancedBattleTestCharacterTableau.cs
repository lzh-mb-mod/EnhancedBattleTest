using System;
using System.Reflection;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace EnhancedBattleTest.UI.Tableaus
{
    public sealed class EnhancedBattleTestCharacterTableau : CharacterTableau
    {
        private const float MinZoom = 1f;
        private const float MaxZoom = 2f;
        private const float BaseMaxCameraAdvance = 3f;
        private const float ZoomSmoothingSpeed = 10f;
        private const float BaseVerticalDragSpeed = 0.002f;
        private const float BaseMaxVerticalOffset = 1.5f;

        private static readonly FieldInfo CameraFrameField =
            typeof(CharacterTableau).GetField(
                "_camPos",
                BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(
                typeof(CharacterTableau).FullName,
                "_camPos");

        private static readonly FieldInfo AgentVisualsField =
            typeof(CharacterTableau).GetField(
                "_agentVisuals",
                BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(
                typeof(CharacterTableau).FullName,
                "_agentVisuals");

        private MatrixFrame _baseCameraFrame;
        private bool _hasBaseCameraFrame;
        private bool _isDragging;
        private float _verticalOffset;
        private float _currentZoom = MinZoom;
        private float _zoom = MinZoom;

        public void SetZoom(float value)
        {
            _zoom = MBMath.ClampFloat(value, MinZoom, MaxZoom);
        }

        public void SetIsDragging(bool value)
        {
            _isDragging = value;
            RotateCharacter(value);
        }

        public new void SetTargetSize(int width, int height)
        {
            RestoreBaseCameraFrame();
            base.SetTargetSize(width, height);
            if (_hasBaseCameraFrame)
            {
                _baseCameraFrame =
                    (MatrixFrame)CameraFrameField.GetValue(this);
            }
        }

        public new void OnFinalize()
        {
            RestoreBaseCameraFrame();
            base.OnFinalize();
        }

        public new void OnTick(float dt)
        {
            RestoreBaseCameraFrame();

            base.OnTick(dt);

            _baseCameraFrame =
                (MatrixFrame)CameraFrameField.GetValue(this);
            _hasBaseCameraFrame = true;

            float agentScale = GetAgentScale();
            float maxVerticalOffset =
                BaseMaxVerticalOffset * agentScale;
            if (_isDragging)
            {
                _verticalOffset +=
                    Input.MouseMoveY
                    * BaseVerticalDragSpeed
                    * agentScale;
            }
            _verticalOffset = MBMath.ClampFloat(
                _verticalOffset,
                -maxVerticalOffset,
                maxVerticalOffset);

            _currentZoom = MBMath.LerpFPSIndependent(
                _currentZoom,
                _zoom,
                dt * ZoomSmoothingSpeed);

            MatrixFrame zoomedCameraFrame = _baseCameraFrame;
            zoomedCameraFrame.origin.z += _verticalOffset;
            zoomedCameraFrame.Elevate(
                -(_currentZoom - MinZoom) / (MaxZoom - MinZoom)
                * BaseMaxCameraAdvance
                * agentScale);
            CameraFrameField.SetValue(this, zoomedCameraFrame);
        }

        private void RestoreBaseCameraFrame()
        {
            if (_hasBaseCameraFrame)
                CameraFrameField.SetValue(this, _baseCameraFrame);
        }

        private float GetAgentScale()
        {
            var agentVisuals =
                AgentVisualsField.GetValue(this) as AgentVisuals;
            float scale = agentVisuals?.GetScale() ?? 1f;
            return scale > 0f ? scale : 1f;
        }
    }
}
