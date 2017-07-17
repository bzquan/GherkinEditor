// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphGeneration.cs" company="Jamie Dixon Ltd">
//   Jamie Dixon
// </copyright>
// <summary>
//   Defines the GraphGeneration type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GraphVizWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Reflection;

    using Commands;
    using Queries;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// The main entry class for the wrapper.
    /// </summary>
    public class GraphGeneration
    {
        private const string ProcessFolder = "GraphViz";
        private const string ConfigFile = "config6";

        private readonly IGetStartProcessQuery startProcessQuery;
        private readonly IGetProcessStartInfoQuery getProcessStartInfoQuery;
        private readonly IRegisterLayoutPluginCommand registerLayoutPlugincommand;
        private String graphvizPath = null;

        public GraphGeneration(IGetStartProcessQuery startProcessQuery,
                               IGetProcessStartInfoQuery getProcessStartInfoQuery,
                               IRegisterLayoutPluginCommand registerLayoutPlugincommand)
        {
            this.startProcessQuery = startProcessQuery;
            this.getProcessStartInfoQuery = getProcessStartInfoQuery;
            this.registerLayoutPlugincommand = registerLayoutPlugincommand;

            this.graphvizPath = ConfigurationManager.AppSettings["graphVizLocation"];
        }

        #region Properties

        public String GraphvizPath
        {
            get { return graphvizPath ?? Path.Combine(AssemblyDirectory, ProcessFolder); }
            set
            {
                if (value != null && value.Trim().Length > 0)
                {
                    graphvizPath = value.Replace("\\", "/");
                }
                else
                {
                    graphvizPath = null;
                }
            }
        }
        
        public Enums.RenderingEngine RenderingEngine { get; set; } = Enums.RenderingEngine.Dot;

        public bool IsGraphvizInstalled => File.Exists(FilePath);
        private string ConfigLocation => Path.Combine(GraphvizPath, ConfigFile);
        private bool ConfigExists => File.Exists(ConfigLocation);
        
        private static string AssemblyDirectory
        {
            get
            {
                var uriBuilder = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
                return Uri.UnescapeDataString(uriBuilder.Path);
            }
        }

        private string FilePath
        {
            get
            {
                string fileName = this.GetRenderingEngine(RenderingEngine) + ".exe";
                return Path.Combine(GraphvizPath, fileName);
            }
        }

        #endregion

        /// <summary>
        /// Generates a graph based on the dot file passed in.
        /// </summary>
        /// <param name="dotGraph"> A string representation of a dot file.</param>
        /// <param name="returnType"> The type of file to be returned. </param>
        /// <returns> a byte array. </returns>
        public byte[] GenerateGraphDirectly(string dotGraph, Enums.GraphReturnType returnType)
        {
            if (!ConfigExists)
            {
                this.registerLayoutPlugincommand.Invoke(FilePath, this.RenderingEngine);
            }

            string fileType = this.GetReturnType(returnType);
            var processStartInfo = this.GetProcessStartInfo(fileType);
            return GenerateGraphBinary(dotGraph, processStartInfo);
        }

        /// <summary>
        ///  Generates a graph based on the dot file passed in.
        ///  It will generate temporary dot file before generating output.
        /// </summary>
        /// <param name="dotGraph">dot graph text</param>
        /// <param name="returnType"> The type of file to be returned. </param>
        /// <returns> a byte array. </returns>
        public byte[] GenerateGraphViaFile(string dotGraph, Enums.GraphReturnType returnType)
        {
            if (!ConfigExists)
            {
                this.registerLayoutPlugincommand.Invoke(FilePath, this.RenderingEngine);
            }

            string inputFile = WriteFileOfUTF8WithoutBOM(dotGraph);
            string fileType = this.GetReturnType(returnType);
            var processStartInfo = this.GetProcessStartInfo(fileType, inputFile);
            var binary = GenerateGraphBinary(null, processStartInfo);
            File.Delete(inputFile);

            return binary;
        }

        #region Private Methods

        private byte[] GenerateGraphBinary(string dotGraph, ProcessStartInfo processStartInfo)
        {
            byte[] output;
            using (var process = this.startProcessQuery.Invoke(processStartInfo))
            {
                if (!string.IsNullOrEmpty(dotGraph))
                {
                    InputDotViaStdIn(dotGraph, process);
                }
                using (var stdOut = process.StandardOutput)
                {
                    var baseStream = stdOut.BaseStream;
                    output = ReadFully(baseStream);
                }
            }

            return output;
        }

        private static void InputDotViaStdIn(string dotGraph, Process process)
        {
            process.BeginErrorReadLine();
            using (var stdIn = process.StandardInput)
            {
                stdIn.WriteLine(dotGraph);
            }
        }

        private static string WriteFileOfUTF8WithoutBOM(string dotFile)
        {
            string inputFile = Path.GetTempPath() + Guid.NewGuid().ToString() + ".dot";
            System.Text.Encoding utf8WithoutBom = new System.Text.UTF8Encoding(false); // without BOM
            using (StreamWriter sw = new StreamWriter(File.Open(inputFile, FileMode.Create), utf8WithoutBom))
            {
                sw.Write(dotFile);
            }

            return inputFile;
        }

        private System.Diagnostics.ProcessStartInfo GetProcessStartInfo(string returnType, string inputFile = null)
        {
            return this.getProcessStartInfoQuery.Invoke(new ProcessStartInfoWrapper
            {
                FileName = this.FilePath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,

                Arguments = ProcessArguments(returnType, inputFile),
                CreateNoWindow = true
            });
        }

        private string ProcessArguments(string returnType, string inputFile)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("-T")
              .Append(returnType)
              .Append(" ")
              .Append(inputFile);

            return sb.ToString();
        }

        private byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private string GetReturnType(Enums.GraphReturnType returnType)
        {
            var nameValues = new Dictionary<Enums.GraphReturnType, string>
                                 {
                                     { Enums.GraphReturnType.Png, "png" },
                                     { Enums.GraphReturnType.Jpg, "jpg" },
                                     { Enums.GraphReturnType.Pdf, "pdf" },
                                     { Enums.GraphReturnType.Plain, "plain" },
                                     { Enums.GraphReturnType.PlainExt, "plain-ext" },
                                     { Enums.GraphReturnType.Svg, "svg" }

                                 };
            return nameValues[returnType];
        }

        private string GetRenderingEngine(Enums.RenderingEngine renderingType)
        {
            var nameValues = new Dictionary<Enums.RenderingEngine, string>
                                 {
                                     { Enums.RenderingEngine.Dot, "dot" },
                                     { Enums.RenderingEngine.Neato, "neato" },
                                     { Enums.RenderingEngine.Twopi, "twopi" },
                                     { Enums.RenderingEngine.Circo, "circo" },
                                     { Enums.RenderingEngine.Fdp, "fdp" },
                                     { Enums.RenderingEngine.Sfdp, "sfdp" },
                                     { Enums.RenderingEngine.Patchwork, "patchwork" },
                                     { Enums.RenderingEngine.Osage, "osage" }
                                 };
            return nameValues[renderingType];
        }

        #endregion
    }
}
