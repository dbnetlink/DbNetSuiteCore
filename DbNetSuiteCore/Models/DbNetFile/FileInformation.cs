using DbNetSuiteCore.Enums.DbNetFile;
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
        public DateTime LastModified { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastAccessed { get; set; }
        public long Length { get; set; }

        public FileInformation(IFileInfo fileInfo)
        {
            Name = fileInfo.Name;
            IsDirectory = fileInfo.IsDirectory;
            LastModified = fileInfo.LastModified.UtcDateTime;
            Length = fileInfo.Length;

            if (!fileInfo.IsDirectory)
            {
                FileInfo systemfileInfo = new FileInfo(fileInfo.PhysicalPath);
                Created = systemfileInfo.CreationTime;
                LastAccessed = systemfileInfo.LastAccessTime;
                Extension = systemfileInfo.Extension;
            }
        }
    }
}