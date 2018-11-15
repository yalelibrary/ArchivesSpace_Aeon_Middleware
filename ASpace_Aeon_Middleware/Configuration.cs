using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ASpace_Aeon_Middleware
{
    public static class Configuration
    {
        public static Uri ArchivesSpaceUri = GetConfigSetting<Uri>("archivesSpaceApiUri");
        public static string ArchivesSpaceUsername = GetConfigSetting<string>("archivesSpaceUsername");
        public static string ArchivesSpacePassword = GetConfigSetting<string>("archivesSpacePassword");
        public static string DefaultSite = GetConfigSetting<string>("defaultRepositoryCode");
        public static Uri VapiUri = GetConfigSetting<Uri>("VgerApiAddress");
        public static Dictionary<string, string> RepositoryMapping = GetRepoMapDictionary();

        public static T GetConfigSetting<T>(string key)
        {
            try
            {
                var appSetting = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(appSetting))
                    throw new Exception(string.Format("Helpers.AppSettings: key {0} was null or empty", key));

                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)(converter.ConvertFromInvariantString(appSetting));

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //http://stackoverflow.com/questions/16549758/copy-key-values-from-namevaluecollection-to-generic-dictionary
        public static Dictionary<string,string> GetRepoMapDictionary()
        {
            var section =
                (NameValueCollection)ConfigurationManager.GetSection("RepoMapping");
            return section.ToDictionary();
        }
    }
}