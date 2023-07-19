using JsonRpc.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DemoLanguageServer.Contracts
{
    /// <summary>
    /// A <see cref="IJsonValueConverter"/> implementation that is supposed to be used with
    /// LSP over JSON-RPC.
    /// </summary>
    public class LanguageServiceParameterValueConverter : JsonValueConverter
    {

        private static readonly JsonSerializer serializer = new JsonSerializer
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            // That is, by default `null` in the received JSON will be treated as if it does not exist.
            NullValueHandling = NullValueHandling.Ignore
        };

        public LanguageServiceParameterValueConverter() : base(serializer)
        {
        }

    }
}
