using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

internal class VSCodeExtensionHandling
{
    private DirectoryInfo[] ExtensionsList { get; init; }
    private Dictionary<string, string> ExtensionsDictionary { get; init; }
    public List<string> ExtensionsToDelete { get; init; }

    public VSCodeExtensionHandling(string extenstionsDirectory)
    {
        if (!Directory.Exists(extenstionsDirectory))
        {
            throw new ArgumentException("Send an EXISTING directory to scan", nameof(extenstionsDirectory));
        }

        ExtensionsList = new DirectoryInfo(extenstionsDirectory).GetDirectories();
        ExtensionsDictionary = ExtensionsList.ToDictionary(d => d.Name, d => d.FullName);
        ExtensionsToDelete = new List<string>();
    }   

    public async Task<IEnumerable<string>> Check()
    {
        ExtensionsToDelete.Clear();
        while (!NetworkInterface.GetIsNetworkAvailable())
        {
            Console.WriteLine("Waiting for network to come online. If you want to exit press Command\\Ctrl+C.");
            Thread.Sleep(1000);
        }

        //TODO: Change this code to run parallel
        var extenstionsList = CheckIfExists(GetExtenstionsListByVersion().Select(item => item.Key));
        await foreach (var res in extenstionsList)
        {
            if (res != null)
            {
                ExtensionsToDelete.AddRange(ExtensionsDictionary.Where(ext => ext.Key.StartsWith(res + "-")).Select(ext => ext.Value));
            }
        }

        return ExtensionsToDelete;
    }

    public IEnumerable<string> Find()
    {
        ExtensionsToDelete.Clear();
        var directoriesCandidatesToDelete = GetExtenstionsListByVersion();
        directoriesCandidatesToDelete.Where(key => key.Value.Count == 1).ToList().ForEach(item => directoriesCandidatesToDelete.Remove(item.Key)); //TODO: Check if this can be optimized
        return RemoveHighestVersion(directoriesCandidatesToDelete);
    }

    public void Delete()
    {
        foreach (var fullPath in ExtensionsToDelete)
        {
            if (Directory.Exists(fullPath))
            {
                Console.WriteLine($"Path Found: {fullPath}");
                Directory.Delete(fullPath, true);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Deleted: {fullPath}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Path Not Found. Bad: {fullPath}");
            }
        }
    }

    private IEnumerable<string> RemoveHighestVersion(Dictionary<string, IList<(Version, string)>> directoriesCandidatesToDelete)
    {
        ExtensionsToDelete.Clear();
        foreach (var key in directoriesCandidatesToDelete)
        {
            if (key.Value.Count == 1)
            {
                throw new InvalidOperationException("This should not have happened");
            }
            // Sort the tuples by the version in the tuple
            var highestVersion = key.Value.OrderBy(tuple => tuple.Item1).Last();
            key.Value.Remove(highestVersion);
            ExtensionsToDelete.AddRange(key.Value.Select(item=>item.Item2));
            Console.WriteLine($"Skipping: {highestVersion.Item2}");
        }

        return ExtensionsToDelete;
    }

    //TODO: Fix documentation
    /// <summary>
    /// Key:string - extension name
    /// Value:IEnumerable<(Version, string) - Version < HighestVersion, Directories 
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, IList<(Version, string)>> GetExtenstionsListByVersion()
    {
        Dictionary<string, IList<(Version, string)>> directoriesCandidatesToDelete = new();

        foreach (var ext in ExtensionsList)
        {
            var keyPattern = @"^.*?(.*)-[0-9].+";
            var keyMatches = Regex.Matches(ext.Name, keyPattern);
            var versionPattern = @"\d+\.\d+\.\d+";
            var versionMatches = Regex.Matches(ext.Name, versionPattern);
            if (keyMatches.Count > 0 && versionMatches.Count > 0)
            {
                var key = keyMatches[0].Groups[1].Value;
                // Console.WriteLine($"Key: {key}");
                var version = versionMatches[0].Value;
                //Console.WriteLine($"Version: {version}");
                if (!directoriesCandidatesToDelete.ContainsKey(key))
                {
                    var versions = new List<(Version, string)>();
                    directoriesCandidatesToDelete.Add(key, versions);
                }
                directoriesCandidatesToDelete[key].Add(new(new Version(version), ext.FullName));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;//TODO: Take this out of here
                Console.WriteLine($"[{ext.Name}]: This folders didn't match. Ignoring. {keyMatches.Count} - {versionMatches.Count}");//TODO: Change to Debug? Take out? Something else?
                Console.ForegroundColor = ConsoleColor.White;//TODO: Take this out of here
            }
        }

        return directoriesCandidatesToDelete;
    }

    private async IAsyncEnumerable<string> CheckIfExists(IEnumerable<string> extenstionsList)
    {
        foreach (var extName in extenstionsList)
        {
            //https://marketplace.visualstudio.com/items?itemName=redhat.java
            var url = $"https://marketplace.visualstudio.com/items?itemName={extName}";
            HttpClient client = new(); //TODO: Make this a static class member?
            Console.WriteLine($"Start Checking: {url}");
            var response = await client.GetAsync(url);
            Console.WriteLine($"Done Checking: {url}");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                yield return extName;
            }
        }
    }
}
