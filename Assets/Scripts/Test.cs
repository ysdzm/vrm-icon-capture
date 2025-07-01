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

                // Hairオブジェクトを探す（1つ目を使う）
                Renderer hairRenderer = null;
                var hairRenderers = instance.gameObject.GetComponentsInChildren<Renderer>(true);
                foreach (var r in hairRenderers)
                {
                    if (r.name.ToLower().Contains("hair"))
                    {
                        hairRenderer = r;
                        break;
                    }
                }

                if (hairRenderer == null)
                {
                    Debug.LogWarning("Hairオブジェクトが見つかりませんでした");
                }
                else
                {
                    // 赤・緑・青でループして髪色を変えて撮影
                    Color[] colors = { Color.red, Color.green, Color.blue };
                    string[] colorNames = { "red", "green", "blue" };

                    for (int c = 0; c < colors.Length; c++)
                    {
                        ApplyColorToHairTexture(hairRenderer, colors[c]);

                        await Task.Delay(1000); // 表示反映待ち

                        string savePath = Path.Combine(
                            Application.dataPath,
                            "../Data/images",
                            $"vrm_capture_{i}_{colorNames[c]}.png"
                        );

                        await CaptureCameraImageAsync(captureCamera, savePath);
                        Debug.Log($"画像保存完了: {savePath}");
                    }
                }

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
    private void ApplyColorToHairTexture(Renderer renderer, Color color)
    {
        var mat = renderer.material;

        if (!mat.HasProperty("_MainTex")) return;

        var originalTex = mat.GetTexture("_MainTex") as Texture2D;
        if (originalTex == null) return;

        // 読み取り可能なテクスチャを作成
        Texture2D readableTex = MakeReadableTexture(originalTex);

        // 新しい色付きテクスチャを作成
        Texture2D newTex = new Texture2D(readableTex.width, readableTex.height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[readableTex.width * readableTex.height];

        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % readableTex.width;
            int y = i / readableTex.width;
            float alpha = readableTex.GetPixel(x, y).a;
            pixels[i] = new Color(color.r, color.g, color.b, alpha);
        }

        newTex.SetPixels(pixels);
        newTex.Apply();

        // 両方のプロパティに代入
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", newTex);
        if (mat.HasProperty("_ShadeTex")) mat.SetTexture("_ShadeTex", newTex);
    }

    private Texture2D MakeReadableTexture(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0);
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;

        Texture2D readableTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readableTex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        readableTex.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return readableTex;
    }


    /// <summary>
    /// 指定カメラの出力をPNGとして保存する
    /// </summary>
    private async Task CaptureCameraImageAsync(Camera cam, string path)
    {
        string directory = Path.GetDirectoryName(path);
        Directory.CreateDirectory(directory);

        // Unity の標準スクリーンショット機能を使用
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log($"スクリーンショット要求: {path}");

        // スクリーンショット保存には少し時間がかかるので少し待機（1秒ほどが確実）
        await Task.Delay(1000);
    }
}
