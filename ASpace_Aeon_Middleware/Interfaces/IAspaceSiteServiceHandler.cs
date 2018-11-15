using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ArchivesSpace_.Net_Client;

namespace ASpace_Aeon_Middleware.Interfaces
{
    public interface IAspaceSiteServiceHandler
    {
        Task<ArchivesSpaceService> GetArchivesSpaceServiceAsync(string siteName);
    }
}
