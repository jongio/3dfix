using System;
using System.IO;
using Xunit;

namespace ThreeDFix.test
{
    public class FixerTests
    {

        [Fact]
        public async void CanFixFilesViaClass()
        {
            string inputFile = $"{Environment.CurrentDirectory}\\assets\\brokenbottle_class.3mf";
            string outputFile = $"{Environment.CurrentDirectory}\\assets\\brokenbottle_class_fixed.3mf";


            var fixer = new Fixer();

            DeleteFile(outputFile);

            outputFile = await fixer.FixAsync(inputFile);

            Assert.True(File.Exists(outputFile));

            DeleteFile(outputFile);
        }

        [Fact]
        public async void CanFixStreamViaClass()
        {
            string inputFile = $"{Environment.CurrentDirectory}\\assets\\brokenbottle_class_stream.3mf";
            string outputFile = $"{Environment.CurrentDirectory}\\assets\\brokenbottle_class_stream_fixed.3mf";

            DeleteFile(outputFile);

            using var inputStream = File.OpenRead(inputFile);

            var fixer = new Fixer();

            using (var stream = await fixer.FixAsync(inputStream))
            {
                using var outfile = File.Create(outputFile);

                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(outfile);
            }

            var fileBytes = File.ReadAllBytes(outputFile);

            Assert.True(fileBytes.Length > 0);

            DeleteFile(outputFile);
        }

        [Fact]
        public async void CanFixFilesViaClassRelativePath()
        {
            string inputFile = $".\\assets\\brokenbottle_class_relative.3mf";
            string outputFile = $".\\assets\\brokenbottle_class_relative_fixed.3mf";


            var fixer = new Fixer();

            DeleteFile(outputFile);

            await fixer.FixAsync(inputFile);

            Assert.True(File.Exists(fixer.OutputFile));

            DeleteFile(fixer.OutputFile);
        }

        [Fact]
        public async void CanFixFilesViaConsole()
        {
            string inputFile = $"{Environment.CurrentDirectory}\\assets\\brokenbottle_console.3mf";
            string outputFile = $"{Environment.CurrentDirectory}\\assets\\brokenbottle_console_fixed.3mf";

            DeleteFile(outputFile);

            await Program.Main(new string[] { "--i", inputFile, "--o", outputFile });

            Assert.True(File.Exists(outputFile));

            DeleteFile(outputFile);
        }

        private void DeleteFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }
}
