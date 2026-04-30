using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Project.Runtime.Scripts
{
    public class BackendManager : MonoBehaviour
    {
        [Header("Backend Configuration")]
        [SerializeField] private string _backendFolderName = "main";
        [SerializeField] private string _executableName = "main.exe";

        private Process _backendProcess;

        private void Awake()
        {
            StartBackendProcess();
        }

        private void StartBackendProcess()
        {
            var folderPath = Path.Combine(Application.streamingAssetsPath, _backendFolderName);
            var exePath = Path.Combine(folderPath, _executableName);

            if (!File.Exists(exePath)) return;

            _backendProcess = new Process();
            _backendProcess.StartInfo.FileName = exePath;
            _backendProcess.StartInfo.UseShellExecute = false;
            _backendProcess.StartInfo.CreateNoWindow = true;
            
            _backendProcess.Start();
        }

        private void OnApplicationQuit()
        {
            if (_backendProcess == null) return;
            if (_backendProcess.HasExited) return;

            _backendProcess.Kill();
            _backendProcess.Dispose();
        }
    }
}