using CommandLine;

var parsedArguments = Parser.Default.ParseArguments<Options>(args);
await parsedArguments.WithParsedAsync(RunOptions);
//await parsedArguments.WithNotParsedAsync(HandleParseError); //This is currentlly not needed. It is here for a future reference.

//handle options
async static Task RunOptions(Options opts)
{
    if (opts.FindDuplicates)
    {
        VSCodeExtensionHandling extHandling = new(opts.Path);
        extHandling.Find();
        Delete(extHandling);
    }

    if (opts.CheckMarketplace)
    {
        VSCodeExtensionHandling extHandling = new(opts.Path);
        await extHandling.Check();
        Delete(extHandling);
    }
}

static void Delete(VSCodeExtensionHandling extHandling)
{
    foreach (var fullPath in extHandling.ExtensionsToDelete)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"About to Delete: {fullPath}");
    }
    var done = extHandling.ExtensionsToDelete.Count == 0;
    while (!done)
    {
        Console.WriteLine("You are about to delete the following folders. Are you sure you want to proceed? (yes / no)");
        var result = Console.ReadLine().ToLower();//TODO: Remove the ToLower
        if (result.Equals("yes"))//TODO: Add ignore flag and remove the ToLower
        {
            extHandling.Delete();
            done = true;
        }
        else if (result.Equals("no"))//TODO: Add ignore flag and remove the ToLower
        {
            done = true;
        }
    } 

    Console.ForegroundColor = ConsoleColor.White;
}

//async static Task HandleParseError(IEnumerable<Error> errs)
//{
//    await Task.Run(() =>
//    {
//        foreach (var err in errs)
//        {
//            //switch (err.Tag)
//            //{
//            //    case ErrorType.UnknownOptionError:
//            //        Console.WriteLine($"Unknown Option {(err as UnknownOptionError).Token}");
//            //        break;
//            //}
//            Console.WriteLine($"Error: {err}");
//            //Console.WriteLine("No arguments were provided. Send a directory to scan.");
//        }
//        //TOOD: Add error handling
//        //CommandLine.BadFormatConversionError
//        //handle errors
//        //throw new ArgumentException($"errs: {errs}");
//    });
//}

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Done!");
