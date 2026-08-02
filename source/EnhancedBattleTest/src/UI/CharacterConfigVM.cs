using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest.UI
{
    public abstract class CharacterConfigVM : ViewModel
    {
        private const float MinPreviewZoom = 1f;
        private const float MaxPreviewZoom = 2f;
        private const float PreviewZoomStep = 0.2f;

        private float _previewZoom = MinPreviewZoom;

        [DataSourceProperty]
        public float PreviewZoom
        {
            get => _previewZoom;
            private set
            {
                value = MBMath.ClampFloat(
                    value,
                    MinPreviewZoom,
                    MaxPreviewZoom);
                if (MBMath.ApproximatelyEqualsTo(_previewZoom, value))
                    return;

                _previewZoom = value;
                OnPropertyChangedWithValue(value, nameof(PreviewZoom));
                OnPropertyChanged(nameof(CanZoomIn));
                OnPropertyChanged(nameof(CanZoomOut));
                OnPreviewZoomChanged(value);
            }
        }

        [DataSourceProperty]
        public bool CanZoomIn => PreviewZoom < MaxPreviewZoom;

        [DataSourceProperty]
        public bool CanZoomOut => PreviewZoom > MinPreviewZoom;

        public abstract void SetConfig(
            PartyConfig partyConfig,
            CharacterConfig config,
            bool isAttacker,
            BasicCharacterObject preferredBannerCharacter,
            bool useSelectedCharacterForBanner);
        public abstract void SelectedCharacterChanged(Character character);

        protected virtual void OnPreviewZoomChanged(float value)
        {
        }

        public static CharacterConfigVM Create()
        {
            return new SPCharacterConfigVM();
        }

        public void ZoomIn()
        {
            PreviewZoom += PreviewZoomStep;
        }

        public void ZoomOut()
        {
            PreviewZoom -= PreviewZoomStep;
        }
    }
}
