using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Printing3D;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ThreeDFix
{
    public class Fixer
    {
        private string intputFilePath = string.Empty;
        private string outputFilePath = string.Empty;

        /// <summary>
        /// The absolute path to the 3d file to fix.
        /// </summary>
        public string InputFilePath
        {
            get { return intputFilePath; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("InputFilePath");
                }
                intputFilePath = GetAbsolutePath(value);
            }
        }

        /// <summary>
        /// The absolute path to the fixed 3d file to output. Defaults to appending '_fixed' to the InputFilePath file name.
        /// </summary>
        public string OutputFilePath
        {
            get { return outputFilePath; }
            set
            {

                outputFilePath = SetOutputFilePath(value);

            }
        }

        /// <summary>
        /// A class to fix the geometry of a 3d print file.
        /// </summary>
        /// <param name="inputFilePath">The absolute path to the 3d file to fix.</param>
        /// <param name="outputFilePath">The absolute path to the fixed 3d file to output. Defaults to appending '_fixed' to the InputFilePath file name.</param>
        public Fixer(string inputFilePath, string outputFilePath = "")
        {
            InputFilePath = inputFilePath;
            OutputFilePath = outputFilePath;
        }

        /// <summary>
        /// Fixes the file provided by the inputFilePath property and outputs the fixed file to the outputFilePath property.
        /// </summary>
        /// <returns></returns>
        public async Task FixAsync()
        {
            if (string.IsNullOrEmpty(InputFilePath))
            {
                throw new ArgumentNullException("InputFilePath");
            }

            var package = new Printing3D3MFPackage();
            using var stream = await FileRandomAccessStream.OpenAsync(InputFilePath, FileAccessMode.ReadWrite);
            var model = await package.LoadModelFromPackageAsync(stream);
            
            await model.RepairAsync();
            await package.SaveModelToPackageAsync(model);

            using var outstream = WindowsRuntimeStreamExtensions.AsStream(await package.SaveAsync());
            using var outfile = File.Create(OutputFilePath);

            outstream.Seek(0, SeekOrigin.Begin);
            outstream.CopyTo(outfile);
        }

        private string SetOutputFilePath(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (string.IsNullOrEmpty(InputFilePath))
                {
                    throw new Exception("InputFilePath must be set before OutputFilePath.");
                }
                else
                {
                    return string.Concat(Path.GetDirectoryName(InputFilePath), "\\",
                                            Path.GetFileNameWithoutExtension(InputFilePath),
                                            "_fixed",
                                            Path.GetExtension(InputFilePath));
                }
            }
            else
            {
                return GetAbsolutePath(value);
            }
        }

        public static String GetAbsolutePath(String path)
        {
            return GetAbsolutePath(null, path);
        }

        public static String GetAbsolutePath(String basePath, String path)
        {
            if (path == null)
                return null;
            if (basePath == null)
                basePath = Path.GetFullPath("."); // quick way of getting current working directory
            else
                basePath = GetAbsolutePath(null, basePath); // to be REALLY sure ;)
            String finalPath;
            // specific for windows paths starting on \ - they need the drive added to them.
            // I constructed this piece like this for possible Mono support.
            if (!Path.IsPathRooted(path) || "\\".Equals(Path.GetPathRoot(path)))
            {
                if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    finalPath = Path.Combine(Path.GetPathRoot(basePath), path.TrimStart(Path.DirectorySeparatorChar));
                else
                    finalPath = Path.Combine(basePath, path);
            }
            else
                finalPath = path;
            // resolves any internal "..\" to get the true full path.
            return Path.GetFullPath(finalPath);
        }
    }
}
