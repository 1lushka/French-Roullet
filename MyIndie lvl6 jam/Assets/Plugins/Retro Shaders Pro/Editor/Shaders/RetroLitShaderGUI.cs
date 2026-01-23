using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;

namespace RetroShadersPro.URP
{
    internal class RetroLitShaderGUI : ShaderGUI
    {
        MaterialProperty baseColorProp = null;
        const string baseColorName = "_BaseColor";
        const string baseColorLabel = "Base Color";
        const string baseColorTooltip = "Albedo color of the object.";

        MaterialProperty baseTexProp = null;
        const string baseTexName = "_BaseMap";
        const string baseTexLabel = "Base Texture";
        const string baseTexTooltip = "Albedo texture of the object.";

        // -------- Normal Map --------
        MaterialProperty bumpMapProp = null;
        const string bumpMapName = "_BumpMap";
        const string bumpMapLabel = "Normal Map";
        const string bumpMapTooltip = "Tangent-space normal map.";

        MaterialProperty bumpScaleProp = null;
        const string bumpScaleName = "_BumpScale";
        const string bumpScaleLabel = "Normal Strength";
        const string bumpScaleTooltip = "Normal map intensity.";
        // ----------------------------

        // -------- Outline --------
        MaterialProperty outlineProp = null;
        const string outlineName = "_Outline";
        const string outlineLabel = "Outline";

        MaterialProperty outlineColorProp = null;
        const string outlineColorName = "_OutlineColor";
        const string outlineColorLabel = "Outline Color";

        MaterialProperty outlineWidthProp = null;
        const string outlineWidthName = "_OutlineWidth";
        const string outlineWidthLabel = "Outline Width (World)";

        const string outlineKeywordName = "_OUTLINE_ON";
        // -------------------------

        // -------- Rim Light --------
        MaterialProperty rimProp = null;
        const string rimName = "_Rim";
        const string rimLabel = "Rim Light";

        MaterialProperty rimColorProp = null;
        const string rimColorName = "_RimColor";
        const string rimColorLabel = "Rim Color";

        MaterialProperty rimPowerProp = null;
        const string rimPowerName = "_RimPower";
        const string rimPowerLabel = "Rim Power";

        MaterialProperty rimIntensityProp = null;
        const string rimIntensityName = "_RimIntensity";
        const string rimIntensityLabel = "Rim Intensity";

        const string rimKeywordName = "_RIM_ON";
        // ---------------------------

        MaterialProperty resolutionLimitProp = null;
        const string resolutionLimitName = "_ResolutionLimit";
        const string resolutionLimitLabel = "Resolution Limit";
        const string resolutionLimitTooltip = "Limits the resolution of the texture to this value." +
            "\nNote that this setting only snaps the resolution to powers of two." +
            "\nAlso, make sure the Base Texture has mipmaps enabled.";

        MaterialProperty snapsPerUnitProp = null;
        const string snapsPerUnitName = "_SnapsPerUnit";
        const string snapsPerUnitLabel = "Snaps Per Meter";
        const string snapsPerUnitTooltip = "The mesh vertices snap to a limited number of points in space." +
            "\nThis uses clip space, so the mesh may jitter when the camera rotates.";

        MaterialProperty colorBitDepthProp = null;
        const string colorBitDepthName = "_ColorBitDepth";
        const string colorBitDepthLabel = "Color Depth";
        const string colorBitDepthTooltip = "Limits the total number of values used for each color channel.";

        MaterialProperty colorBitDepthOffsetProp = null;
        const string colorBitDepthOffsetName = "_ColorBitDepthOffset";
        const string colorBitDepthOffsetLabel = "Color Depth Offset";
        const string colorBitDepthOffsetTooltip = "Increase this value if the bit depth offset makes your object too dark.";

        MaterialProperty ambientLightProp = null;
        const string ambientLightName = "_AmbientLight";
        const string ambientLightLabel = "Ambient Light Strength";
        const string ambientLightTooltip = "When the ambient light override is used, apply this much ambient light.";

        MaterialProperty affineTextureStrengthProp = null;
        const string affineTextureStrengthName = "_AffineTextureStrength";
        const string affineTextureStrengthLabel = "Affine Texture Strength";
        const string affineTextureStrengthTooltip = "How strongly the affine texture mapping effect is applied." +
            "\nWhen this is set to 1, the shader uses affine texture mapping exactly like the PS1." +
            "\nWhen this is set to 0, the shader uses perspective-correct texture mapping, like modern systems.";

        MaterialProperty ambientToggleProp = null;
        const string ambientToggleName = "_USE_AMBIENT_OVERRIDE";
        const string ambientToggleLabel = "Ambient Light Override";
        const string ambientToggleTooltip = "Should the object use Unity's default ambient light, or a custom override amount?";

        MaterialProperty usePointFilteringProp = null;
        const string usePointFilteringName = "_USE_POINT_FILTER";
        const string usePointFilteringLabel = "Point Filtering";
        const string usePointFilteringTooltip = "Should the shader use point filtering?";

        MaterialProperty useDitheringProp = null;
        const string useDitheringName = "_USE_DITHERING";
        const string useDitheringLabel = "Enable Dithering";
        const string useDitheringTooltip = "Should the shader use color dithering?";

        MaterialProperty usePixelLightingProp = null;
        const string usePixelLightingName = "_USE_PIXEL_LIGHTING";
        const string usePixelLightingLabel = "Texel-aligned Lighting";
        const string usePixelLightingTooltip = "Should lighting and shadow calculations snap to the closest texel on the object's texture?";

        MaterialProperty useVertexColorProp = null;
        const string useVertexColorName = "_USE_VERTEX_COLORS";
        const string useVertexColorLabel = "Use Vertex Colors";
        const string useVertexColorTooltip = "Should the base color of the object use vertex coloring?";

        MaterialProperty alphaClipProp = null;
        const string alphaClipName = "_AlphaClip";
        const string alphaClipLabel = "Alpha Clip";
        const string alphaClipTooltip = "Should the shader clip pixels based on alpha using a threshold value?";

        MaterialProperty alphaClipThresholdProp = null;
        const string alphaClipThresholdName = "_Cutoff";
        const string alphaClipThresholdLabel = "Threshold";
        const string alphaClipThresholdTooltip = "The threshold value to use for alpha clipping.";

        private MaterialProperty cullProp;
        private const string cullName = "_Cull";
        private const string cullLabel = "Render Face";
        private const string cullTooltip = "Should Unity render Front, Back, or Both faces of the mesh?";

        private const string surfaceTypeName = "_Surface";
        private const string surfaceTypeLabel = "Surface Type";
        private const string surfaceTypeTooltip = "Should the object be transparent or opaque?";

        private const string alphaTestName = "_ALPHATEST_ON";

        private enum SurfaceType { Opaque = 0, Transparent = 1 }
        private enum RenderFace { Front = 2, Back = 1, Both = 0 }

        private SurfaceType surfaceType = SurfaceType.Opaque;
        private RenderFace renderFace = RenderFace.Front;

        protected readonly MaterialHeaderScopeList materialScopeList = new MaterialHeaderScopeList(uint.MaxValue);
        protected MaterialEditor materialEditor;
        private bool firstTimeOpen = true;

        private static void SetKeyword(Material mat, string keyword, bool enabled)
        {
            if (enabled) mat.EnableKeyword(keyword);
            else mat.DisableKeyword(keyword);
        }

        private void FindProperties(MaterialProperty[] props)
        {
            baseColorProp = FindProperty(baseColorName, props, true);
            baseTexProp = FindProperty(baseTexName, props, true);

            bumpMapProp = FindProperty(bumpMapName, props, false);
            bumpScaleProp = FindProperty(bumpScaleName, props, false);

            outlineProp = FindProperty(outlineName, props, false);
            outlineColorProp = FindProperty(outlineColorName, props, false);
            outlineWidthProp = FindProperty(outlineWidthName, props, false);

            rimProp = FindProperty(rimName, props, false);
            rimColorProp = FindProperty(rimColorName, props, false);
            rimPowerProp = FindProperty(rimPowerName, props, false);
            rimIntensityProp = FindProperty(rimIntensityName, props, false);

            resolutionLimitProp = FindProperty(resolutionLimitName, props, true);
            snapsPerUnitProp = FindProperty(snapsPerUnitName, props, true);
            colorBitDepthProp = FindProperty(colorBitDepthName, props, true);
            colorBitDepthOffsetProp = FindProperty(colorBitDepthOffsetName, props, true);
            ambientLightProp = FindProperty(ambientLightName, props, false);
            affineTextureStrengthProp = FindProperty(affineTextureStrengthName, props, true);
            ambientToggleProp = FindProperty(ambientToggleName, props, false);
            usePointFilteringProp = FindProperty(usePointFilteringName, props, false);
            useDitheringProp = FindProperty(useDitheringName, props, true);
            usePixelLightingProp = FindProperty(usePixelLightingName, props, false);
            useVertexColorProp = FindProperty(useVertexColorName, props, false);

            cullProp = FindProperty(cullName, props, true);
            alphaClipProp = FindProperty(alphaClipName, props, true);
            alphaClipThresholdProp = FindProperty(alphaClipThresholdName, props, true);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (materialEditor == null)
                throw new ArgumentNullException("No MaterialEditor found (RetroLitShaderGUI).");

            Material material = materialEditor.target as Material;
            this.materialEditor = materialEditor;

            FindProperties(properties);

            if (firstTimeOpen)
            {
                materialScopeList.RegisterHeaderScope(new GUIContent("Surface Options"), 1u << 0, DrawSurfaceOptions);
                materialScopeList.RegisterHeaderScope(new GUIContent("Retro Properties"), 1u << 1, DrawRetroProperties);
                firstTimeOpen = false;
            }

            materialScopeList.DrawHeaders(materialEditor, material);
            materialEditor.serializedObject.ApplyModifiedProperties();
        }

        private void DrawSurfaceOptions(Material material)
        {
            surfaceType = (SurfaceType)material.GetFloat(surfaceTypeName);
            renderFace = (RenderFace)material.GetFloat(cullName);

            bool surfaceTypeChanged = false;
            EditorGUI.BeginChangeCheck();
            surfaceType = (SurfaceType)EditorGUILayout.EnumPopup(new GUIContent(surfaceTypeLabel, surfaceTypeTooltip), surfaceType);
            if (EditorGUI.EndChangeCheck()) surfaceTypeChanged = true;

            EditorGUI.BeginChangeCheck();
            renderFace = (RenderFace)EditorGUILayout.EnumPopup(new GUIContent(cullLabel, cullTooltip), renderFace);
            if (EditorGUI.EndChangeCheck())
            {
                switch (renderFace)
                {
                    case RenderFace.Both: material.SetFloat(cullName, 0); break;
                    case RenderFace.Back: material.SetFloat(cullName, 1); break;
                    case RenderFace.Front: material.SetFloat(cullName, 2); break;
                }
            }

            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(alphaClipProp, new GUIContent(alphaClipLabel, alphaClipTooltip));
            if (EditorGUI.EndChangeCheck()) surfaceTypeChanged = true;

            bool alphaClip;

            if (surfaceTypeChanged)
            {
                switch (surfaceType)
                {
                    case SurfaceType.Opaque:
                        {
                            material.SetOverrideTag("RenderType", "Opaque");
                            material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                            material.SetFloat("_ZWrite", 1);
                            material.SetFloat(surfaceTypeName, 0);

                            alphaClip = material.GetFloat(alphaClipName) >= 0.5f;
                            if (alphaClip)
                            {
                                material.EnableKeyword(alphaTestName);
                                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                                material.SetOverrideTag("RenderType", "TransparentCutout");
                            }
                            else
                            {
                                material.DisableKeyword(alphaTestName);
                                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                                material.SetOverrideTag("RenderType", "Opaque");
                            }
                            break;
                        }
                    case SurfaceType.Transparent:
                        {
                            alphaClip = material.GetFloat(alphaClipName) >= 0.5f;
                            SetKeyword(material, alphaTestName, alphaClip);

                            material.SetOverrideTag("RenderType", "Transparent");
                            material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            material.SetFloat("_ZWrite", 0);
                            material.SetFloat(surfaceTypeName, 1);

                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                            break;
                        }
                }
            }

            alphaClip = material.GetFloat(alphaClipName) >= 0.5f;
            if (alphaClip)
            {
                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(alphaClipThresholdProp, new GUIContent(alphaClipThresholdLabel, alphaClipThresholdTooltip));
                EditorGUI.indentLevel--;
            }
        }

        private void DrawRetroProperties(Material material)
        {
            materialEditor.ShaderProperty(baseColorProp, new GUIContent(baseColorLabel, baseColorTooltip));
            materialEditor.ShaderProperty(baseTexProp, new GUIContent(baseTexLabel, baseTexTooltip));

            if (bumpMapProp != null)
            {
                if (bumpScaleProp != null)
                {
                    materialEditor.TexturePropertySingleLine(
                        new GUIContent(bumpMapLabel, bumpMapTooltip),
                        bumpMapProp,
                        bumpScaleProp
                    );
                }
                else
                {
                    materialEditor.TexturePropertySingleLine(
                        new GUIContent(bumpMapLabel, bumpMapTooltip),
                        bumpMapProp
                    );
                }
            }

            materialEditor.ShaderProperty(resolutionLimitProp, new GUIContent(resolutionLimitLabel, resolutionLimitTooltip));
            materialEditor.ShaderProperty(snapsPerUnitProp, new GUIContent(snapsPerUnitLabel, snapsPerUnitTooltip));
            materialEditor.ShaderProperty(colorBitDepthProp, new GUIContent(colorBitDepthLabel, colorBitDepthTooltip));
            materialEditor.ShaderProperty(colorBitDepthOffsetProp, new GUIContent(colorBitDepthOffsetLabel, colorBitDepthOffsetTooltip));
            materialEditor.ShaderProperty(affineTextureStrengthProp, new GUIContent(affineTextureStrengthLabel, affineTextureStrengthTooltip));

            if (ambientLightProp != null)
            {
                materialEditor.ShaderProperty(ambientToggleProp, new GUIContent(ambientToggleLabel, ambientToggleTooltip));

                bool ambient = material.GetFloat(ambientToggleName) >= 0.5f;
                SetKeyword(material, ambientToggleName, ambient);

                if (ambient)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(ambientLightProp, new GUIContent(ambientLightLabel, ambientLightTooltip));
                    EditorGUI.indentLevel--;
                }
            }

            if (usePointFilteringProp != null)
                materialEditor.ShaderProperty(usePointFilteringProp, new GUIContent(usePointFilteringLabel, usePointFilteringTooltip));

            if (useDitheringProp != null)
            {
                materialEditor.ShaderProperty(useDitheringProp, new GUIContent(useDitheringLabel, useDitheringTooltip));
                SetKeyword(material, useDitheringName, material.GetFloat(useDitheringName) >= 0.5f);
            }

            if (usePixelLightingProp != null)
            {
                materialEditor.ShaderProperty(usePixelLightingProp, new GUIContent(usePixelLightingLabel, usePixelLightingTooltip));
                SetKeyword(material, usePixelLightingName, material.GetFloat(usePixelLightingName) >= 0.5f);
            }

            if (useVertexColorProp != null)
            {
                materialEditor.ShaderProperty(useVertexColorProp, new GUIContent(useVertexColorLabel, useVertexColorTooltip));
                SetKeyword(material, useVertexColorName, material.GetFloat(useVertexColorName) >= 0.5f);
            }

            // ===== OUTLINE UI =====
            if (outlineProp != null)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Outline", EditorStyles.boldLabel);

                materialEditor.ShaderProperty(outlineProp, outlineLabel);
                bool outlineEnabled = material.GetFloat(outlineName) >= 0.5f;
                SetKeyword(material, outlineKeywordName, outlineEnabled);

                if (outlineEnabled)
                {
                    EditorGUI.indentLevel++;
                    if (outlineColorProp != null) materialEditor.ShaderProperty(outlineColorProp, outlineColorLabel);
                    if (outlineWidthProp != null) materialEditor.ShaderProperty(outlineWidthProp, outlineWidthLabel);
                    EditorGUI.indentLevel--;
                }
            }

            // ===== RIM LIGHT UI =====
            if (rimProp != null)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Rim Light", EditorStyles.boldLabel);

                materialEditor.ShaderProperty(rimProp, rimLabel);
                bool rimEnabled = material.GetFloat(rimName) >= 0.5f;
                SetKeyword(material, rimKeywordName, rimEnabled);

                if (rimEnabled)
                {
                    EditorGUI.indentLevel++;
                    if (rimColorProp != null) materialEditor.ShaderProperty(rimColorProp, rimColorLabel);
                    if (rimPowerProp != null) materialEditor.ShaderProperty(rimPowerProp, rimPowerLabel);
                    if (rimIntensityProp != null) materialEditor.ShaderProperty(rimIntensityProp, rimIntensityLabel);
                    EditorGUI.indentLevel--;
                }
            }
        }
    }
}
