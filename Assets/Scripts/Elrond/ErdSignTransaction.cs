using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Elrond
{
    public sealed class ErdSignTransaction : JsonRpcRequest
    {
        [JsonProperty("params")]
        private ErdTransactionData[] _parameters;

        [JsonIgnore]
        public ErdTransactionData[] Parameters => _parameters;

        public ErdSignTransaction(params ErdTransactionData[] transactionDatas) : base()
        {
            this.Method = "erd_batch_sign";
            this._parameters = transactionDatas;
        }
    }
}
