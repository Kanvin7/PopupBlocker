using System.IO;
using System.Text.Json;

namespace PopupBlocker.Utility.Commons
{
    public static class FileOperation
    {
        public static JsonSerializerOptions DefaultJsonSerializerOptions => new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static T ReadJsonFromFile<T>(string path, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"文件不存在: {path}");

            // 允许别人同时读，避免因为临时占用就把整份配置读失败
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return JsonSerializer.Deserialize<T>(stream, jsonSerializerOptions ?? DefaultJsonSerializerOptions) ?? throw new NullReferenceException($"文件内容无效: {path}");
        }

        public static void WriteJsonToFile<T>(string path, T data, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            stream.SetLength(0);
            JsonSerializer.Serialize(stream, data, jsonSerializerOptions ?? DefaultJsonSerializerOptions);
        }
    }
}
