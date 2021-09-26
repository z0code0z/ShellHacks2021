using UnityEngine.XR;

namespace UnityEngine.Rendering.Universal
{
    internal class MobileBloomUrpPass : ScriptableRenderPass
    {
        public Material material;

        private RenderTargetIdentifier source;
        private RenderTargetIdentifier bloomTemp = new RenderTargetIdentifier(bloomTempString);
        private RenderTargetIdentifier bloomTemp1 = new RenderTargetIdentifier(bloomTemp1String);
        private RenderTargetIdentifier bloomTex = new RenderTargetIdentifier(bloomTexString);
        private RenderTargetIdentifier tempCopy = new RenderTargetIdentifier(tempCopyString);

        private readonly string tag;
        private readonly float bloomDiffusion;
        private readonly Color bloomColor;
        private readonly float bloomAmount;
        private readonly float bloomThreshold;
        private readonly float bloomSoftness;
        private int numberOfPasses;
        private float knee;

        static readonly int blurAmountString = Shader.PropertyToID("_BlurAmount");
        static readonly int bloomColorString = Shader.PropertyToID("_BloomColor");
        static readonly int blDataString = Shader.PropertyToID("_BloomData");

        static readonly int bloomTempString = Shader.PropertyToID("_BlurTemp");
        static readonly int bloomTemp1String = Shader.PropertyToID("_BlurTemp2");
        static readonly int bloomTexString = Shader.PropertyToID("_BlurTex");
        static readonly int tempCopyString = Shader.PropertyToID("_TempCopy");

        RenderTextureDescriptor opaqueDesc, half, quarter, eighths, sixths;

        public MobileBloomUrpPass(RenderPassEvent renderPassEvent, Material material,
            float bloomDiffusion, Color bloomColor, float bloomAmount, float bloomThreshold, float bloomSoftness, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            this.tag = tag;
            this.material = material;

            this.bloomDiffusion = bloomDiffusion;
            this.bloomColor = bloomColor;
            this.bloomAmount = bloomAmount;
            this.bloomThreshold = bloomThreshold;
            this.bloomSoftness = bloomSoftness;
        }

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (bloomDiffusion == 0 && bloomAmount == 0)
            {
                return;
            }

            if (XRSettings.enabled)
            {
                opaqueDesc = XRSettings.eyeTextureDesc;
                half = XRSettings.eyeTextureDesc;
                half.height /= 2; half.width /= 2;
                quarter = XRSettings.eyeTextureDesc;
                quarter.height /= 4; quarter.width /= 4;
                eighths = XRSettings.eyeTextureDesc;
                eighths.height /= 8; eighths.width /= 8;
                sixths = XRSettings.eyeTextureDesc;
                sixths.height /= XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePass ? 8 : 16; sixths.width /= XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePass ? 8 : 16;
            }
            else
            {
                opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                half = new RenderTextureDescriptor(opaqueDesc.width / 2, opaqueDesc.height / 2);
                quarter = new RenderTextureDescriptor(opaqueDesc.width / 4, opaqueDesc.height / 4);
                eighths = new RenderTextureDescriptor(opaqueDesc.width / 8, opaqueDesc.height / 8);
                sixths = new RenderTextureDescriptor(opaqueDesc.width / 16, opaqueDesc.height / 16);
                opaqueDesc.depthBufferBits = 0;
                half.depthBufferBits = 0;
                quarter.depthBufferBits = 0;
                eighths.depthBufferBits = 0;
                sixths.depthBufferBits = 0;
            }

            CommandBuffer cmd = CommandBufferPool.Get(tag);
            cmd.GetTemporaryRT(tempCopyString, opaqueDesc, FilterMode.Bilinear);
            cmd.Blit(source, tempCopy);

            material.SetFloat(blurAmountString, bloomDiffusion);
            material.SetColor(bloomColorString, bloomAmount * bloomColor);
            knee = bloomThreshold * bloomSoftness;
            material.SetVector(blDataString, new Vector4(bloomThreshold, bloomThreshold - knee, 2f * knee, 1f / (4f * knee + 0.00001f)));
            numberOfPasses = Mathf.Clamp(Mathf.CeilToInt(bloomDiffusion * 4), 1, 4);
            material.SetFloat(blurAmountString, numberOfPasses > 1 ? bloomDiffusion > 1 ? bloomDiffusion : (bloomDiffusion * 4 - Mathf.FloorToInt(bloomDiffusion * 4 - 0.001f)) * 0.5f + 0.5f : bloomDiffusion * 4);

            if (numberOfPasses == 1 || bloomDiffusion == 0)
            {
                cmd.GetTemporaryRT(bloomTexString, half, FilterMode.Bilinear);
                cmd.Blit(tempCopy, bloomTex, material, 0);
            }
            else if (numberOfPasses == 2)
            {
                cmd.GetTemporaryRT(bloomTexString, half, FilterMode.Bilinear);
                cmd.GetTemporaryRT(bloomTempString, quarter, FilterMode.Bilinear);
                cmd.Blit(tempCopy, bloomTemp, material, 0);
                cmd.Blit(bloomTemp, bloomTex, material, 1);
            }
            else if (numberOfPasses == 3)
            {
                cmd.GetTemporaryRT(bloomTexString, quarter, FilterMode.Bilinear);
                cmd.GetTemporaryRT(bloomTempString, eighths, FilterMode.Bilinear);
                cmd.Blit(tempCopy, bloomTex, material, 0);
                cmd.Blit(bloomTex, bloomTemp, material, 1);
                cmd.Blit(bloomTemp, bloomTex, material, 1);
            }
            else if (numberOfPasses == 4)
            {
                cmd.GetTemporaryRT(bloomTexString, quarter, FilterMode.Bilinear);
                cmd.GetTemporaryRT(bloomTempString, eighths, FilterMode.Bilinear);
                cmd.GetTemporaryRT(bloomTemp1String, sixths, FilterMode.Bilinear);
                cmd.Blit(tempCopy, bloomTex, material, 0);
                cmd.Blit(bloomTex, bloomTemp, material, 1);
                cmd.Blit(bloomTemp, bloomTemp1, material, 1);
                cmd.Blit(bloomTemp1, bloomTemp, material, 1);
                cmd.Blit(bloomTemp, bloomTex, material, 1);
            }

            cmd.SetGlobalTexture(bloomTexString, bloomTex);
            cmd.Blit(tempCopy, source, material, 2);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempCopyString);
            cmd.ReleaseTemporaryRT(bloomTempString);
            cmd.ReleaseTemporaryRT(bloomTemp1String);
            cmd.ReleaseTemporaryRT(bloomTexString);
        }
    }
}
