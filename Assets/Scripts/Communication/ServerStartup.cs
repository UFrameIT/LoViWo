using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ServerStartup : MonoBehaviour
{
    public static Process process;
    public static ProcessStartInfo processInfo;

    void Start()
    {
        GameState.serverRunning = false;
        StartCoroutine(ServerRoutine());
    }

    IEnumerator ServerRoutine()
    {
        UnityWebRequest request = UnityWebRequest.Get(GameSettings.ServerAdress + "/scroll/list");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            UnityEngine.Debug.Log("no running server " + request.error);


#if !UNITY_WEBGL
            if (File.Exists(GameSettings.frameITJarPath) && Directory.Exists(GameSettings.archivesPath))
            {
                processInfo = new ProcessStartInfo();
                processInfo.FileName = "java";
                processInfo.Arguments = @"-jar " + GameSettings.frameITJarPath + " -bind :8085 -archive-root " + GameSettings.archivesPath;
                //set "UseShellExecute = true" AND "CreateNoWindow = false" to see the mmt-server output
                processInfo.UseShellExecute = false;
                processInfo.CreateNoWindow = true;

                process = Process.Start(processInfo);
                yield return null;
            }
            else {
                UnityEngine.Debug.Log("ServerStartup: Either file with path \"" + GameSettings.frameITJarPath + "\" OR directory \"" + GameSettings.archivesPath + "\" does not exist.");
            }
#endif
            while (true)
            {
                request = UnityWebRequest.Get(GameSettings.ServerAdress + "/scroll/list");
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    // UnityEngine.Debug.Log("ServerStartup: No running server.");
                }
                else
                {
                    break;
                }



                yield return null;
            }
        }

        GameState.serverRunning = true;
        UnityEngine.Debug.Log("ServerStartup: finished.");
        yield return null;
    }
}
