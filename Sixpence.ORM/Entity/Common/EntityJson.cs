using System.Buffers.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Sixpence.ORM.Entity
{
    public static partial class EntityCommon
    {
        public static string ConvertToJson(BaseEntity entity)
        {
            return JsonSerializer.Serialize(entity, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = new UnderlineToPascal(),
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
        }

        public static T ConvertFromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = new PascalToUnderline(),
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
        }
    }

    public class UnderlineToPascal : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return EntityCommon.UnderlineToPascal(name);
        }
    }

    public class PascalToUnderline : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return EntityCommon.PascalToUnderline(name);
        }
    }
}
