// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2020 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

//using UnityEngine;
using System.IO;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility.AssetInjection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using UnityEngine;
using System;

namespace DaggerfallWorkshopWpf
{
    public enum TextureFormat
    {
        DXT5,
        ARGB32
    }

    public enum TextureImport
    {
        None,
        LooseFiles
    }

    public class TextureReader
    {
        const int spectralEyesReserved = 14;
        const int spectralEyesPatched = 247;
        const int spectralGrayStart = 96;

        bool mipMaps = true;

        // Special texture indices
        public const int EditorFlatsTextureArchive = 199;
        public const int AnimalsTextureArchive = 201;
        public const int LightsTextureArchive = 210;
        public const int FixedTreasureFlatsArchive = 216;
        public const int FireWallsArchive = 356;
        //public int[] MiscFlatsTextureArchives = new int[] { 97, 205, 211, 212, 213, 301 };

        /// <summary>
        /// Gets or sets Arena2 path.
        /// Path must be set before attempting to load textures.
        /// </summary>
        public string Arena2Path { get; private set; }

        /// <summary>
        /// Gets or sets flag to generate mipmaps for textures.
        /// </summary>
        public bool MipMaps
        {
            get { return mipMaps; }
            set { mipMaps = value; }
        }

        public static GetTextureSettings CreateTextureSettings(
            int archive = 0,
            int record = 0,
            int frame = 0,
            int alphaIndex = 0,
            int borderSize = 0,
            bool dilate = false,
            int maxAtlasSize = 2048)
        {
            GetTextureSettings settings = new GetTextureSettings();
            settings.archive = archive;
            settings.record = record;
            settings.frame = frame;
            settings.alphaIndex = alphaIndex;
            settings.borderSize = borderSize;
            settings.dilate = dilate;
            settings.atlasMaxSize = maxAtlasSize;

            return settings;
        }

        public byte[] GetTextureBytes(GetTextureSettings settings, out DFSize sz)
        {
            // Check if window, spectral, or auto-emissive
            //bool isWindow = ClimateSwaps.IsExteriorWindow(settings.archive, settings.record);
            bool isSpectral = TextureFile.IsSpectralArchive(settings.archive);
            bool isEmissive = IsEmissive(settings.archive, settings.record);

            // Assign texture file
            TextureFile textureFile = new TextureFile(Path.Combine(
                Arena2Path,
                TextureFile.IndexToFileName(settings.archive)),
                FileUsage.UseMemory,
                true);

            // Get starting DFBitmap
            DFBitmap srcBitmap = textureFile.GetDFBitmap(settings.record, settings.frame);

            if (isSpectral)
            {
                // Adjust source bitmap to set spectral grays
                // 180 = transparency amount (~70% visible)
                SetSpectral(ref srcBitmap);
                return textureFile.GetBytesRGBA(
                    srcBitmap,
                    settings.alphaIndex,
                    settings.borderSize,
                    out sz,
                    spectralEyesPatched,
                    180);
            }
            else
            {
                // Read direct from source bitmap
                return textureFile.GetBytesRGBA(
                    srcBitmap,
                    settings.alphaIndex,
                    settings.borderSize,
                    out sz);
            }
        }

        /// <summary>
        /// Gets Unity textures from Daggerfall texture with all options.
        /// Returns all supported texture maps for Standard shader in one call.
        /// </summary>
        /// <param name="settings">Get texture settings.</param>
        /// <param name="alphaTextureFormat">Alpha TextureFormat.</param>
        /// <param name="textureImport">Options for import of custom textures.</param>
        /// <returns>GetTextureResults.</returns>
        public GetTextureResults GetTexture2D(
            GetTextureSettings settings,
            SupportedAlphaTextureFormats alphaTextureFormat = SupportedAlphaTextureFormats.ARGB32,
            TextureImport textureImport = TextureImport.None)
        {
            GetTextureResults results = new GetTextureResults();

            // Check if window, spectral, or auto-emissive
            //bool isWindow = ClimateSwaps.IsExteriorWindow(settings.archive, settings.record);
            bool isSpectral = TextureFile.IsSpectralArchive(settings.archive);
            bool isEmissive = (settings.autoEmission) ? IsEmissive(settings.archive, settings.record) : false;

            // Override readable flag when user has set preference in material reader
            if (DaggerfallWpf3D.Instance.MaterialReader.ReadableTextures)
            {
                settings.stayReadable = true;
            }

            // Disable mipmaps in retro mode
            ///if (DaggerfallWpf3D.Settings.RetroRenderingMode > 0 && !DaggerfallWpf3D.Settings.UseMipMapsInRetroMode)
            mipMaps = false;

            // Assign texture file
            TextureFile textureFile;
            if (settings.textureFile == null)
                textureFile = new TextureFile(Path.Combine(Arena2Path, TextureFile.IndexToFileName(settings.archive)), FileUsage.UseMemory, true);
            else
                textureFile = settings.textureFile;

            // Get starting DFBitmap
            DFSize sz;
            DFBitmap srcBitmap = textureFile.GetDFBitmap(settings.record, settings.frame);

            // Get albedo Color32 array
            byte[] albedoColors;
            if (isSpectral)
            {
                // Adjust source bitmap to set spectral grays
                // 180 = transparency amount (~70% visible)
                SetSpectral(ref srcBitmap);
                albedoColors = textureFile.GetBytesBGRA(srcBitmap, settings.alphaIndex, settings.borderSize, out sz, spectralEyesPatched, 180);
            }
            else
            {
                // Read direct from source bitmap
                albedoColors = textureFile.GetBytesBGRA(srcBitmap, settings.alphaIndex, settings.borderSize, out sz);
            }

            ///// Sharpen source image
            ///if (settings.sharpen)
            ///    albedoColors = ImageProcessing.Sharpen(ref albedoColors, sz.Width, sz.Height);

            ///// Dilate edges
            ///if (settings.borderSize > 0 && settings.dilate && !settings.copyToOppositeBorder)
            ///    ImageProcessing.DilateColors(ref albedoColors, sz);

            ///// Copy to opposite border
            ///if (settings.borderSize > 0 && settings.copyToOppositeBorder)
            ///    ImageProcessing.WrapBorder(ref albedoColors, sz, settings.borderSize);

            /*
             PixelFormat pf = PixelFormats.Bgr32;
            int width = 200;
            int height = 200;
            int rawStride = (width * pf.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * height]; 
             */

            // Set albedo texture
            ImageSource albedoMap;
            ///if (!TextureReplacement.TryImportTexture(settings.archive, settings.record, settings.frame, TextureMap.Albedo, textureImport, !settings.stayReadable, out albedoMap))
            {
                PixelFormat format = PixelFormats.Bgra32;
                // width of image as a multiple of 8
                int stride = (sz.Width * format.BitsPerPixel + 7) / 8;

                // Create albedo texture
                albedoMap = BitmapSource.Create(
                    sz.Width, sz.Height,
                    96, 96,
                    format, null,
                    albedoColors, stride);
                //albedoMap = new Texture2D(sz.Width, sz.Height, ParseTextureFormat(alphaTextureFormat), MipMaps);
                //albedoMap.SetPixels32(albedoColors);
                //albedoMap.Apply(true, !settings.stayReadable);
            }

            ///// Adjust mipmap bias of albedo map when retro mode rendering is enabled
            ///if (albedoMap && DaggerfallWpf3D.Settings.RetroRenderingMode > 0)
            ///{
            ///    albedoMap.mipMapBias = -0.75f;
            ///}

            ///// Set normal texture (always import normal if present on disk)
            ///ImageSource normalMap = null;
            ///bool normalMapImported = TextureReplacement.TryImportTexture(settings.archive, settings.record, settings.frame, TextureMap.Normal, textureImport, !settings.stayReadable, out normalMap);
            ///if (!normalMapImported && settings.createNormalMap && textureFile.SolidType == TextureFile.SolidTypes.None)
            ///{
            ///    // Create normal texture - must be ARGB32
            ///    // Normal maps are bypassed for solid-colour textures
            ///    Color32[] normalColors;
            ///    normalColors = ImageProcessing.GetBumpMap(ref albedoColors, sz.Width, sz.Height);
            ///    normalColors = ImageProcessing.ConvertBumpToNormals(ref normalColors, sz.Width, sz.Height, settings.normalStrength);
            ///    normalMap = new Texture2D(sz.Width, sz.Height, TextureFormat.ARGB32, MipMaps);
            ///    normalMap.SetPixels32(normalColors);
            ///    normalMap.Apply(true, !settings.stayReadable);
            ///}

#if false
            // Import emission map or create basic emissive texture
            ImageSource emissionMap = null;
            bool resultEmissive = false;
            if (TextureReplacement.TryImportTexture(settings.archive, settings.record, settings.frame, TextureMap.Emission, textureImport, !settings.stayReadable, out emissionMap))
            {
                // Always import emission if present on disk
                resultEmissive = true;
            }
            else
            {
                if (settings.createEmissionMap || (settings.autoEmission && isEmissive) && !isWindow)
                {
                    if (settings.archive != FireWallsArchive)
                        // Just reuse albedo map for basic colour emission
                        emissionMap = albedoMap;
                    else
                    {
                        // Mantellan Crux fire walls - lessen stroboscopic effect
                        Color fireColor = new Color(0.706f, 0.271f, 0.086f); // Average of dim frame (0)
                        // Color fireColor = new Color(0.773f, 0.38f, 0.122f); // Average of average frames (1 and 3)
                        // Color fireColor = new Color(0.851f, 0.506f, 0.165f); // Average of bright frame (2)
                        Color32[] firewallEmissionColors = textureFile.GetFireWallColors32(ref albedoColors, sz.Width, sz.Height, fireColor, 0.3f);
                        emissionMap = new Texture2D(sz.Width, sz.Height, ParseTextureFormat(alphaTextureFormat), MipMaps);
                        emissionMap.SetPixels32(firewallEmissionColors);
                        emissionMap.Apply(true, !settings.stayReadable);
                    }
                    resultEmissive = true;
                }

                // Windows need special handling as only glass parts are emissive
                if ((settings.createEmissionMap || settings.autoEmissionForWindows) && isWindow)
                {
                    // Create custom emission texture for glass area of windows
                    Color32[] emissionColors = textureFile.GetWindowColors32(srcBitmap);
                    emissionMap = new Texture2D(sz.Width, sz.Height, ParseTextureFormat(alphaTextureFormat), MipMaps);
                    emissionMap.SetPixels32(emissionColors);
                    emissionMap.Apply(true, !settings.stayReadable);
                    resultEmissive = true;
                }

                // Spectral need special handling as only eye parts are emissive
                if ((settings.createEmissionMap || settings.autoEmission) && isSpectral)
                {
                    Color eyeEmission = Color.red;
                    Color bodyEmission = Color.black;// new Color(0.1f, 0.1f, 0.1f);
                    Color32[] emissionColors = textureFile.GetSpectralEmissionColors32(srcBitmap, albedoColors, settings.borderSize, spectralEyesPatched, eyeEmission, bodyEmission);
                    emissionMap = new Texture2D(albedoMap.width, albedoMap.height, ParseTextureFormat(alphaTextureFormat), MipMaps);
                    emissionMap.SetPixels32(emissionColors);
                    emissionMap.Apply(true, !settings.stayReadable);
                    resultEmissive = true;
                }

                // Some archives need special handling as they contains a mix of emissive and non-emissive flats
                // This can cause problems with atlas packing due to mismatch between albedo and emissive texture counts
                if ((settings.createEmissionMap || settings.autoEmission) && IsEmissiveArchive(settings.archive))
                {
                    // For the unlit flats we create a null-emissive black texture
                    if (!resultEmissive)
                    {
                        Color32[] emissionColors = new Color32[albedoMap.width * albedoMap.height];
                        emissionMap = new Texture2D(albedoMap.width, albedoMap.height, ParseTextureFormat(alphaTextureFormat), MipMaps);
                        emissionMap.SetPixels32(emissionColors);
                        emissionMap.Apply(true, !settings.stayReadable);
                        resultEmissive = true;
                    }
                }
            }
#endif
            // Shrink UV rect to compensate for internal border
            float ru = 1f / sz.Width;
            float rv = 1f / sz.Height;
            results.singleRect = new Rect(
                settings.borderSize * ru,
                settings.borderSize * rv,
                (sz.Width - settings.borderSize * 2) * ru,
                (sz.Height - settings.borderSize * 2) * rv);

            // Store results
            results.albedoMap = albedoMap;
            //results.normalMap = normalMap;
            //results.emissionMap = emissionMap;
            //results.isWindow = isWindow;
            //results.isEmissive = resultEmissive;
            results.textureFile = textureFile;

            return results;
        }

        /// <summary>
        /// Updates spectral color indices to set eyes red and accentuate other features
        /// </summary>
        /// <param name="srcBitmap"></param>
        public void SetSpectral(ref DFBitmap srcBitmap)
        {
            for (int i = 0; i < srcBitmap.Data.Length; i++)
            {
                byte index = srcBitmap.Data[i];

                // Index 14 is reserved for emissive eyes, patching this to 112 (white color index) for best interaction with shader
                // Other colours are reindexed to a grey gradiant area of palette so that bones are brighter than shroud
                if (index == spectralEyesReserved)
                    srcBitmap.Data[i] = spectralEyesPatched;
                else if (index > 0)
                    srcBitmap.Data[i] = (byte)(spectralGrayStart - index);
            }
        }

        #region Private Methods

        private PixelFormat ParseTextureFormat(SupportedAlphaTextureFormats format)
        {
            switch (format)
            {
                default:
                case SupportedAlphaTextureFormats.RGBA32:
                case SupportedAlphaTextureFormats.ARGB32:
                case SupportedAlphaTextureFormats.ARGB444:
                case SupportedAlphaTextureFormats.RGBA444:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region Helpers

        // Textures that should receive emission map
        // TODO: Consider setting this from an external list
        static DaggerfallTextureIndex[] emissiveTextures = new DaggerfallTextureIndex[]
        {
            // Fireplace
            new DaggerfallTextureIndex() { archive = 87, record = 0 },

            // Lights (which are on/lit)
            new DaggerfallTextureIndex() { archive = 101, record = 2 },
            new DaggerfallTextureIndex() { archive = 101, record = 3 },
            new DaggerfallTextureIndex() { archive = 101, record = 5 },
            new DaggerfallTextureIndex() { archive = 101, record = 6 },
            new DaggerfallTextureIndex() { archive = 101, record = 7 },
            new DaggerfallTextureIndex() { archive = 101, record = 8 },
            new DaggerfallTextureIndex() { archive = 101, record = 9 },
            // new DaggerfallTextureIndex() { archive = 101, record = 10 }, // is glass globe a light source?
            new DaggerfallTextureIndex() { archive = 101, record = 11 },
            new DaggerfallTextureIndex() { archive = 101, record = 12 },
            new DaggerfallTextureIndex() { archive = 190, record = 3 },
            new DaggerfallTextureIndex() { archive = 190, record = 4 },
            new DaggerfallTextureIndex() { archive = 190, record = 5 },
            new DaggerfallTextureIndex() { archive = 200, record = 7 },
            new DaggerfallTextureIndex() { archive = 200, record = 8 },
            new DaggerfallTextureIndex() { archive = 200, record = 9 },
            new DaggerfallTextureIndex() { archive = 200, record = 10 },

            // Statue
            new DaggerfallTextureIndex() { archive = 202, record = 2 },

            // Brewing potion
            new DaggerfallTextureIndex() { archive = 208, record = 2 },

            new DaggerfallTextureIndex() { archive = 210, record = 0 },
            new DaggerfallTextureIndex() { archive = 210, record = 1 },
            new DaggerfallTextureIndex() { archive = 210, record = 2 },
            new DaggerfallTextureIndex() { archive = 210, record = 3 },
            new DaggerfallTextureIndex() { archive = 210, record = 4 },
            new DaggerfallTextureIndex() { archive = 210, record = 5 },
            new DaggerfallTextureIndex() { archive = 210, record = 6 },
            new DaggerfallTextureIndex() { archive = 210, record = 8 },
            new DaggerfallTextureIndex() { archive = 210, record = 9 },
            new DaggerfallTextureIndex() { archive = 210, record = 11 },
            new DaggerfallTextureIndex() { archive = 210, record = 13 },
            new DaggerfallTextureIndex() { archive = 210, record = 14 },
            new DaggerfallTextureIndex() { archive = 210, record = 15 },
            new DaggerfallTextureIndex() { archive = 210, record = 16 },
            new DaggerfallTextureIndex() { archive = 210, record = 17 },
            new DaggerfallTextureIndex() { archive = 210, record = 18 },
            new DaggerfallTextureIndex() { archive = 210, record = 19 },
            new DaggerfallTextureIndex() { archive = 210, record = 20 },
            new DaggerfallTextureIndex() { archive = 210, record = 21 },
            new DaggerfallTextureIndex() { archive = 210, record = 22 },
            new DaggerfallTextureIndex() { archive = 210, record = 23 },
            new DaggerfallTextureIndex() { archive = 210, record = 24 },
            new DaggerfallTextureIndex() { archive = 210, record = 25 },
            new DaggerfallTextureIndex() { archive = 210, record = 26 },
            new DaggerfallTextureIndex() { archive = 210, record = 27 },
            new DaggerfallTextureIndex() { archive = 210, record = 28 },
            new DaggerfallTextureIndex() { archive = 210, record = 29 },

            new DaggerfallTextureIndex() { archive = 253, record = 10 },
            new DaggerfallTextureIndex() { archive = 253, record = 17 },
            new DaggerfallTextureIndex() { archive = 253, record = 18 },
            new DaggerfallTextureIndex() { archive = 253, record = 19 },
            new DaggerfallTextureIndex() { archive = 253, record = 22 },
            new DaggerfallTextureIndex() { archive = 253, record = 41 },
            new DaggerfallTextureIndex() { archive = 253, record = 48 },
            new DaggerfallTextureIndex() { archive = 253, record = 49 },
            new DaggerfallTextureIndex() { archive = 253, record = 50 },
            new DaggerfallTextureIndex() { archive = 253, record = 51 },
            new DaggerfallTextureIndex() { archive = 253, record = 52 },
            new DaggerfallTextureIndex() { archive = 253, record = 75 },
            new DaggerfallTextureIndex() { archive = 253, record = 77 },

            // Ghost
            new DaggerfallTextureIndex() { archive = 273, record = 0 },
            new DaggerfallTextureIndex() { archive = 273, record = 1 },
            new DaggerfallTextureIndex() { archive = 273, record = 2 },
            new DaggerfallTextureIndex() { archive = 273, record = 3 },
            new DaggerfallTextureIndex() { archive = 273, record = 4 },
            new DaggerfallTextureIndex() { archive = 273, record = 5 },
            new DaggerfallTextureIndex() { archive = 273, record = 6 },
            new DaggerfallTextureIndex() { archive = 273, record = 7 },
            new DaggerfallTextureIndex() { archive = 273, record = 8 },
            new DaggerfallTextureIndex() { archive = 273, record = 9 },
            new DaggerfallTextureIndex() { archive = 273, record = 10 },
            new DaggerfallTextureIndex() { archive = 273, record = 11 },
            new DaggerfallTextureIndex() { archive = 273, record = 12 },
            new DaggerfallTextureIndex() { archive = 273, record = 13 },
            new DaggerfallTextureIndex() { archive = 273, record = 14 },

            // Wraith
            new DaggerfallTextureIndex() { archive = 278, record = 0 },
            new DaggerfallTextureIndex() { archive = 278, record = 1 },
            new DaggerfallTextureIndex() { archive = 278, record = 2 },
            new DaggerfallTextureIndex() { archive = 278, record = 3 },
            new DaggerfallTextureIndex() { archive = 278, record = 4 },
            new DaggerfallTextureIndex() { archive = 278, record = 5 },
            new DaggerfallTextureIndex() { archive = 278, record = 6 },
            new DaggerfallTextureIndex() { archive = 278, record = 7 },
            new DaggerfallTextureIndex() { archive = 278, record = 8 },
            new DaggerfallTextureIndex() { archive = 278, record = 9 },
            new DaggerfallTextureIndex() { archive = 278, record = 10 },
            new DaggerfallTextureIndex() { archive = 278, record = 11 },
            new DaggerfallTextureIndex() { archive = 278, record = 12 },
            new DaggerfallTextureIndex() { archive = 278, record = 13 },
            new DaggerfallTextureIndex() { archive = 278, record = 14 },

            // Frost daedra
            new DaggerfallTextureIndex() { archive = 280, record = 0 },
            new DaggerfallTextureIndex() { archive = 280, record = 1 },
            new DaggerfallTextureIndex() { archive = 280, record = 2 },
            new DaggerfallTextureIndex() { archive = 280, record = 3 },
            new DaggerfallTextureIndex() { archive = 280, record = 4 },
            new DaggerfallTextureIndex() { archive = 280, record = 5 },
            new DaggerfallTextureIndex() { archive = 280, record = 6 },
            new DaggerfallTextureIndex() { archive = 280, record = 7 },
            new DaggerfallTextureIndex() { archive = 280, record = 8 },
            new DaggerfallTextureIndex() { archive = 280, record = 9 },
            new DaggerfallTextureIndex() { archive = 280, record = 10 },
            new DaggerfallTextureIndex() { archive = 280, record = 11 },
            new DaggerfallTextureIndex() { archive = 280, record = 12 },
            new DaggerfallTextureIndex() { archive = 280, record = 13 },
            new DaggerfallTextureIndex() { archive = 280, record = 14 },
            new DaggerfallTextureIndex() { archive = 280, record = 15 },
            new DaggerfallTextureIndex() { archive = 280, record = 16 },
            new DaggerfallTextureIndex() { archive = 280, record = 17 },
            new DaggerfallTextureIndex() { archive = 280, record = 18 },
            new DaggerfallTextureIndex() { archive = 280, record = 19 },
            new DaggerfallTextureIndex() { archive = 400, record = 3 }, // corpse

            // Fire daedra
            new DaggerfallTextureIndex() { archive = 281, record = 0 },
            new DaggerfallTextureIndex() { archive = 281, record = 1 },
            new DaggerfallTextureIndex() { archive = 281, record = 2 },
            new DaggerfallTextureIndex() { archive = 281, record = 3 },
            new DaggerfallTextureIndex() { archive = 281, record = 4 },
            new DaggerfallTextureIndex() { archive = 281, record = 5 },
            new DaggerfallTextureIndex() { archive = 281, record = 6 },
            new DaggerfallTextureIndex() { archive = 281, record = 7 },
            new DaggerfallTextureIndex() { archive = 281, record = 8 },
            new DaggerfallTextureIndex() { archive = 281, record = 9 },
            new DaggerfallTextureIndex() { archive = 281, record = 10 },
            new DaggerfallTextureIndex() { archive = 281, record = 11 },
            new DaggerfallTextureIndex() { archive = 281, record = 12 },
            new DaggerfallTextureIndex() { archive = 281, record = 13 },
            new DaggerfallTextureIndex() { archive = 281, record = 14 },
            new DaggerfallTextureIndex() { archive = 281, record = 15 },
            new DaggerfallTextureIndex() { archive = 281, record = 16 },
            new DaggerfallTextureIndex() { archive = 281, record = 17 },
            new DaggerfallTextureIndex() { archive = 281, record = 18 },
            new DaggerfallTextureIndex() { archive = 281, record = 19 },
            new DaggerfallTextureIndex() { archive = 400, record = 2 }, // corpse

            // Fire atronach
            new DaggerfallTextureIndex() { archive = 290, record = 0 },
            new DaggerfallTextureIndex() { archive = 290, record = 1 },
            new DaggerfallTextureIndex() { archive = 290, record = 2 },
            new DaggerfallTextureIndex() { archive = 290, record = 3 },
            new DaggerfallTextureIndex() { archive = 290, record = 4 },
            new DaggerfallTextureIndex() { archive = 290, record = 5 },
            new DaggerfallTextureIndex() { archive = 290, record = 6 },
            new DaggerfallTextureIndex() { archive = 290, record = 7 },
            new DaggerfallTextureIndex() { archive = 290, record = 8 },
            new DaggerfallTextureIndex() { archive = 290, record = 9 },
            new DaggerfallTextureIndex() { archive = 290, record = 10 },
            new DaggerfallTextureIndex() { archive = 290, record = 11 },
            new DaggerfallTextureIndex() { archive = 290, record = 12 },
            new DaggerfallTextureIndex() { archive = 290, record = 13 },
            new DaggerfallTextureIndex() { archive = 290, record = 14 },
            new DaggerfallTextureIndex() { archive = 290, record = 15 },
            new DaggerfallTextureIndex() { archive = 290, record = 16 },
            new DaggerfallTextureIndex() { archive = 290, record = 17 },
            new DaggerfallTextureIndex() { archive = 290, record = 18 },
            new DaggerfallTextureIndex() { archive = 290, record = 19 },
            new DaggerfallTextureIndex() { archive = 405, record = 2 }, // corpse

            // Mantellan Crux fire textures
            new DaggerfallTextureIndex() { archive = 356, record = 0 },
            new DaggerfallTextureIndex() { archive = 356, record = 2 },
            new DaggerfallTextureIndex() { archive = 356, record = 3 },

            // Spell missiles
            new DaggerfallTextureIndex() { archive = 375, record = 0 },
            new DaggerfallTextureIndex() { archive = 375, record = 1 },
            new DaggerfallTextureIndex() { archive = 376, record = 0 },
            new DaggerfallTextureIndex() { archive = 376, record = 1 },
            new DaggerfallTextureIndex() { archive = 377, record = 0 },
            new DaggerfallTextureIndex() { archive = 377, record = 1 },
            new DaggerfallTextureIndex() { archive = 378, record = 0 },
            new DaggerfallTextureIndex() { archive = 378, record = 1 },
            new DaggerfallTextureIndex() { archive = 379, record = 0 },
            new DaggerfallTextureIndex() { archive = 379, record = 1 },

            // Magic decorative effects
            new DaggerfallTextureIndex() { archive = 380, record = 3 },
            // new DaggerfallTextureIndex() { archive = 380, record = 5 }, // UI
            new DaggerfallTextureIndex() { archive = 434, record = 3 },
            // new DaggerfallTextureIndex() { archive = 434, record = 5 }, // UI

            // Lysandus
            new DaggerfallTextureIndex() { archive = 473, record = 0 },
            new DaggerfallTextureIndex() { archive = 473, record = 1 },
            new DaggerfallTextureIndex() { archive = 473, record = 2 },
            new DaggerfallTextureIndex() { archive = 473, record = 3 },
            new DaggerfallTextureIndex() { archive = 473, record = 4 },
            new DaggerfallTextureIndex() { archive = 473, record = 5 },
            new DaggerfallTextureIndex() { archive = 473, record = 6 },
            new DaggerfallTextureIndex() { archive = 473, record = 7 },
            new DaggerfallTextureIndex() { archive = 473, record = 8 },
            new DaggerfallTextureIndex() { archive = 473, record = 9 },
            new DaggerfallTextureIndex() { archive = 473, record = 10 },
            new DaggerfallTextureIndex() { archive = 473, record = 11 },
            new DaggerfallTextureIndex() { archive = 473, record = 12 },
            new DaggerfallTextureIndex() { archive = 473, record = 13 },
            new DaggerfallTextureIndex() { archive = 473, record = 14 },
        };

        static Dictionary<int, List<int>> fastEmissiveTextures = null;

        public TextureReader(string arena2Path)
        {
            Arena2Path = arena2Path;
            FastEmissiveTexturesInit();
        }

        private static void FastEmissiveTexturesInit()
        {
            fastEmissiveTextures = new Dictionary<int, List<int>>();
            foreach (DaggerfallTextureIndex emissiveTexture in emissiveTextures)
            {
                List<int> textureRecords;
                if (!fastEmissiveTextures.TryGetValue(emissiveTexture.archive, out textureRecords))
                {
                    textureRecords = new List<int>();
                    fastEmissiveTextures.Add(emissiveTexture.archive, textureRecords);
                }
                textureRecords.Add(emissiveTexture.record);
            }
        }

        public bool IsEmissiveArchive(int archive)
        {
            return fastEmissiveTextures.ContainsKey(archive);
        }

        public bool IsEmissive(int archive, int record)
        {
            List<int> textureRecords;
            if (fastEmissiveTextures.TryGetValue(archive, out textureRecords))
            {
                // Check emissive list for this texture
                foreach (int textureRecord in textureRecords)
                {
                    if (textureRecord == record)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if sprite is a child NPC sprite texture.
        /// </summary>
        /// <param name="archive">Texture archive.</param>
        /// <param name="record">Texture record.</param>
        /// <returns>True if a child NPC sprite texture.</returns>
        public static bool IsChildNPCTexture(int archive, int record)
        {
            // Archives known to store child NPC textures
            // Records are checked for each individually for a match
            const int templePeople = 181;
            const int mediumCommonPeople = 182;
            const int flatPeople2 = 184;
            const int testBigFlats = 186;
            const int kludgeTown = 197;
            const int daggerfallPeople = 334;
            const int wayrestPeople = 346;
            const int sentinelPeople = 357;

            if (archive == templePeople)
            {
                if (record == 3)
                    return true;
            }

            if (archive == mediumCommonPeople)
            {
                if (record == 4 || record == 5 || record == 6 || record == 18 || record == 36 || record == 37 || record == 38 || record == 42 || record == 43 || record == 52 || record == 53)
                    return true;
            }

            if (archive == flatPeople2)
            {
                if (record == 15)
                    return true;
            }

            if (archive == testBigFlats)
            {
                if (record == 4 || record == 5 || record == 6 || record == 7 || record == 19 || record == 37 || record == 38 || record == 39 || record == 43 || record == 44 || record == 53 || record == 54)
                    return true;
            }

            if (archive == kludgeTown)
            {
                if (record == 3)
                    return true;
            }

            if (archive == daggerfallPeople)
            {
                if (record == 2 || record == 3 || record == 6 || record == 9 || record == 12)
                    return true;
            }

            if (archive == wayrestPeople)
            {
                if (record == 2 || record == 3 || record == 12 || record == 15 || record == 16 || record == 18)
                    return true;
            }

            if (archive == sentinelPeople)
            {
                if (record == 5 || record == 6 || record == 7 || record == 8)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
