using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UniVRM10;

public class Test : MonoBehaviour
{
    public string vrmFileName = "dokudami.vrm";

    private async void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, vrmFileName);

#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.LogError("Androidでは LoadPathAsync は直接使えません（StreamingAssetsはパス解決できません）");
        return;
#endif

        Debug.Log(path);

        var instance = await Vrm10.LoadPathAsync(
            path
        );

        if (instance != null)
        {
            Debug.Log("VRMロード成功！");
        }
        else
        {
            Debug.LogError("VRMロード失敗");
        }
    }
}
