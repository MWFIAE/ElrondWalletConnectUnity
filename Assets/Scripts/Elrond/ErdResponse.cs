using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Elrond
{
    public class ErdResponse : JsonRpcResponse
    {
        [JsonProperty]
        private string result;

        [JsonIgnore]
        public string Result => result;
    }
}