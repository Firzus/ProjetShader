using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignettePostProcessRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class VignetteSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Material vignetteMaterial = null;
    }

    public VignetteSettings settings = new VignetteSettings();
    private VignetteRenderPass vignetteRenderPass;
    private RenderTargetHandle tempTexture;

    class VignetteRenderPass : ScriptableRenderPass
    {
        private Material vignetteMaterial;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempRenderTarget;

        public VignetteRenderPass(Material material)
        {
            vignetteMaterial = material;
            tempRenderTarget.Init("_TempVignetteTarget");
        }

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (vignetteMaterial == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("VignettePostProcess");

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            cmd.GetTemporaryRT(tempRenderTarget.id, descriptor, FilterMode.Bilinear);

            Blit(cmd, source, tempRenderTarget.Identifier());

            Blit(cmd, tempRenderTarget.Identifier(), source, vignetteMaterial);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempRenderTarget.id);
        }
    }

    public override void Create()
    {
        vignetteRenderPass = new VignetteRenderPass(settings.vignetteMaterial);
        vignetteRenderPass.renderPassEvent = settings.renderPassEvent;
        tempTexture.Init("_TemporaryColorTexture");

    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.vignetteMaterial == null)
            return;

        renderer.EnqueuePass(vignetteRenderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {

        var cameraColorTargetIdent = renderer.cameraColorTarget;
        vignetteRenderPass.Setup(cameraColorTargetIdent);
    }

}