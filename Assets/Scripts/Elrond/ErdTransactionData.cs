namespace WalletConnectSharp.Core.Models.Elrond
{
    public class ErdTransactionData
    {
        public int nonce;
        public string from;
        public string to;
        public string amount;
        public string gasPrice;
        public string gasLimit;
        public string data;
        public string chainID;
        public int version;
    }
}