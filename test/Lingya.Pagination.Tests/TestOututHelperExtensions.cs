using Newtonsoft.Json;
namespace Xunit.Abstractions {
    public static class TestOututHelperExtensions {
        internal static JsonSerializerSettings JsonSettings = new () {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static void WriteJson (this ITestOutputHelper output, object obj) {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject (obj, Newtonsoft.Json.Formatting.Indented, JsonSettings);
            output.WriteLine (json);
        }

    }
}