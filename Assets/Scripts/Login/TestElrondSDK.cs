using Erdcsharp.Configuration;
using Erdcsharp.Domain;
using Erdcsharp.Domain.SmartContracts;
using Erdcsharp.Domain.Values;
using Erdcsharp.Provider;
using Erdcsharp.Provider.Dtos;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using Network = Erdcsharp.Configuration.Network;

public class TestElrondSDK : MonoBehaviour
{
    ElrondProvider provider;
    AccountDto account;
    Account userAccount;
    NetworkConfig networkConfig;
    // Start is called before the first frame update
    async void Start()
    {
        provider = new ElrondProvider(new HttpClient(), new ElrondNetworkConfiguration(Network.DevNet));
        networkConfig = await NetworkConfig.GetFromNetwork(provider);
        account = await provider.GetAccount("erd1lgp3ezf2wfkejnu0sm5y9g4x3ad05gr8lfc0g69vvdwwj0wjv0gscv2w4s");
        GetAccountBalance();

        var outputType = TypeValue.BigUintTypeValue;
        Address smartContractAddress = Address.FromBech32("erd1qqqqqqqqqqqqqpgqvmy8t2e7g5dh28nmucyqnsl2r2wq9c6l0eqq25evuf");
        Address caller = null;
        var args = Address.FromBech32("erd1jza9qqw0l24svfmm2u8wj24gdf84hksd5xrctk0s0a36leyqptgs5whlhf");
        var queryResult = await SmartContract.QuerySmartContract<NumericValue>(
                                                                                provider,
                                                                                smartContractAddress,
                                                                                outputType,
                                                                                "getDeposit",
                                                                                caller,
                                                                                args);
        Debug.Log(queryResult.Number);
    }





    private void GetAccountBalance()
    {


        Debug.Log($"Balance : {account.Balance}");

        var amount = TokenAmount.From(account.Balance);
        Debug.Log($"Balance in EGLD : {amount.ToCurrencyString()}");
    }

    void MakeTransaction()
    {
        //var txRequest = TransactionRequest.Create(account, networkConfig);
        
        //var tx = await txRequest.Send(_provider, wallet);
        //await tx.AwaitExecuted(_provider);
    }
}
