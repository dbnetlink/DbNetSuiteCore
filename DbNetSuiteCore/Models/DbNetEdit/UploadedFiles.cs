using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class UploadedFiles 
    {
        private IFormCollection _formCollection;
        public IFormCollection FormCollection
        { 
            get { return _formCollection; } 
            set { 
                _formCollection = value; 
                foreach (IFormFile file in Files)
                {
                    FileBytes[file.Name] = GetFileBytes(file).Result;
                }
            }
        }

        public IFormFileCollection Files => FormCollection.Files;

        public Dictionary<string, byte[]> FileBytes{ get; set; } = new Dictionary<string, byte[]>();

        private async Task<byte[]> GetFileBytes(IFormFile formFile)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await formFile.CopyToAsync(ms);
                return ms.ToArray();
            }
        }
    }
}
