using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Printing3D;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ThreeDFix
{
    public class Fixer
    {
        private string inputFile = string.Empty;
        private string outputFile = string.Empty;
        private Stream inputStream;

        /// <summary>
        /// The absolute path to the 3d file to fix.
        /// </summary>
        public string InputFile
        {
            get { return inputFile; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("InputFile");
                }
                inputFile = GetAbsolutePath(value);
            }
        }

        /// <summary>
        /// The input file as a Stream.
        /// </summary>
        public Stream InputStream
        {
            get { return inputStream; }
            set
            {
                inputStream = value ?? throw new ArgumentNullException("InputStream");
            }
        }

        /// <summary>
        /// The absolute path to the fixed 3d file to output. Defaults to appending '_fixed' to the InputFile file name.
        /// </summary>
        public string OutputFile
        {
            get
            {
                // If we don't yet have an outputFile, then we need to set it to empty string, so the set handler sets the default value.
                if (string.IsNullOrEmpty(outputFile))
                {
                    OutputFile = string.Empty;
                }
                return outputFile;
            }
            set
            {
                outputFile = SetOutputFile(value);
            }
        }

        /// <summary>
        /// A class to fix the geometry of a 3d print file.
        /// </summary>
        public Fixer()
        {
        }

        /// <summary>
        /// Fixes 3D printing inputFile and returns path to fixed file.
        /// </summary>
        /// <param name="inputFile">The absolute path to the file to be fixed.</param>
        /// <param name="outputFile">The absolute path to the output fixed file. Defaults to appending '_fixed' to the InputFile file name.</param>
        /// <returns>The absolute path to the fixed file.</returns>
        public async Task<string> FixAsync(string inputFile, string outputFile = "")
        {
            InputFile = inputFile;
            OutputFile = outputFile;

            var package = new Printing3D3MFPackage();
            using var stream = await FileRandomAccessStream.OpenAsync(InputFile, FileAccessMode.ReadWrite);
            var model = await package.LoadModelFromPackageAsync(stream);

            await model.RepairAsync();
            await package.SaveModelToPackageAsync(model);

            using var outstream = WindowsRuntimeStreamExtensions.AsStream(await package.SaveAsync());
            using var outfile = File.Create(OutputFile);

            outstream.Seek(0, SeekOrigin.Begin);
            outstream.CopyTo(outfile);

            return OutputFile;
        }
        
        /// <summary>
        /// Fixes 3D printing file stream.
        /// </summary>
        /// <param name="inputStream">The input file to be fixed as a Stream object.</param>
        /// <returns>The fixed file as a Stream.</returns>
        public async Task<Stream> FixAsync(Stream inputStream)
        {
            InputStream = inputStream;

            // 1. LoadModelFromPackageAsync accepts IRandomAccessStream and uses stream cloning internally
            // 2. WindowsRuntimeStreamExtensions.AsRandomStream converts Stream to IRandomAccessStream, but the resulting stream doesn't support cloning
            // 3. InMemoryRandomAccessStream does support cloning. So we needed a way to go from Stream to InMemoryRandomAccessStream
            // 4. Solution: Copy Stream to MemoryStream. Write to InMemoryRandomAccessStream via WriteAsync, which accepts IBuffer
            //    To get IBuffer, we first need to get the memoryStream Bytes with ToArray() and then get IBuffer using the
            //    System.Runtime.InteropServices.WindowsRuntime AsBuffer90 extension method.  
            //    We then pass the InMemoryRandomAccessStream object to  LoadModelFromPackageAsync.

            using var memoryStream = new MemoryStream();
            await InputStream.CopyToAsync(memoryStream);

            using InMemoryRandomAccessStream memoryRandomAccessStream = new InMemoryRandomAccessStream();
            await memoryRandomAccessStream.WriteAsync(memoryStream.ToArray().AsBuffer());

            var package = new Printing3D3MFPackage();
            var model = await package.LoadModelFromPackageAsync(memoryRandomAccessStream);

            await model.RepairAsync();
            await package.SaveModelToPackageAsync(model);

            return WindowsRuntimeStreamExtensions.AsStream(await package.SaveAsync());
        }

        private string SetOutputFile(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (string.IsNullOrEmpty(InputFile))
                {
                    throw new Exception("InputFile must be set before OutputFile.");
                }
                else
                {
                    return string.Concat(Path.GetDirectoryName(InputFile), "\\",
                                            Path.GetFileNameWithoutExtension(InputFile),
                                            "_fixed",
                                            Path.GetExtension(InputFile));
                }
            }
            else
            {
                return GetAbsolutePath(value);
            }
        }

        private static String GetAbsolutePath(String path)
        {
            return GetAbsolutePath(null, path);
        }
        
        private static String GetAbsolutePath(String basePath, String path)
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
