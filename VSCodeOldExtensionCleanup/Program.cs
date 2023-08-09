using Microsoft.VisualBasic;
using System;
using System.Text.RegularExpressions;

Console.ForegroundColor = ConsoleColor.White;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

string directory = null;

if (args.Length > 1)
{
    throw new ArgumentException("Send a directory to scan", nameof(args));
}
else if (args.Length == 1)
{
    foreach (var arg in args)
    {
        Console.WriteLine($"Argument={arg}");
        if (Directory.Exists(arg))
        {
            directory = arg;
        }
        else
        {
            throw new ArgumentException("Send an EXISTING directory to scan", nameof(args));
        }
    }

    DirectoryInfo di = new(directory);
    // String - key - d.Name;
    // Tuple :
    // String - Version - d.Name.Split("-")
    // String[] - Directories - Version < HighestVersion
    Dictionary<string, IList<Tuple<Version, string>>> directoriesCandidatesToDelete = new();
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
                var versions = new List<Tuple<Version, string>>();
                directoriesCandidatesToDelete.Add(key, versions);
            }
            directoriesCandidatesToDelete[key].Add(new Tuple<Version, string>(new Version(version), d.Name));
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{d.Name}]: This folders didn't match. Ignoring. {keyMatches.Count} - {versionMatches.Count}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    var directoriesToDelete = RemoveHighestVersion(directoriesCandidatesToDelete);

    DeleteRedundantDirectories(directoriesToDelete);

    Console.WriteLine("Done!");
}
else
{
    Console.WriteLine("No arguments");
}

void DeleteRedundantDirectories(IList<string> directoriesToDelete)
{
    foreach (var dir in directoriesToDelete)
    {
        var fullPath = Path.Combine(directory, dir);
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
            Console.WriteLine($"Path Not FoundBad: {fullPath}");
        }
    }

    Console.ForegroundColor = ConsoleColor.White;
}

IList<string> RemoveHighestVersion(Dictionary<string, IList<Tuple<Version, string>>> directoriesCandidatesToDelete) //TODO: Give a better  name
{
    var result = new List<string>();
    foreach (var key in directoriesCandidatesToDelete)
    {
        if (key.Value.Count > 1)
        {
            // Sort the tuples by the first string in the tuple
            var sorted_tuples = key.Value.OrderBy(tuple => tuple.Item1).ToList();

            Console.WriteLine($"Skipping: {sorted_tuples[^1]}");

            for (var i = 0; i < sorted_tuples.Count - 1; i++)
            {
                var tuple = sorted_tuples[i];
                Console.WriteLine($"Selected for Deleting: {tuple.Item1} {tuple.Item2}");
                result.Add(tuple.Item2);
            }
        }
    }

    return result;
}






