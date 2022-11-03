using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Newtonsoft.Json;


#if UNITY_WEBGL
public class DarkblockExample : MonoBehaviour
{   
    
    [SerializeField] 
    string tokenId = "token_id";
    [SerializeField] 
    string contractAddress = "contract_address";
    [SerializeField] 
    string chain = "Solana";
    public string responseString = "";
    public string[] artId;
    public string[] owner;
    public string proxyUri;
    public string sessionToken;
    public int step = 0;
    public int epoch;
    

    ///////////////////////////////////////////
    // classes for the response from the API //
    [System.Serializable]
    public class Darkblock
    {
        [JsonProperty("status", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("darkblock", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DarkblockClass DarkblockDarkblock { get; set; }

        [JsonProperty("dbstack", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<DarkblockClass> Dbstack { get; set; }
    }
    [System.Serializable]
    public class DarkblockClass
    {
        [JsonProperty("id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("tags", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<Tag> Tags { get; set; }

        [JsonProperty("data", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Data Data { get; set; }

        [JsonProperty("block", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Block Block { get; set; }

        [JsonProperty("owner", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Owner Owner { get; set; }

        [JsonProperty("signature", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Signature { get; set; }
    }
    [System.Serializable]
    public class Block
    {
        [JsonProperty("height", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("timestamp", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? Timestamp { get; set; }

        [JsonProperty("id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
    }
    [System.Serializable]
    public class Data
    {
        [JsonProperty("size", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("type", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }
    [System.Serializable]
    public class Owner
    {
        [JsonProperty("address", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }
    }
    [System.Serializable]
    public class Tag
    {
        [JsonProperty("name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("value", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    public Darkblock res = new Darkblock();

    ////////////////////////////////
    // run this function to start //
    public void onClick () {
        OnSignMessage();
    }

    //////////////////////////////////////////////////////////////////////
    // Ienumerator to send a get request to darkblock api info endpoint //
     IEnumerator GetArtId(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
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
                    responseString = webRequest.downloadHandler.text;
                    Darkblock res = JsonConvert.DeserializeObject<Darkblock>(responseString);
                    Debug.Log(res.Status);
                     //Debug.Log(res.Dbstack[0].Tags[0].Value);
                    artId = new string[res.Dbstack.Count];
                    owner = new string[res.Dbstack.Count];
                    for(int i = 0; i < res.Dbstack.Count; i++)
                        {
                        artId[i] = res.Dbstack[i].Tags[5].Value;
                        owner[i] = res.Dbstack[i].Owner.Address;
                        }
                    break;       
            }

        }
        step++;

        //////////////////////////////////////////
        // get all darkblocks from the token id //
        for (int i = 0; i < artId.Length; i++)
        {
            Debug.Log("darkblock " + i+1 + " of " + artId.Length);
           
        
            UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle($"https://gateway.darkblock.io/proxy?artid={artId[i]}&session_token={epoch}_{sessionToken}&token_id={tokenId}&contract={contractAddress}&platform={chain}&owner={owner[i]}" );
        
            // Request and wait for the desired page.
            yield return www.SendWebRequest();
            


            if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
        }
            }
       
    yield break;
    }

////////////////////////////////////////////////////////////////
// sign the message to generate the session token for the api //
async public void OnSignMessage()
    {   
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        epoch = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;

        try {
            string message = epoch + "_rest_of_your_message";
            string response = await Web3GL.Sign(message);
            print(response);
            sessionToken = response;

        } catch (Exception e) {
            Debug.LogException(e, this);
        }
        step++;
        StartCoroutine(GetArtId($"https://api.darkblock.io/v1/darkblock/info?nft_id={contractAddress}:{tokenId}&nft_platform={chain}"));
    }
}
#endif




