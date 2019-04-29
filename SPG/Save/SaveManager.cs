using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using System.IO;
using SPG;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
using System.Xml;
using System.Diagnostics;

namespace SPG.Save
{    
    public interface ISaveGame
    {
        string GetName();
    }

    public static class BinarySerializer
    {
        public static byte[] Serialize<T>(T obj)
        {
            var serializer = new DataContractSerializer(typeof(T));
            var stream = new MemoryStream();
            using (var writer =
                XmlDictionaryWriter.CreateBinaryWriter(stream))
            {
                serializer.WriteObject(writer, obj);
            }
            return stream.ToArray();
        }

        public static T Deserialize<T>(byte[] data)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using (var stream = new MemoryStream(data))
            using (var reader =
                XmlDictionaryReader.CreateBinaryReader(
                    stream, XmlDictionaryReaderQuotas.Max))
            {
                return (T)serializer.ReadObject(reader);
            }
        }
    }

    public static class SaveManager
    {
        private static string GetPath(string fileName)
        {
            var assembly = Assembly.GetEntryAssembly();
            var gameName = assembly.GetName().Name;

            return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{gameName}\\Save\\{fileName}";
        }

        /// <summary>
        /// Loads a savegame. SaveGame must be initialized at that point.
        /// Returns success. (False if no savegame exists)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="saveGame"></param>
        /// <returns></returns>
        public static bool Load<T>(ref T saveGame) where T : ISaveGame
        {
            if (saveGame == null)
            {
                Debug.WriteLine($"Savegame instance must be initialized first.");
                return false;
            }

            var fileName = saveGame.GetName();
            var filePath = GetPath(fileName);

            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"File '{fileName}' doesn't exist.");
                return false;
            }

            bool success = true;
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                try
                {
                    using (var ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);

                        byte[] data = ms.ToArray();

                        saveGame = BinarySerializer.Deserialize<T>(data);
                    }                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to load game. Reason: " + e.Message);
                    success = false;
                }
                finally
                {
                    fs.Close();
                }
            }

            return success;
        }

        public static void Delete<T>(this T saveGame) where T : ISaveGame
        {
            var fileName = saveGame.GetName();
            var filePath = GetPath(fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// Saves all variables that are stored in the save game object. 
        /// IMPORTANT: Object/Class must have Serializable Attribute, or else saving/loading is not possible!
        /// </summary>
        public static void Save<T>(this T saveGame) where T : ISaveGame
        {
            var fileName = saveGame.GetName();
            var filePath = GetPath(fileName);

            bool success = File.Exists(filePath);
            if (success) { }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            success = true;
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                try
                {
                    byte[] data = BinarySerializer.Serialize<T>(saveGame);
                    fs.Write(data, 0, data.Length);
                }
                catch (Exception e)
                {
                    success = false;
                    Console.WriteLine("Failed to save game. Reason: " + e.Message);                    
                }
                finally
                {
                    fs.Close();
                }
            }
        }
    }
}
