using CommandLine;

//TODO: Change command line to be: app.exe PATH <flags>?
//TODO: e.g. vsext.exe "c:\test" -c -f
internal class Options
{
    [Option('f', "findDuplicates", Required = false, HelpText = "Find duplicate VSExtenstions that were not removed by VSCode.")]
    public bool FindDuplicates { get; set; }

    // Omitting long name, defaults to name of property, ie "--verbose"
    [Option(
      Default = true,
      HelpText = "Prints all options to standard output.")]
    public bool Verbose { get; set; }

    [Option('c', "checkMarketplace",
      Default = false,
      HelpText = "Check if the extenstion still exists in the Visual Studio Code Marketplace.")]
    public bool CheckMarketplace { get; set; }

    [Value(0, MetaName = "offset", HelpText = "File offset.", Required = true)]
    public string Path { get; set; }
    
    
    
    
    
    
    
    //TODO: Check what is all the code below here

    //[Value(2, MetaName = "offset", HelpText = "File offset.")]
    //public long? Offset { get; set; }



    //[Usage(ApplicationAlias = "yourapp")]
    //public static IEnumerable<Example> Examples
    //{
    //    get
    //    {
    //        yield return new Example("Normal scenario", new Options { ExtenstionDirectory = "file.bin", OutputFile = "out.bin" });
    //        yield return new Example("Logging warnings", UnParserSettings.WithGroupSwitchesOnly(), new Options { ExtenstionDirectory = "file.bin", LogWarning = true });
    //        yield return new Example("Logging errors", new[] { UnParserSettings.WithGroupSwitchesOnly(), UnParserSettings.WithUseEqualTokenOnly() }, new Options { ExtenstionDirectory = "file.bin", LogError = true });
    //    }
    //}

    //[Usage(ApplicationAlias = "yourapp")]
    //public static IEnumerable<Example> Examples
    //{
    //    get
    //    {
    //        return new List<Example>() {
    //    new Example("Convert file to a trendy format", new Options { filename = "file.bin" })
    //  };
    //    }
    //}
}
