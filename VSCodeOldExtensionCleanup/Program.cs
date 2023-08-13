using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

Console.ForegroundColor = ConsoleColor.White;

if (args.Length > 1)
{
    throw new ArgumentException("Send a directory to scan", nameof(args));
}
else if (args.Length == 1)
{
    Console.WriteLine($"Argument={arg}");
    DirectoryInfo di = new(args[0]);
    if (!Directory.Exists(di.FullName)) {
        throw new ArgumentException("Send an EXISTING directory to scan", nameof(args));
    }
    // String - key - d.Name;
    // Tuple :
    // String - Version - d.Name.Split("-")
    // String[] - Directories - Version < HighestVersion
    Dictionary<string, IList<(Version, string)>> directoriesCandidatesToDelete = new();
    foreach (var d in di.GetDirectories())
    {
        var keyPattern = @"^.*?(.*)-[0-9].+";
        var keyMatches = Regex.Matches(d.Name, keyPattern);
        var versionPattern = @"\d+\.\d+\.\d+";
        var versionMatches = Regex.Matches(d.Name, versionPattern);
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
            directoriesCandidatesToDelete[key].Add(new(new Version(version), d.FullName));
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{d.Name}]: This folders didn't match. Ignoring. {keyMatches.Count} - {versionMatches.Count}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    directoriesCandidatesToDelete.Where(key => key.Value.Count == 1).ToList().ForEach(item => directoriesCandidatesToDelete.Remove(item.Key));

    var directoriesToDelete = RemoveHighestVersion(directoriesCandidatesToDelete);

    DeleteRedundantDirectories(directoriesToDelete);

    Console.WriteLine("Done!");
}
else
{
    Console.WriteLine("No arguments were provided. Send a directory to scan.");
}

void DeleteRedundantDirectories(IEnumerable<(Version, string)> directoriesToDelete)
{
    foreach (var dir in directoriesToDelete)
    {
        var fullPath = dir.Item2;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"About to Delete: {fullPath}");
        if (Directory.Exists(fullPath))
        {
            Console.WriteLine($"Path Found: {fullPath}");
            Console.ForegroundColor = ConsoleColor.Red;
            Directory.Delete(fullPath, true);
            Console.WriteLine($"Deleted: {fullPath}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Path Not Found. Bad: {fullPath}");
        }
    }

    Console.ForegroundColor = ConsoleColor.White;
}

IEnumerable<(Version, string)> RemoveHighestVersion(Dictionary<string, IList<(Version, string)>> directoriesCandidatesToDelete) //TODO: Give a better  name
{
    var result = new List<(Version, string)>();
    foreach (var key in directoriesCandidatesToDelete)
    {
        if (key.Value.Count == 1)
        {
            throw new InvalidOperationException("This should not have happened");
        }
        // Sort the tuples by the version in the tuple
        var highestVersion = key.Value.OrderBy(tuple => tuple.Item1).Last();
        key.Value.Remove(highestVersion);
        result.AddRange(key.Value);
        Console.WriteLine($"Skipping: {highestVersion.Item2}");
    }

    return result;
}

public class Clean
{
    //TODO: Move all the logic above to a method in this class
}
