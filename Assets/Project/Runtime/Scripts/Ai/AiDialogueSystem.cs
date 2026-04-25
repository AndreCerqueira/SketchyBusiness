using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Project.Runtime.Scripts.AI
{
    [Serializable]
    public class DialogueResponse
    {
        public string Text;
    }

    public class AiDialogueSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string _endpoint = "http://127.0.0.1:8000/generate-intro";

        public void RequestIntroDialogue(Action<string> onCompleted)
        {
            StartCoroutine(FetchIntroDialogueAsync(onCompleted));
        }

        public void CancelAllRequests()
        {
            StopAllCoroutines();
        }

        private IEnumerator FetchIntroDialogueAsync(Action<string> onCompleted)
        {
            using (var request = UnityWebRequest.Get(_endpoint))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onCompleted?.Invoke(string.Empty);
                    yield break;
                }

                var response = JsonUtility.FromJson<DialogueResponse>(request.downloadHandler.text);
                onCompleted?.Invoke(response.Text);
            }
        }
    }
}