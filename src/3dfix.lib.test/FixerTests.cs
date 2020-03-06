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
            string inputFilePath = $"{Environment.CurrentDirectory}\\assets\\brokenbottle_class.3mf";

            var fixer = new Fixer(inputFilePath);

            DeleteFixedFile(fixer.OutputFilePath);

            await fixer.FixAsync();

            Assert.True(File.Exists(fixer.OutputFilePath));

            DeleteFixedFile(fixer.OutputFilePath);
        }

        [Fact]
        public async void CanFixFilesViaClassRelativePath()
        {
            string inputFilePath = $".\\assets\\brokenbottle_class_relative.3mf";

            var fixer = new Fixer(inputFilePath);

            DeleteFixedFile(fixer.OutputFilePath);

            await fixer.FixAsync();

            Assert.True(File.Exists(fixer.OutputFilePath));

            DeleteFixedFile(fixer.OutputFilePath);
        }

        [Fact]
        public async void CanFixFilesViaConsole()
        {
            string inputFilePath = $"{Environment.CurrentDirectory}\\assets\\brokenbottle_console.3mf";
            string outputFilePath = $"{Environment.CurrentDirectory}\\assets\\brokenbottle_console_fixed.3mf";

            DeleteFixedFile(outputFilePath);

            await Program.Main(new string[] { "--i", inputFilePath, "--o", outputFilePath });

            Assert.True(File.Exists(outputFilePath));

            DeleteFixedFile(outputFilePath);
        }

        private void DeleteFixedFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }
}
