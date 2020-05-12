using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
public class BotConfig {
    public string Token;

    public void Save (string path) {
        using (var sw = new StreamWriter (path)) {
            sw.Write (JsonConvert.SerializeObject (this));
        }
    }

    public static BotConfig FromFile (string path) {
        using (var sw = new StreamReader (path)) {
            var json = sw.ReadToEnd();
            return JsonConvert.DeserializeObject<BotConfig>(json);
        }
    }
}