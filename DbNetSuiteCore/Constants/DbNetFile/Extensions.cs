using DbNetSuiteCore.Enums.DbNetFile;
using System.Collections.Generic;

namespace DbNetSuiteCore.Constants.DbNetFile
{
    public class Extensions
    {
        public static Dictionary<FileType, List<string>> FileTypeExtensions = new Dictionary<FileType, List<string>>()
        {
            { FileType.Image,  new List<string>() { "gif", "jpg", "jpeg", "png", "apng", "avif", "webp", "bmp", "gif", "svg" } },
            { FileType.Audio,  new List<string>() { "mp3", "wav", "flac", "mpeg" } },
            { FileType.Video,  new List<string>() { "mp4", "webm", "ogg", "mov" } },
            { FileType.Html,  new List<string>() { "htm", "html" } },
            { FileType.Pdf,  new List<string>() { "pdf" } }
     };
    }
}