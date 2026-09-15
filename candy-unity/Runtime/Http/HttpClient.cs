using System;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace MoshEngine.Candy.Unity
{
    public class HttpClient : MonoBehaviour
    {
        private JsonSerializerSettings _jsonSerializerSettings;

        private void Awake()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
                // TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public IEnumerator Get<TResponse>(string url, Action<TResponse> onSuccess, Action<string> onFailure = null)
            where TResponse : class
        {
            using var webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();

            if (
                webRequest.result
                is UnityWebRequest.Result.ConnectionError
                    or UnityWebRequest.Result.ProtocolError
                    or UnityWebRequest.Result.DataProcessingError
            )
            {
                onFailure?.Invoke(webRequest.error);
            }
            else
            {
                var responseJson = webRequest.downloadHandler.text;
                var response = JsonConvert.DeserializeObject<TResponse>(responseJson, _jsonSerializerSettings);
                onSuccess?.Invoke(response);
            }
        }

        public IEnumerator Post<TRequest, TResponse>(
            string url,
            TRequest requestBody,
            Action<TResponse> onSuccess,
            Action<string> onFailure = null
        )
            where TRequest : class
            where TResponse : class
        {
            var jsonBody = JsonConvert.SerializeObject(requestBody, _jsonSerializerSettings);

            using var webRequest = new UnityWebRequest(url, "POST");
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (
                webRequest.result
                is UnityWebRequest.Result.ConnectionError
                    or UnityWebRequest.Result.ProtocolError
                    or UnityWebRequest.Result.DataProcessingError
            )
            {
                onFailure?.Invoke(webRequest.error);
            }
            else
            {
                var response = JsonConvert.DeserializeObject<TResponse>(
                    webRequest.downloadHandler.text,
                    _jsonSerializerSettings
                );
                onSuccess?.Invoke(response);
            }
        }
    }
}
