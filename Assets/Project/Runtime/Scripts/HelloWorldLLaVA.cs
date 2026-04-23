using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Project.Runtime.Scripts.AI
{
    [Serializable]
    public class HelloResponse
    {
        public string response;
    }

    public class HelloWorldLLaVA : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string _endpoint = "http://127.0.0.1:8000/hello-test";

        private void Start()
        {
            StartCoroutine(TestConnectionAsync());
        }

        private IEnumerator TestConnectionAsync()
        {
            using (var request = UnityWebRequest.Get(_endpoint))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(request.error);
                    yield break;
                }

                var response = JsonUtility.FromJson<HelloResponse>(request.downloadHandler.text);
                Debug.Log($"<color=green>{response.response}</color>");
            }
        }
    }
}