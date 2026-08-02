using System;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace EnhancedBattleTest.UI
{
    public sealed class CharacterPreviewVM : CharacterViewModel
    {
        private readonly Action _zoomIn;
        private readonly Action _zoomOut;
        private float _previewZoom = 1f;

        [DataSourceProperty]
        public float PreviewZoom
        {
            get => _previewZoom;
            set
            {
                if (MBMath.ApproximatelyEqualsTo(_previewZoom, value))
                    return;

                _previewZoom = value;
                OnPropertyChangedWithValue(value, nameof(PreviewZoom));
            }
        }

        public CharacterPreviewVM(
            StanceTypes stance,
            Action zoomIn,
            Action zoomOut)
            : base(stance)
        {
            _zoomIn = zoomIn;
            _zoomOut = zoomOut;
        }

        public void ZoomIn()
        {
            _zoomIn();
        }

        public void ZoomOut()
        {
            _zoomOut();
        }
    }
}
