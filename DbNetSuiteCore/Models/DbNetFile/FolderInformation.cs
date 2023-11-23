using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.Linq;

namespace DbNetSuiteCore.Models.DbNetFile
{
    public class FolderInformation
    {
        public string Name { get; set; }
        public string ParentFolder { get; set; } = string.Empty;

        public List<FolderInformation> SubFolders { get; set; } = new List<FolderInformation>();

        public FolderInformation(IFileInfo fileInfo, string parentFolder)
        {
            Name = fileInfo.Name;
            ParentFolder = parentFolder;
        }

        public FolderInformation(string folder)
        {
            Name = folder.Split("/").Last();
            ParentFolder = folder;
        }
    }
}