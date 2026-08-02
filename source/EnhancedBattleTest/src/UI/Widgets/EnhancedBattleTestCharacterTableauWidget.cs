using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;

namespace EnhancedBattleTest.UI.Widgets
{
    public sealed class EnhancedBattleTestCharacterTableauWidget
        : CharacterTableauWidget
    {
        private float _zoom = 1f;

        [Editor(false)]
        public float Zoom
        {
            get => _zoom;
            set
            {
                value = Math.Max(1f, Math.Min(2f, value));
                if (MBMath.ApproximatelyEqualsTo(_zoom, value))
                    return;

                _zoom = value;
                OnPropertyChanged(value, nameof(Zoom));
                SetTextureProviderProperty(nameof(Zoom), value);
            }
        }

        public EnhancedBattleTestCharacterTableauWidget(UIContext context)
            : base(context)
        {
            TextureProviderName =
                nameof(EnhancedBattleTestCharacterTableauTextureProvider);
        }

        protected override bool OnPreviewMouseScroll()
        {
            float scrollDelta = EventManager.DeltaMouseScroll;
            if (scrollDelta > 0.001f)
                EventFired("ZoomIn");
            else if (scrollDelta < -0.001f)
                EventFired("ZoomOut");

            return true;
        }
    }
}
