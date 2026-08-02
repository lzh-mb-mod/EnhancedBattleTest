using EnhancedBattleTest.UI.Tableaus;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.TwoDimension;
using GauntletTexture = TaleWorlds.TwoDimension.Texture;
using NativeTexture = TaleWorlds.Engine.Texture;

namespace EnhancedBattleTest.UI
{
    public sealed class EnhancedBattleTestCharacterTableauTextureProvider
        : TextureProvider
    {
        private readonly EnhancedBattleTestCharacterTableau _tableau =
            new EnhancedBattleTestCharacterTableau();

        private NativeTexture _texture;
        private GauntletTexture _providedTexture;
        private bool _isHidden;

        public float CustomAnimationProgressRatio =>
            _tableau.GetCustomAnimationProgressRatio();

        public string BannerCodeText
        {
            set => _tableau.SetBannerCode(value);
        }

        public string BodyProperties
        {
            set => _tableau.SetBodyProperties(value);
        }

        public int StanceIndex
        {
            set => _tableau.SetStanceIndex(value);
        }

        public bool IsFemale
        {
            set => _tableau.SetIsFemale(value);
        }

        public int Race
        {
            set => _tableau.SetRace(value);
        }

        public bool IsBannerShownInBackground
        {
            set => _tableau.SetIsBannerShownInBackground(value);
        }

        public bool IsEquipmentAnimActive
        {
            set => _tableau.SetIsEquipmentAnimActive(value);
        }

        public string EquipmentCode
        {
            set => _tableau.SetEquipmentCode(value);
        }

        public string IdleAction
        {
            set => _tableau.SetIdleAction(value);
        }

        public string IdleFaceAnim
        {
            set => _tableau.SetIdleFaceAnim(value);
        }

        public bool CurrentlyRotating
        {
            set => _tableau.SetIsDragging(value);
        }

        public string MountCreationKey
        {
            set => _tableau.SetMountCreationKey(value);
        }

        public uint ArmorColor1
        {
            set => _tableau.SetArmorColor1(value);
        }

        public uint ArmorColor2
        {
            set => _tableau.SetArmorColor2(value);
        }

        public string CharStringId
        {
            set => _tableau.SetCharStringID(value);
        }

        public bool TriggerCharacterMountPlacesSwap
        {
            set => _tableau.TriggerCharacterMountPlacesSwap();
        }

        public float CustomRenderScale
        {
            set => _tableau.SetCustomRenderScale(value);
        }

        public float Zoom
        {
            set => _tableau.SetZoom(value);
        }

        public bool IsPlayingCustomAnimations
        {
            get => _tableau.IsRunningCustomAnimation;
            set
            {
                if (value)
                    _tableau.StartCustomAnimation();
                else
                    _tableau.StopCustomAnimation();
            }
        }

        public bool ShouldLoopCustomAnimation
        {
            get => _tableau.ShouldLoopCustomAnimation;
            set => _tableau.ShouldLoopCustomAnimation = value;
        }

        public int LeftHandWieldedEquipmentIndex
        {
            set => _tableau.SetLeftHandWieldedEquipmentIndex(value);
        }

        public int RightHandWieldedEquipmentIndex
        {
            set => _tableau.SetRightHandWieldedEquipmentIndex(value);
        }

        public float CustomAnimationWaitDuration
        {
            set => _tableau.CustomAnimationWaitDuration = value;
        }

        public string CustomAnimation
        {
            set => _tableau.SetCustomAnimation(value);
        }

        public bool IsTableauEnabled
        {
            set => _tableau.SetEnabled(value);
        }

        public bool IsHidden
        {
            get => _isHidden;
            set => _isHidden = value;
        }

        public override void Clear(bool clearNextFrame)
        {
            _tableau.OnFinalize();
            base.Clear(clearNextFrame);
        }

        protected override GauntletTexture OnGetTextureForRender(
            TwoDimensionContext twoDimensionContext,
            string name)
        {
            CheckTexture();
            return _providedTexture;
        }

        public override void SetTargetSize(int width, int height)
        {
            base.SetTargetSize(width, height);
            _tableau.SetTargetSize(width, height);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            CheckTexture();
            _tableau.OnTick(dt);
        }

        private void CheckTexture()
        {
            if ((NativeObject)_texture == (NativeObject)_tableau.Texture)
                return;

            _texture = _tableau.Texture;
            _providedTexture =
                (NativeObject)_texture != (NativeObject)null
                    ? new GauntletTexture(new EngineTexture(_texture))
                    : null;
        }
    }
}
