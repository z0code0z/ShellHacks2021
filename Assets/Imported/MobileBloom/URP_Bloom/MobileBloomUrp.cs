namespace UnityEngine.Rendering.Universal
{
    public class MobileBloomUrp : ScriptableRendererFeature
    {
        [System.Serializable]
        public class MobileBloomSettings
        {
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;

            public Material blitMaterial = null;

            [Range(0, 2)]
            public float BloomDiffusion = 1f;

            public Color BloomColor = Color.white;

            [Range(0, 10)]
            public float BloomAmount = 1f;

            [Range(0, 5)]
            public float BloomThreshold = 0.0f;

            [Range(0, 1)]
            public float BloomSoftness = 0.0f;
        }

        public MobileBloomSettings settings = new MobileBloomSettings();

        MobileBloomUrpPass mobileBloomUrpPass;

        public override void Create()
        {
            mobileBloomUrpPass = new MobileBloomUrpPass(settings.Event, settings.blitMaterial, settings.BloomDiffusion, settings.BloomColor, settings.BloomAmount, settings.BloomThreshold, settings.BloomSoftness, this.name);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            mobileBloomUrpPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(mobileBloomUrpPass);
        }
    }
}

