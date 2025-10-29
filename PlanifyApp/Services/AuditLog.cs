using System;
using System.IO;

namespace Planify.Services
{
    public sealed class AuditLog
    {
        readonly string _path;
        public AuditLog()
        {
            var root = Path.Combine(FileSystem.AppDataDirectory, "Planify");
            Directory.CreateDirectory(root);
            _path = Path.Combine(root, "audit.log");
        }

        public void Write(string user, string action, string details)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{user}\t{action}\t{details}";
            File.AppendAllLines(_path, new[] { line });
        }
    }
}
