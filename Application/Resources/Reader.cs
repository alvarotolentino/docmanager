using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Utf8Json;

namespace Application.Resources
{
    public class Reader
    {
        public static dynamic GetMessages() {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream("Application.Resources.messages.json");
            StreamReader reader = new StreamReader(resourceStream);
            var content = reader.ReadToEnd();
            var json = JsonSerializer.Deserialize<dynamic>(content);
            return json;
        }
    }
}