using UnityEngine;
using UnityEngine.UI;

public class GKUIStripImage : MonoBehaviour
{

    public enum ImageType
    {
        Raw = 0,
        Sprite
    }

    public ImageType type = ImageType.Raw;
    public string assetBundle = "";
    public string assetName = "";

    // Use this for initialization
    void Start()
    {
        switch (type)
        {
            case ImageType.Raw:
                RawImage raw = gameObject.GetComponent<RawImage>();
                AssetBundleController.Instance().DownloadTexture(assetBundle, assetName, ref raw);
                break;
            case ImageType.Sprite:
                Image image = gameObject.GetComponent<Image>();
                AssetBundleController.Instance().DownloadSprite(assetBundle, assetName, ref image);
                break;
        }
    }
}

