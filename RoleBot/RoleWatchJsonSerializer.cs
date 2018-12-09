using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoleBot
{
    class RoleWatchJsonSerializer : JsonConverter
    {
        private readonly Type[] _types;

        public RoleWatchJsonSerializer (params Type[] types)
        {
            _types = types;
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType) => _types.Any(t => t == objectType);
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var item = JObject.Load(reader);
            var guild = item["Guild"].Value<string>();
            var channel = item["Channel"].Value<string>();
            var message = item["Message"].Value<string>();
            var emoji = item["Emoji"].Value<string>();
            var role = item["Role"].Value<string>();

            var rw = new RoleWatch(guild, channel, message, emoji, role);

            return rw;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = new JObject();
            var rw = (RoleWatch)value;

            o.Add("Guild", rw.Guild.Id);
            o.Add("Channel", rw.Channel.Id);
            o.Add("Message", rw.Message.Id);
            o.Add("Emoji", rw.Emoji.Name);
            o.Add("Role", rw.Role.Id);

            o.WriteTo(writer);
        }
    }
}
