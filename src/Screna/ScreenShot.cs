﻿using System;
using System.Drawing;
using Captura;
using Captura.Models;

namespace Screna
{
    /// <summary>
    /// Contains methods for taking ScreenShots
    /// </summary>
    public static class ScreenShot
    {
        /// <summary>
        /// Captures the entire Desktop.
        /// </summary>
        /// <param name="IncludeCursor">Whether to include the Mouse Cursor.</param>
        /// <returns>The Captured Image.</returns>
        public static IBitmapImage Capture(bool IncludeCursor = false)
        {
            return Capture(WindowProvider.DesktopRectangle, IncludeCursor);
        }

        /// <summary>
        /// Capture transparent Screenshot of a Window.
        /// </summary>
        /// <param name="Window">The <see cref="IWindow"/> to Capture.</param>
        /// <param name="IncludeCursor">Whether to include Mouse Cursor.</param>
        public static IBitmapImage CaptureTransparent(IWindow Window, bool IncludeCursor = false)
        {
            if (Window == null)
                throw new ArgumentNullException(nameof(Window));

            var backdrop = new WindowScreenShotBackdrop(Window);

            backdrop.ShowWhite();

            var r = backdrop.Rectangle;

            // Capture screenshot with white background
            using (var whiteShot = CaptureInternal(r))
            {
                backdrop.ShowBlack();

                // Capture screenshot with black background
                using (var blackShot = CaptureInternal(r))
                {
                    backdrop.Dispose();

                    var transparentImage = Extensions.DifferentiateAlpha(whiteShot, blackShot);

                    if (transparentImage == null)
                        return null;

                    // Include Cursor only if within window
                    if (IncludeCursor && r.Contains(MouseCursor.CursorPosition))
                    {
                        using (var g = Graphics.FromImage(transparentImage))
                            MouseCursor.Draw(g, P => new Point(P.X - r.X, P.Y - r.Y));
                    }

                    return new DrawingImage(transparentImage.CropEmptyEdges());
                }
            }
        }

        static Bitmap CaptureInternal(Rectangle Region, bool IncludeCursor = false)
        {
            var bmp = new Bitmap(Region.Width, Region.Height);

            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(Region.Location, Point.Empty, Region.Size, CopyPixelOperation.SourceCopy);

                if (IncludeCursor)
                    MouseCursor.Draw(g, P => new Point(P.X - Region.X, P.Y - Region.Y));

                g.Flush();
            }

            return bmp;
        }

        /// <summary>
        /// Captures a Specific Region.
        /// </summary>
        /// <param name="Region">A <see cref="Rectangle"/> specifying the Region to Capture.</param>
        /// <param name="IncludeCursor">Whether to include the Mouse Cursor.</param>
        /// <returns>The Captured Image.</returns>
        public static IBitmapImage Capture(Rectangle Region, bool IncludeCursor = false)
        {
            return new DrawingImage(CaptureInternal(Region, IncludeCursor));
        }
    }
}
