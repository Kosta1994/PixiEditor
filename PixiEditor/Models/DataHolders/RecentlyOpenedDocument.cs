﻿using PixiEditor.Helpers;
using PixiEditor.Models.IO;
using PixiEditor.Models.Position;
using PixiEditor.Parser;
using PixiEditor.Parser.Skia;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.DataHolders
{
    [DebuggerDisplay("{FilePath}")]
    public class RecentlyOpenedDocument : NotifyableObject
    {
        private bool corrupt;

        private string filePath;

        private WriteableBitmap previewBitmap;

        public string FilePath
        {
            get => filePath;
            set
            {
                SetProperty(ref filePath, value);
                RaisePropertyChanged(nameof(FileName));
                RaisePropertyChanged(nameof(FileExtension));
                PreviewBitmap = null;
            }
        }

        public bool Corrupt { get => corrupt; set => SetProperty(ref corrupt, value); }

        public string FileName => Path.GetFileNameWithoutExtension(filePath);

        public string FullFileName => Path.GetFileName(filePath);

        public string FileExtension
        {
            get
            {
                if (Corrupt)
                {
                    return "? (Corrupt)";
                }
                string extension = Path.GetExtension(filePath).ToLower();
                return SupportedFilesHelper.IsExtensionSupported(extension) ? extension : $"? ({extension})";
            }
        }

        public WriteableBitmap PreviewBitmap
        {
            get
            {
                if (previewBitmap == null && !Corrupt)
                {
                    previewBitmap = LoadPreviewBitmap();
                }

                return previewBitmap;
            }
            private set => SetProperty(ref previewBitmap, value);
        }

        public RecentlyOpenedDocument(string path)
        {
            FilePath = path;
        }

        private WriteableBitmap LoadPreviewBitmap()
        {
            if (FileExtension == ".pixi")
            {
                SerializableDocument serializableDocument;

                try
                {
                    serializableDocument = PixiParser.Deserialize(filePath);
                }
                catch
                {
                    corrupt = true;
                    return null;
                }

                using Surface surface = Surface.Combine(serializableDocument.Width, serializableDocument.Height,
                          serializableDocument.Layers
                              .Where(x => x.Opacity > 0.8)
                              .Select(x => (x.ToSKImage(), new Coordinates(x.OffsetX, x.OffsetY))));

                return surface.ToWriteableBitmap();
            }
            else if (SupportedFilesHelper.IsExtensionSupported(FileExtension))
            {
                WriteableBitmap bitmap = null;

                try
                {
                    bitmap = Importer.ImportWriteableBitmap(FilePath);
                }
                catch
                {
                    corrupt = true;
                    return null;
                }
                
                if (bitmap == null)//prevent crash
                    return null;

                ImageFileMaxSizeChecker imageFileMaxSizeChecker = new ImageFileMaxSizeChecker()
                {
                    MaxAllowedWidthInPixels = Constants.MaxPreviewWidth,
                    MaxAllowedHeightInPixels = Constants.MaxPreviewHeight,
                };

                if (bitmap == null)
                    return null;

                return imageFileMaxSizeChecker.IsFileUnderMaxSize(bitmap) ?
                    bitmap
                    : bitmap.Resize(width: Constants.MaxPreviewWidth, height: Constants.MaxPreviewHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
            }

            return null;
        }
    }
}
