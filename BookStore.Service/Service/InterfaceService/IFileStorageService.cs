using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.InterfaceService
{
    public interface IFileStorageService
    {
        Task<string> UploadFileToDefaultAsync(Stream fileStream, string fileName);
    }
}
