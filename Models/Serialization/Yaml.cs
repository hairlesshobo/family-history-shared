using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FoxHollow.FHM.Shared.Utilities.Serialization
{
    public static class Yaml
    {
        private static ISerializer BuildSerializer()
        {
            return new SerializerBuilder()
                        .WithQuotingNecessaryStrings()
                        .WithNamingConvention(UnderscoredNamingConvention.Instance)
                        .Build();
        }

        /// <summary>
        ///     Dump the provided .NET object model to a yaml-formatted string
        /// </summary>
        /// <param name="model">Object to be dumped</param>
        /// <returns>Text string that contains the entire generated yaml document</returns>
        public static string DumpToString(object model)
        {
            var serializer = BuildSerializer();

            return serializer.Serialize(model);
        }

        /// <summary>
        ///     Convert the provided .NET object model to yaml and write it to the provided stream
        /// </summary>
        /// <param name="model">Object to be dumped</param>
        /// <param name="stream">Stream to write the yaml document to</param>
        public static void DumpToStream(object model, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                var serializer = BuildSerializer();
                serializer.Serialize(writer, model);
            }
        }

        /// <summary>
        ///     Convert the provided .NET object model to yaml and write it to a file
        ///     located at the provided path, optionally overwriting the existing file
        /// </summary>
        /// <param name="model">Object to be dumped</param>
        /// <param name="filePath">Full path to the yaml document that is to be written</param>
        /// <param name="overwrite">If true, it will overwrite a file that already exists with the same name, default is false</param>
        public static void DumpToFile(object model, string filePath, bool overwrite = false)
        {
            if (File.Exists(filePath) && !overwrite)
                throw new Exception($"Destination path already exists: {filePath}");

            using (var fileHandle = File.OpenWrite(filePath))
                Yaml.DumpToStream(model, fileHandle);
        }


        private static IDeserializer BuildDeserializer()
            => new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();


        /// <summary>
        ///     Deserialize the YAML document provided as a string and convert
        ///     it to a native .NET object of type TResult
        /// </summary>
        /// <param name="text">YAML text document to deserialize</param>
        /// <typeparam name="TResult">Type that the document is to be loaded into</typeparam>
        /// <returns>.NET Object represented by the provided YAML document</returns>
        public static TResult LoadFromString<TResult>(string text)
        {
            var deserializer = BuildDeserializer();

            return deserializer.Deserialize<TResult>(text);
        }

        /// <summary>
        ///     Deserialize the YAML document provided as a stream and convert
        ///     it to a native .NET object of type TResult
        /// </summary>
        /// <param name="stream">YAML text document to deserialize as a stream</param>
        /// <typeparam name="TResult">Type that the document is to be loaded into</typeparam>
        /// <returns>.NET Object represented by the provided YAML document</returns>
        public static TResult LoadFromStream<TResult>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var deserializer = BuildDeserializer();

                return deserializer.Deserialize<TResult>(reader);
            }
        }
        
        /// <summary>
        ///     Read the file located at the provided filePath and deserialize the YAML
        ///     document into a native .NET object of type TResult
        /// </summary>
        /// <param name="filePath">Full path to the yaml file that is to be read</param>
        /// <typeparam name="TResult">Type that the document is to be loaded into</typeparam>
        /// <returns>.NET Object represented by the provided YAML document</returns>
        public static TResult LoadFromFile<TResult>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Provided file path does not exist: {filePath}");

            using (var fileHandle = File.OpenRead(filePath))
            {
                return Yaml.LoadFromStream<TResult>(fileHandle);
            }
        }
    }
}