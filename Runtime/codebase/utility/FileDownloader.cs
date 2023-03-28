using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Solana.Unity.Metaplex.NFT.Library;
using Solana.Unity.Wallet;
using UnityEngine;
using UnityEngine.Networking;

namespace Solana.Unity.SDK.Utility
{
    
    public class PublicKeyJsonConverter : JsonConverter<PublicKey>
    {
        public override void WriteJson(JsonWriter writer, PublicKey value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override PublicKey ReadJson(JsonReader reader, Type objectType, PublicKey existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var pk = serializer.Deserialize<string>(reader);
            if (pk == null) return null;
            return new PublicKey(pk);
        }
    }

    public class CreatorJsonConverter : JsonConverter<Creator>
    {
        public override void WriteJson(JsonWriter writer, Creator value, JsonSerializer serializer)
        {
            writer.WriteValue(value.key.ToString() + "-" + value.share + "-" + value.verified);
        }

        public override Creator ReadJson(JsonReader reader, Type objectType, Creator existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var parse = serializer.Deserialize<string>(reader)?.Split("-");
            if (parse is not { Length: 3 })
                return null;
            return new Creator(new PublicKey(parse[0]), (byte)int.Parse(parse[1]), bool.Parse(parse[2]));
        }
    }
    
    public class CollectionJsonConverter : JsonConverter<Collection>
    {
        public override void WriteJson(JsonWriter writer, Collection value, JsonSerializer serializer)
        {
            writer.WriteValue(value.key.ToString() + "-" + value.verified);
        }

        public override Collection ReadJson(JsonReader reader, Type objectType, Collection existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var parse = serializer.Deserialize<string>(reader)?.Split("-");
            if (parse is not { Length: 2 })
                return null;
            return new Collection(new PublicKey(parse[0]),  bool.Parse(parse[1]));
        }
    }

    public static class FileLoader
    {
        public static async Task<T> LoadFile<T>(string path, string optionalName = "")
        {

            if (typeof(T) == typeof(Texture2D))
            {
                return await LoadTexture<T>(path);
            }
            else
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return await LoadJsonWebRequest<T>(path);
#else
                return await LoadJson<T>(path);
#endif
            }
        }

        private static async Task<T> LoadTexture<T>(string filePath, CancellationToken token = default)
        {
            using var uwr = UnityWebRequest.Get(filePath);
            uwr.SendWebRequest();

            while (!uwr.isDone && !token.IsCancellationRequested)
            {
                await Task.Yield();
            }

            if (uwr.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(uwr.error);
                return default;
            }
            var data = uwr.downloadHandler.data;
            var tex = new Texture2D(2, 2);
            tex.LoadImage(data);
            return (T)Convert.ChangeType(tex, typeof(T));
        }

        private static async Task<T> LoadJsonWebRequest<T>(string path)
        {
            using var uwr = UnityWebRequest.Get(path);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SendWebRequest();

            while (!uwr.isDone)
            {
                await Task.Yield();
            }

            var json = uwr.downloadHandler.text;
            if (uwr.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(uwr.error);
                return default;
            }

            Debug.Log(json);
            try
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                return data;
            }
            catch
            {
                return default;
            }
        }

        private static async Task<T> LoadJson<T>(string path)
        {            
            var client = new HttpClient();
            
            try
            {
                var response = await client.GetAsync(path);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseBody);
                client.Dispose();
                return data;
            }
            catch
            {
                client.Dispose();
                return default;
            }
        }

        public static T LoadFileFromLocalPath<T>(string path)
        {
            if (!File.Exists(path))
                return default;

            var bytes = File.ReadAllBytes(path);

            var texture = new Texture2D(1, 1);
            if (typeof(T) == typeof(Texture2D))
            {
                texture.LoadImage(bytes);
                return (T)Convert.ChangeType(texture, typeof(T));
            }

            var contents = File.ReadAllText(path);
            try
            {
                var serializeOptions = new JsonSerializerSettings()
                {
                    Converters =
                    {
                        new PublicKeyJsonConverter(),
                        new CreatorJsonConverter(),
                        new CollectionJsonConverter()
                    }
                };
                var data = JsonConvert.DeserializeObject<T>(contents, serializeOptions);
                return data;
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
                return default;
            }
        }

        public static void SaveToPersistentDataPath<T>(string path, T data)
        {
            if (typeof(T) == typeof(Texture2D))
            {
                var dataToByte = ((Texture2D)Convert.ChangeType(data, typeof(Texture2D))).EncodeToPNG();
                File.WriteAllBytes(path, dataToByte);
            }
            else
            {
                var serializeOptions = new JsonSerializerSettings()
                {
                    Converters =
                    {
                        new PublicKeyJsonConverter(),
                        new CreatorJsonConverter(),
                        new CollectionJsonConverter()
                    }
                };
                var dataString = JsonConvert.SerializeObject(data, serializeOptions);
                File.WriteAllText(path, dataString);
            }
        }
        
        /// <summary>
        /// Resize great textures to small, because of performance
        /// </summary>
        /// <param name="texture2D"> Texture to resize</param>
        /// <param name="targetX"> Target width</param>
        /// <param name="targetY"> Target height</param>
        /// <returns></returns>
        public static Texture2D Resize(Texture texture2D, int targetX, int targetY)
        {
            RenderTexture rt = new RenderTexture(targetX, targetY, 24);
            RenderTexture.active = rt;
            Graphics.Blit(texture2D, rt);
            Texture2D result = new Texture2D(targetX, targetY);
            result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
            result.Apply();
            return result;
        }

    }

}
