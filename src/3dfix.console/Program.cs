using CommandLine;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ThreeDFix
{
    public class Program
    {
        public class Options
        {
            [Option('i', "input", Required = true, HelpText = "The absolute path to the 3d file to fix.")]
            public string Input { get; set; }

            [Option('o', "output", Required = false, HelpText = "The absolute path to the fixed 3d file to output. Defaults to appending '_fixed' to the InputFilePath file name.")]
            public string Output { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Set to true to enable extended output.")]
            public Boolean Verbose { get; set; }
        }

        public class Response
        {
            public Fixer Fixer { get; set; }
            public string Status { get; set; }
            public string Exception { get; set; }
        }

        public async static Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args).MapResult((Options opts) => RunOptions(opts), errs => Task.FromResult(0));
        }

        static async Task RunOptions(Options o)
        {
            var response = new Response();
            try
            {
                response.Fixer = new Fixer(o.Input, o.Output);
                await response.Fixer.FixAsync();
                response.Status = "Success";
            }
            catch (Exception exception)
            {
                response.Status = "Failure";
                response.Exception = o.Verbose ? exception.ToString() : exception.Message;
            }

            Console.WriteLine(
                JsonSerializer.Serialize(response,
                    new JsonSerializerOptions { WriteIndented = true }
                ));
        }
    }
}
