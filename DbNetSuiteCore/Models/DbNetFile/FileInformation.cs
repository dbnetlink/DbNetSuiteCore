using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace DbNetSuiteCore.Models.DbNetFile
{
    public class FileInformation
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public bool IsDirectory { get; set; }
        public DateTime? LastModified { get; set; } = null;
        public DateTime? Created { get; set; } = null;
        public DateTime? LastAccessed { get; set; } = null;
        public long? Length { get; set; } = null;
        public string ParentFolder { get; set; }

        public FileInformation(IFileInfo fileInfo, string parentFolder)
        {
            Name = fileInfo.Name;
            IsDirectory = fileInfo.IsDirectory;
            ParentFolder = parentFolder;

            if (!fileInfo.IsDirectory)
            {
                Length = fileInfo.Length;
                LastModified = fileInfo.LastModified.UtcDateTime;
                FileInfo systemfileInfo = new FileInfo(fileInfo.PhysicalPath);
                Created = systemfileInfo.CreationTime;
                LastAccessed = systemfileInfo.LastAccessTime;
                Extension = systemfileInfo.Extension;
            }
        }
    }
}