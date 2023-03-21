using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace VisFileManager.Validators
{
    public static class KnownNamesProvider
    {
        private static Dictionary<string, string> CachedKnownNames;
        private static object _cacheLock = new object();

        public static string GetKnownNamePath(string path)
        {
            lock (_cacheLock)
            {
                if (CachedKnownNames == null)
                {
                    BuildCache();
                }
                if (CachedKnownNames.ContainsKey(path.ToUpper(CultureInfo.InvariantCulture)))
                {
                    return CachedKnownNames[path.ToUpper(CultureInfo.InvariantCulture)];
                }
                return null;
            }
        }

        private static void BuildCache()
        {
            lock (_cacheLock)
            {
                CachedKnownNames = new Dictionary<string, string>();

                foreach(var knownFolder in KnownFolders.All)
                {
                    var localizedName = knownFolder.LocalizedName;
                    if (!string.IsNullOrEmpty(localizedName) && !CachedKnownNames.ContainsKey(localizedName.ToUpper(CultureInfo.InvariantCulture)))
                    {
                        CachedKnownNames.Add(localizedName.ToUpper(CultureInfo.InvariantCulture), knownFolder.Path);
                    }
                    else if(!CachedKnownNames.ContainsKey(knownFolder.CanonicalName.ToUpper(CultureInfo.InvariantCulture)))
                    {
                        CachedKnownNames.Add(knownFolder.CanonicalName.ToUpper(CultureInfo.InvariantCulture), knownFolder.Path);
                    }
                }

                //fix documents 
                if (CachedKnownNames.ContainsKey("DOCUMENTS"))
                {
                    CachedKnownNames["DOCUMENTS"] = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
            }
        }
    }
}
