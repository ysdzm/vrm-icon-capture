using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UniVRM10;

public class VrmBatchLoader : MonoBehaviour
{
    public string folderName = "sample_models";
    public int vrmCount = 5;
    public Camera captureCamera;

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

                string savePath = Path.Combine(Application.dataPath, "../Data/images", $"vrm_capture_{i}.png");
                await CaptureCameraImageAsync(captureCamera, savePath);
                Debug.Log($"画像保存完了: {savePath}");

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

    /// <summary>
    /// 指定カメラの出力をPNGとして保存する
    /// </summary>
    private async Task CaptureCameraImageAsync(Camera cam, string path)
    {
        string directory = Path.GetDirectoryName(path);
        Directory.CreateDirectory(directory);

        int width = Screen.width;
        int height = Screen.height;

        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        await Task.Yield();
    }
}
