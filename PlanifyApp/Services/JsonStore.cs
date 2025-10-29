using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Planify.Services
{
    public sealed class JsonStore
    {
        readonly string _root;

        public JsonStore(string? root = null)
        {
            _root = root ?? System.IO.Path.Combine(FileSystem.AppDataDirectory, "Planify");
            Directory.CreateDirectory(_root);
        }

        public async Task SaveAsync<T>(string name, T data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(Path.Combine(_root, $"{name}.json"), json);
        }

        public async Task<T?> LoadAsync<T>(string name)
        {
            var path = Path.Combine(_root, $"{name}.json");
            if (!File.Exists(path)) return default;
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
