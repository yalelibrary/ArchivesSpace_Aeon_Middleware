using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ArchivesSpace_.Net_Client;
using ArchivesSpace_.Net_Client.Models;
using ASpace_Aeon_Middleware.Interfaces;

namespace ASpace_Aeon_Middleware
{
    public class AspaceSiteMultiServiceHandler : IAspaceSiteServiceHandler
    {
        private readonly ArchivesSpaceConnectionHandler _aSpaceConnectionHandler;
        private readonly List<ArchivesSpaceService> _serviceList;
        private readonly Dictionary<string, string> _siteMapping;

        public AspaceSiteMultiServiceHandler(ArchivesSpaceCredential cred)
        {
            _aSpaceConnectionHandler = new ArchivesSpaceConnectionHandler(cred, Configuration.ArchivesSpaceUri);
            _serviceList = new List<ArchivesSpaceService>();
            _siteMapping = Configuration.RepositoryMapping; //aeon sites may be named differently from their aspace repos
        }

        public async Task<ArchivesSpaceService> GetArchivesSpaceServiceAsync(string siteName)
        {
            if (_siteMapping.ContainsKey(siteName))
            {
                siteName = _siteMapping[siteName];
            }
            if (_serviceList.Exists(x => x.ActiveRepository.Name == siteName))
            {
                return _serviceList.FirstOrDefault(x => x.ActiveRepository.Name == siteName);
            }
            else
            {
                try
                {
                    var service = new ArchivesSpaceService(_aSpaceConnectionHandler);
                    await service.SetActiveRepositoryAsync(siteName);
                    _serviceList.Add(service);
                    return service;
                }
                catch (InvalidOperationException ex)
                {
                    //this means a wrong repository code was given
                    throw;
                }    
            }
        }
    }
}