using Chaos.NaCl;
using Erdcsharp.Configuration;
using Erdcsharp.Domain;
using Erdcsharp.Provider;
using Erdcsharp.Provider.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Core.Network;
using WalletConnectSharp.Unity;

public class Login : WalletConnectActions
{
    public Text address;
    public Text status;
    public GameObject connect;
    public GameObject disconnect;
    public GameObject transaction;
    public GameObject qr;
    bool loginInProgress;

    ElrondProvider provider;
    AccountDto connectedAccount;
    Account userAccount;
    NetworkConfig networkConfig;
    // Start is called before the first frame update
    async void Start()
    {
        provider = new ElrondProvider(new HttpClient(), new ElrondNetworkConfiguration(Erdcsharp.Configuration.Network.DevNet));
        networkConfig = await NetworkConfig.GetFromNetwork(provider);
    }

    private void ActiveSessionOnDisconnect(object sender, EventArgs e)
    {
        once = false;
        WalletConnect.ActiveSession.OnSessionDisconnect -= ActiveSessionOnDisconnect;
        RefreshButtons();
    }

    void RefreshButtons()
    {
        if (WalletConnect.ActiveSession.Accounts == null)
        {
            if (loginInProgress == true)
            {
                connect.SetActive(false);
            }
            else
            {
                connect.SetActive(true);
            }
            disconnect.SetActive(false);
            transaction.SetActive(false);
            if (!loginInProgress)
            {
                qr.SetActive(false);
            }
            address.text = "-";
            status.text = "";
        }
        else
        {
            connect.SetActive(false);
            disconnect.SetActive(true);
            transaction.SetActive(true);
            qr.SetActive(false);
            loginInProgress = false;
        }
    }

    bool once;

    // Update is called once per frame
    void Update()
    {
        if (WalletConnect.ActiveSession == null)
            return;
        if (once == true)
            return;
        RefreshButtons();


        if (WalletConnect.ActiveSession.Accounts != null)
        {
            OnConnected();
            once = true;
        }
    }

    private void OnConnected()
    {
        WalletConnect.ActiveSession.OnSessionDisconnect += ActiveSessionOnDisconnect;
        RefreshAccount();
      

        //Debug.Log("NETWORK CONFIG");
        //Debug.Log("ChainId " + networkConfig.ChainId);
        //Debug.Log("GasPerDataByte " + networkConfig.GasPerDataByte);
        //Debug.Log("MinGasLimit " + networkConfig.MinGasLimit);
        //Debug.Log("MinGasPrice " + networkConfig.MinGasPrice);
        //Debug.Log("MinTransactionVersion " + networkConfig.MinTransactionVersion);
    }

    async void RefreshAccount()
    {
        connectedAccount = await provider.GetAccount(WalletConnect.ActiveSession.Accounts[0]);

        var amount = TokenAmount.From(connectedAccount.Balance);

        address.text = connectedAccount.Address + "\n EGLD: " + amount.ToDenominated();
        if (!string.IsNullOrEmpty(connectedAccount.Username))
        {
            address.text += "\nHT: " + connectedAccount.Username;
        }
    }


    public async void SendTransaction1()
    {
        status.text = "Sending Transaction";
        
        Debug.Log(address);
        var transaction = new TransactionData()
        {
            nonce = connectedAccount.Nonce,
            from = connectedAccount.Address,
            to = "erd1jza9qqw0l24svfmm2u8wj24gdf84hksd5xrctk0s0a36leyqptgs5whlhf",
            amount = "10000000000000000",
            data = "You see this?",
            gasPrice = networkConfig.MinGasPrice.ToString(),
            gasLimit = (networkConfig.MinGasLimit + 20000).ToString(),
            chainId = networkConfig.ChainId,
            version = networkConfig.MinTransactionVersion

        };

        var signature = await SignTransaction(transaction);

        if(signature.Contains("error"))
        {
            status.text = signature;
            return;
        }

        status.text = "Transaction Signed";
        

        Debug.Log("RESULT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! signature:" + signature);


        SignedTransactionData tx = new SignedTransactionData(transaction, signature);
        string json = JsonUtility.ToJson(tx);

        Debug.Log("SignedTransactionData ->" + json);

        StartCoroutine(PostTransaction("https://devnet-api.elrond.com/transactions", json));


        //TransactionRequestDto tx = new TransactionRequestDto
        //{
        //    Nonce = transaction.nonce,
        //    Value = transaction.amount,
        //    Receiver = new AccountDto(transaction.to),
        //    Sender = connectedAccount.Address,
        //    GasPrice = int.Parse(transaction.gasPrice),
        //    GasLimit = int.Parse(transaction.gasLimit),
        //    Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(transaction.data)),
        //    ChainID = transaction.chainId,
        //    Version = transaction.version,
        //    Signature = signature
        //};
        //var txResult = await provider.SendTransaction(tx);
        //Debug.Log(txResult);
    }

    private void aaa(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" + e.Message + " " + e.Source);
    }

    IEnumerator GetTransactionStatusRequest(string uri, UnityAction<UnityWebRequest.Result, string> CompleteMethod)
    {
        Debug.Log(uri);

        yield return new WaitForSeconds(1);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();


            string result = null;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("Received: " + webRequest.downloadHandler.text);
                    result = webRequest.downloadHandler.text;
                    break;
            }

            CompleteMethod(webRequest.result, result);
        }
    }


    public class TransactionStatus
    {
        public string status { get; set; }
    }


    IEnumerator PostTransaction(string uri, string signedData)
    {

        using var webRequest = new UnityWebRequest();
        webRequest.url = uri; // PostUri is a string containing the url
        webRequest.method = "POST";
        webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(signedData)); // postData is Json file as a string
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("accept", "application/json");
        webRequest.SetRequestHeader("Content-Type", "application/json");





        //UnityWebRequest webRequest = UnityWebRequest.Post(uri, json);

        //webRequest.SetRequestHeader("Content-Type", "application/json");
        //// Request and wait for the desired page.
        yield return webRequest.SendWebRequest();

        string[] pages = uri.Split('/');
        int page = pages.Length - 1;

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error + " " + webRequest.result + " " + webRequest.downloadHandler.text);
                break;
            case UnityWebRequest.Result.Success:
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                string output = webRequest.downloadHandler.text;
                Debug.Log(output);

                BroadcastResponse response = JsonConvert.DeserializeObject<BroadcastResponse>(output);

                Debug.Log("RESPONSE " + response);

                Debug.Log(response.txHash + " " + response.sender);

                txHash = response.txHash;
                CheckStatus(response.txHash);

                break;

        }
    }

    string txHash;

    private void CheckStatus(string txHash)
    {
        StartCoroutine(GetTransactionStatusRequest("https://devnet-api.elrond.com/transactions/" + txHash + "?fields=status", Complete));

        //TransactionDto transactionDetail = await provider.GetTransactionDetail(txHash);
        //Debug.Log(transactionDetail.Status);
        //float time = 0;
        //while (transactionDetail.Status != "Success")
        //{
        //    time += Time.deltaTime;
        //    if (time > 2)
        //    {

        //        transactionDetail = await provider.GetTransactionDetail(txHash);
        //        Debug.Log("in while" + transactionDetail.Status);
        //        time = 0;
        //    }
        //}
    }

    private void Complete(UnityWebRequest.Result result, string message)
    {
        if (result == UnityWebRequest.Result.Success)
        {
            TransactionStatus status = JsonConvert.DeserializeObject<TransactionStatus>(message);

            this.status.text = status.status;

            if(status.status != "success")
            {
                StartCoroutine(GetTransactionStatusRequest("https://devnet-api.elrond.com/transactions/" + txHash + "?fields=status", Complete));
            }
            else
            {
                RefreshAccount();
            }    
        }
    }

    public class BroadcastResponse
    {
        public string txHash;
        public string receiver;
        public string sender;
        public int receiverShard;
        public int senderShard;
        public string status;
    }




    [System.Serializable]
    public class SCQuerry
    {
        //public string scAddress = "erd1qqqqqqqqqqqqqpgqvmy8t2e7g5dh28nmucyqnsl2r2wq9c6l0eqq25evuf";
        //public string funcName = "getTarget";
        //public string[] args;

        public string scAddress = "erd1qqqqqqqqqqqqqpgqvmy8t2e7g5dh28nmucyqnsl2r2wq9c6l0eqq25evuf";
        public string funcName = "getDeposit";
        public string[] args = { "90ba5001cffaab06277b570ee92aa86a4f5bda0da18785d9f07f63afe4800ad1" };
    }

    public void OnClickSendTransaction()
    {

        StartCoroutine(GetRequest("https://devnet-api.elrond.com/query"));
        //Debug.Log("Send");
        ////var address = WalletConnect.ActiveSession.Accounts[0];
        ////var transaction = new TransactionData()
        ////{
        ////    data = "0x",
        ////    from = address,
        ////    to = address,
        ////    gas = "21000",
        ////    value = "0",
        ////    chainId = 2,
        ////};

        ////var results = await SendTransaction(transaction);

        //var results = await Test();


        //Debug.Log(results);
    }

    IEnumerator GetRequest(string uri)
    {
        SCQuerry querry = new SCQuerry();
        string json = JsonUtility.ToJson(querry);
        Debug.Log(json);

        using var webRequest = new UnityWebRequest();
        webRequest.url = uri; // PostUri is a string containing the url
        webRequest.method = "POST";
        webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)); // postData is Json file as a string
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("accept", "*/*");
        webRequest.SetRequestHeader("Content-Type", "application/json");





        //UnityWebRequest webRequest = UnityWebRequest.Post(uri, json);

        //webRequest.SetRequestHeader("Content-Type", "application/json");
        //// Request and wait for the desired page.
        yield return webRequest.SendWebRequest();

        string[] pages = uri.Split('/');
        int page = pages.Length - 1;

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                string output = webRequest.downloadHandler.text;



                ContractResponse response = JsonUtility.FromJson<ContractResponse>(output);

                string encodedText = response.returnData[0];

                Debug.Log("base64 " + encodedText);

                byte[] decodedBytes = Convert.FromBase64String(encodedText);
                string hex = ByteArrayToHexString(decodedBytes);

                Debug.Log("hex " + hex);

                double number = Convert.ToInt64(hex, 16);

                Debug.Log("dec " + number);

                double anount = number / 1000000000000000000;

                Debug.Log("Final value " + anount.ToString("F2"));

                break;
        }



    }

    public static string ByteArrayToHexString(byte[] ba)
    {
        StringBuilder hex = new StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }


    public static byte[] HexStringToByteArray(string hex)
    {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }


    public class ContractResponse
    {
        public string[] returnData;
        public string returnCode;
        public long gasRemaining;
        public int gasRefund { get; set; }
        public Outputaccounts outputAccounts { get; set; }
    }

    public class Outputaccounts
    {
        public account someAccount { get; set; }
    }

    public class account
    {
        public string address { get; set; }
        public int nonce { get; set; }
        public int balanceDelta { get; set; }
        public Storageupdates storageUpdates { get; set; }
        public int callType { get; set; }
    }

    public class Storageupdates
    {
    }



    public async Task<string> Test()
    {
        int nr = 0;
        while (nr < 600)
        {
            nr++;
            await Task.Yield();
        }

        return nr.ToString();
    }

    public void MaiarLogin()
    {
        //WalletConnect.Instance.OpenDeepLink();
        loginInProgress = true;
        qr.SetActive(true);
        connect.SetActive(false);
    }

    public void Disconnect()
    {
        WalletConnect.Instance.CloseSession();
        //Debug.Break();
    }

    public void WebLogin()
    {

    }

    public void GetTotalAmount()
    {

    }


    private void OnDestroy()
    {
        WalletConnect.ActiveSession.OnSessionDisconnect -= ActiveSessionOnDisconnect;
    }
}
