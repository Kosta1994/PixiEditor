﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Colors;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using SkiaSharp;
using System.Windows;

namespace PixiEditor.Models.Tools.Tools
{
    public class BrightnessTool : BitmapOperationTool
    {
        private const float CorrectionFactor = 5f; // Initial correction factor

        private readonly List<Coordinates> pixelsVisited = new List<Coordinates>();
        private List<Coordinates> circleCache = null;
        private int cachedCircleSize = -1;

        public BrightnessTool()
        {
            ActionDisplay = "Draw on pixel to make it brighter. Hold Ctrl to darken.";
            Toolbar = new BrightnessToolToolbar(CorrectionFactor);
        }

        public override bool UsesShift => false;

        public override string Tooltip => "Makes pixel brighter or darker pixel (U). Hold Ctrl to make pixel darker.";

        public BrightnessMode Mode { get; set; } = BrightnessMode.Default;

        public override void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            base.OnRecordingLeftMouseDown(e);
            pixelsVisited.Clear();
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                ActionDisplay = "Draw on pixel to make it darker. Release Ctrl to brighten.";
            }
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                ActionDisplay = "Draw on pixel to make it brighter. Hold Ctrl to darken.";
            }
        }

        public override void Use(Layer layer, List<Coordinates> coordinates, SKColor color)
        {
            int toolSize = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;
            float correctionFactor = Toolbar.GetSetting<FloatSetting>("CorrectionFactor").Value;
            Mode = Toolbar.GetEnumSetting<BrightnessMode>("BrightnessMode").Value;

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                ChangeBrightness(layer, coordinates[0], toolSize, -correctionFactor);
            }
            else
            {
                ChangeBrightness(layer, coordinates[0], toolSize, correctionFactor);
            }
        }

        public void ChangeBrightness(Layer layer, Coordinates coordinates, int toolSize, float correctionFactor)
        {
            if (cachedCircleSize != toolSize)
            {
                cachedCircleSize = toolSize;
                circleCache = CircleTool.GenerateMidpointEllipse(toolSize / 2.0, toolSize / 2.0, 0, 0);
                circleCache.Sort((a, b) => a.Y != b.Y ? a.Y - b.Y : a.X - b.X);
                for (int i = circleCache.Count - 2; i >= 1; i--)
                {
                    if (circleCache[i].Y == circleCache[i - 1].Y && circleCache[i].Y == circleCache[i + 1].Y)
                    {
                        circleCache.RemoveAt(i);
                    }
                }
            }

            int radius = (int)Math.Ceiling(toolSize / 2f);
            layer.DynamicResizeAbsolute(coordinates.X + radius, coordinates.Y + radius, coordinates.X - radius, coordinates.Y - radius);

            for (int i = 0; i < circleCache.Count; i += 2)
            {
                Coordinates left = circleCache[i];
                Coordinates right = circleCache[i + 1];
                int y = left.Y + coordinates.Y;

                for (int x = left.X + coordinates.X; x < right.X + coordinates.X; x++)
                {
                    if (Mode == BrightnessMode.Default)
                    {
                        Coordinates here = new(x, y);
                        if (pixelsVisited.Contains(here))
                            continue;

                        pixelsVisited.Add(here);
                    }

                    SKColor pixel = layer.GetPixelWithOffset(x, y);
                    SKColor newColor = ExColor.ChangeColorBrightness(
                        pixel,
                        correctionFactor);
                    layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(x - layer.OffsetX, y - layer.OffsetY, newColor);
                }
            }
            Int32Rect rect = new(coordinates.X - radius, coordinates.Y - radius, radius * 2, radius * 2);
            layer.InvokeLayerBitmapChange(rect);
        }
    }
}
