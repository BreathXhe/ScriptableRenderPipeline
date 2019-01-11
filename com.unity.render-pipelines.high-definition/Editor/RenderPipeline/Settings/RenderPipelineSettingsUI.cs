using UnityEditor.AnimatedValues;
using UnityEditor.Rendering;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    using CED = CoreEditorDrawer<SerializedRenderPipelineSettings>;
    
    static class RenderPipelineSettingsUI
    {
        enum Expandable
        {
            SupportedFeature = 1 << 0
        }

        readonly static ExpandedState<Expandable, RenderPipelineSettings> k_ExpandedState = new ExpandedState<Expandable, RenderPipelineSettings>(Expandable.SupportedFeature, "HDRP");

        static readonly GUIContent k_SupportedFeatureHeaderContent = CoreEditorUtils.GetContent("Render Pipeline Supported Features");

        static readonly GUIContent k_SupportShadowMaskContent = CoreEditorUtils.GetContent("Shadow Mask|When enabled, HDRP allocates Shader variants and memory for processing shadow masks. This allows you to use shadow masks in your Unity Project.");
        static readonly GUIContent k_SupportSSRContent = CoreEditorUtils.GetContent("SSR|When enabled, HDRP allocates memory for processing screen space reflection (SSR). This allows you to use SSR in your Unity Project.");
        static readonly GUIContent k_SupportSSAOContent = CoreEditorUtils.GetContent("SSAO|When enabled, HDRP allocates memory for processing screen space ambient occlusion (SSAO). This allows you to use SSAO in your Unity Project.");
        static readonly GUIContent k_SupportedSSSContent = CoreEditorUtils.GetContent("Subsurface Scattering|When enabled, HDRP allocates memory for processing subsurface scattering (SSS). This allows you to use SSS in your Unity Project.");
        static readonly GUIContent k_SSSSampleCountContent = CoreEditorUtils.GetContent("High quality |When enabled, HDRP processes higher quality subsurface scattering effects. Warning: There is a high performance cost, do not enable on consoles.");
        static readonly GUIContent k_SupportVolumetricContent = CoreEditorUtils.GetContent("Volumetrics|When enabled, HDRP allocates Shader variants and memory for volumetric effects. This allows you to use volumetric lighting and fog in your Unity Project.");
        static readonly GUIContent k_VolumetricResolutionContent = CoreEditorUtils.GetContent("High quality |When enabled, HDRP increases the resolution of volumetric lighting buffers. Warning: There is a high performance cost, do not enable on consoles.");
        static readonly GUIContent k_SupportLightLayerContent = CoreEditorUtils.GetContent("LightLayers|When enabled, HDRP allocates memory for processing Light Layers. This allows you to use Light Layers in your Unity Project. For deferred rendering, this allocation includes an extra render target in memory and extra cost.");
        static readonly GUIContent k_SupportLitShaderModeContent = CoreEditorUtils.GetContent("Lit Shader Mode|Specifies the rendering modes HDRP supports for Lit Shaders. HDRP removes all allocated memory and Shader variants for modes you do not specify.");
        static readonly GUIContent k_MSAASampleCountContent = CoreEditorUtils.GetContent("MSAA Quality|Specifies the maximum quality HDRP supports for MSAA. Set Lit Shader Mode to Forward Only or Both to use this feature.");
        static readonly GUIContent k_SupportDecalContent = CoreEditorUtils.GetContent("Decals|When enabled, HDRP allocates Shader variants and memory to the decals buffer and cluster decal. This allows you to use decals in your Unity Project.");
        static readonly GUIContent k_SupportMotionVectorContent = CoreEditorUtils.GetContent("Motion Vectors|When enabled, HDRP allocates memory for processing motion vectors which it uses for Motion Blur, TAA, and temporal re-projection of various effect like SSR.");
        static readonly GUIContent k_SupportRuntimeDebugDisplayContent = CoreEditorUtils.GetContent("Runtime debug display|When disabled, HDRP removes all debug display Shader variants when you build for the Unity Player. This decreases build time.");
        static readonly GUIContent k_SupportDitheringCrossFadeContent = CoreEditorUtils.GetContent("Dithering cross fade|When disabled, HDRP removes all dithering cross fade Shader variants when you build for the Unity Player. This decreases build time.");
        static readonly GUIContent k_SupportDistortion = CoreEditorUtils.GetContent("Distortion|When disabled, HDRP removes all distortion Shader variants when you build for the Unity Player. This decreases build time.");
        static readonly GUIContent k_SupportTransparentBackface = CoreEditorUtils.GetContent("Transparent Backface|When disabled, HDRP removes all transparent backface Shader variants when you build for the Unity Player. This decreases build time.");
        static readonly GUIContent k_SupportTransparentDepthPrepass = CoreEditorUtils.GetContent("Transparent Depth Prepass|When disabled, HDRP removes all transparent depth prepass Shader variants when you build for the Unity Player. This decreases build time.");
        static readonly GUIContent k_SupportTransparentDepthPostpass = CoreEditorUtils.GetContent("Transparent Depth Postpass|When disabled, HDRP removes all transparent depth postpass Shader variants when you build for the Unity Player. This decreases build time.");

        static RenderPipelineSettingsUI()
        {
            Inspector = CED.Group(
                    CED.Select(
                        (serialized, owner) => serialized.lightLoopSettings,
                        GlobalLightLoopSettingsUI.Inspector
                        ),
                    CED.Select(
                        (serialized, owner) => serialized.hdShadowInitParams,
                        HDShadowInitParametersUI.Inspector
                    ),
                    CED.Select(
                        (serialized, owner) => serialized.decalSettings,
                        GlobalDecalSettingsUI.Inspector
                        )
                    );
        }
        
        public static readonly CED.IDrawer Inspector;

        public static readonly CED.IDrawer SupportedSettings = CED.FoldoutGroup(
            k_SupportedFeatureHeaderContent,
            Expandable.SupportedFeature,
            k_ExpandedState,
            Drawer_SectionPrimarySettings
            );
        
        static void Drawer_SectionPrimarySettings(SerializedRenderPipelineSettings d, Editor o)
        {
            EditorGUILayout.PropertyField(d.supportShadowMask, k_SupportShadowMaskContent);
            EditorGUILayout.PropertyField(d.supportSSR, k_SupportSSRContent);
            EditorGUILayout.PropertyField(d.supportSSAO, k_SupportSSAOContent);

            EditorGUILayout.PropertyField(d.supportSubsurfaceScattering, k_SupportedSSSContent);
            using (new EditorGUI.DisabledScope(!d.supportSubsurfaceScattering.boolValue))
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(d.increaseSssSampleCount, k_SSSSampleCountContent);
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(d.supportVolumetrics, k_SupportVolumetricContent);
            using (new EditorGUI.DisabledScope(!d.supportVolumetrics.boolValue))
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(d.increaseResolutionOfVolumetrics, k_VolumetricResolutionContent);
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(d.supportLightLayers, k_SupportLightLayerContent);
            
            EditorGUILayout.PropertyField(d.supportedLitShaderMode, k_SupportLitShaderModeContent);

            // MSAA is an option that is only available in full forward but Camera can be set in Full Forward only. Thus MSAA have no dependency currently
            //Note: do not use SerializedProperty.enumValueIndex here as this enum not start at 0 as it is used as flags.
            bool msaaAllowed = d.supportedLitShaderMode.intValue == (int)UnityEngine.Experimental.Rendering.HDPipeline.RenderPipelineSettings.SupportedLitShaderMode.ForwardOnly || d.supportedLitShaderMode.intValue == (int)UnityEngine.Experimental.Rendering.HDPipeline.RenderPipelineSettings.SupportedLitShaderMode.Both;
            using (new EditorGUI.DisabledScope(!msaaAllowed))
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(d.MSAASampleCount, k_MSAASampleCountContent);
                --EditorGUI.indentLevel;
            }
            
            EditorGUILayout.PropertyField(d.supportDecals, k_SupportDecalContent);
            EditorGUILayout.PropertyField(d.supportMotionVectors, k_SupportMotionVectorContent);
            EditorGUILayout.PropertyField(d.supportRuntimeDebugDisplay, k_SupportRuntimeDebugDisplayContent);
            EditorGUILayout.PropertyField(d.supportDitheringCrossFade, k_SupportDitheringCrossFadeContent);
            EditorGUILayout.PropertyField(d.supportDistortion, k_SupportDistortion);
            EditorGUILayout.PropertyField(d.supportTransparentBackface, k_SupportTransparentBackface);
            EditorGUILayout.PropertyField(d.supportTransparentDepthPrepass, k_SupportTransparentDepthPrepass);
            EditorGUILayout.PropertyField(d.supportTransparentDepthPostpass, k_SupportTransparentDepthPostpass);

            // Only display the support ray tracing feature if the platform supports it
#if REALTIME_RAYTRACING_SUPPORT
            if(UnityEngine.SystemInfo.supportsRayTracing)
            {
                EditorGUILayout.PropertyField(d.supportRayTracing, _.GetContent("Support Realtime Raytracing."));
            }
            else
#endif
            {
                d.supportRayTracing.boolValue = false;
            }
        }
    }
}
