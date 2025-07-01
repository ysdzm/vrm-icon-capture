using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UniVRM10;

public class VrmBatchLoader : MonoBehaviour
{
    public string folderName = "sample_models";
    public int vrmCount = 5;

    private async void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.LogError("Androidでは LoadPathAsync は直接使えません（StreamingAssetsはパス解決できません）");
        return;
#endif

        for (int i = 1; i <= vrmCount; i++)
        {
            string fileName = $"sample_{i}.vrm";
            string path = Path.Combine(Application.streamingAssetsPath, folderName, fileName);
            Debug.Log($"Loading VRM: {path}");

            var instance = await Vrm10.LoadPathAsync(path);

            if (instance != null)
            {
                Debug.Log($"VRM {fileName} ロード成功");

                // 何か表示確認のために1フレーム待機
                await Task.Delay(1000);

                // アンロード
                Destroy(instance.gameObject);
                Debug.Log($"VRM {fileName} アンロード完了");
            }
            else
            {
                Debug.LogError($"VRM {fileName} ロード失敗");
            }

            // 少し待ってから次へ（例: 0.5秒）
            await Task.Delay(500);
        }

        Debug.Log("すべてのVRM処理が完了しました。");
    }
}
