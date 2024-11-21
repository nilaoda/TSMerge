using System.CommandLine;
using System.Diagnostics;
using static TSMerge.Utils;

var inputOption = new Option<string[]>("-i", description: "Add inputs") { IsRequired = true, Arity = ArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = false, ArgumentHelpName = "FILE" };
var patternSizeOption = new Option<double>("-p", description: "Pattern size to search (in MB)", getDefaultValue: () => 10) { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "SIZE" };
var outputArgument = new Argument<string>(name: "OUTPUT", description: "File output name");
var rootCommand = new RootCommand("Merge MPEG-TS files into one") { inputOption, outputArgument, patternSizeOption };
rootCommand.SetHandler(DoWork, inputOption, outputArgument, patternSizeOption);
rootCommand.Invoke(args);
return;

void DoWork(string[] inputs, string output, double patternSizeInMb)
{
    try
    {
        var sw = Stopwatch.StartNew();
        MergeMultipleFiles(inputs, output, patternSizeInMb);
        sw.Stop();
        Console.WriteLine($"Task cost: {sw.Elapsed}.");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        Console.ResetColor();
        Environment.ExitCode = -1;
    }
}