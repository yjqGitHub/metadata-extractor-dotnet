using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MetadataExtractor.Tools.FileProcessor
{
    internal abstract class FileHandlerBase : IFileHandler
    {
        private static readonly ISet<string> _supportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "jpg", "jpeg", "png", "gif", "bmp", "ico", "webp", "pcx", "ai", "eps",
            "nef", "crw", "cr2", "orf", "arw", "raf", "srw", "x3f", "rw2", "rwl",
            "tif", "tiff", "psd", "dng",
            "3g2", "3gp", "m4v", "mov", "mp4"
        };

        private int _processedFileCount;
        private int _exceptionCount;
        private int _errorCount;
        private long _processedByteCount;

        public virtual void OnStartingDirectory(string directoryPath)
        {}

        public bool ShouldProcess(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return extension.Length > 1 && _supportedExtensions.Contains(extension.Substring(1));
        }

        public virtual void OnBeforeExtraction(string filePath, string relativePath, TextWriter log)
        {
            _processedFileCount++;
            _processedByteCount += new FileInfo(filePath).Length;
        }

        public virtual void OnExtractionError(string filePath, Exception exception, TextWriter log)
        {
            _exceptionCount++;
            log.Write("\t[{0}] {1}\n", exception.GetType().Name, filePath);
        }

        public virtual void OnExtractionSuccess(string filePath, IReadOnlyList<Directory> directories, string relativePath, TextWriter log)
        {
            if (!directories.Any(d => d.HasError))
                return;

            log.WriteLine(filePath);
            foreach (var directory in directories)
            {
                if (!directory.HasError)
                    continue;
                foreach (var error in directory.Errors)
                {
                    log.Write("\t[{0}] {1}\n", directory.Name, error);
                    _errorCount++;
                }
            }
        }

        public void OnScanCompleted(TextWriter log)
        {
            if (_processedFileCount <= 0)
                return;

            log.WriteLine(
                "Processed {0:#,##0} files ({1:#,##0} bytes) with {2:#,##0} exceptions and {3:#,##0} file errors\n",
                _processedFileCount, _processedByteCount, _exceptionCount, _errorCount);
        }
    }
}