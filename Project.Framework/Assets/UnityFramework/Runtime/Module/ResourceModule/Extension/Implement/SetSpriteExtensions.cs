using System.Threading;
using UnityFramework;
using UnityEngine;
using UnityEngine.UI;

public static class SetSpriteExtensions
{
    /// <summary>
    /// 设置图片。
    /// </summary>
    /// <param name="image">UI/Image。</param>
    /// <param name="location">资源定位地址。</param>
    /// <param name="setNativeSize">是否使用原始分辨率。</param>
    /// <param name="cancellationToken">取消设置资源的Token。</param>
    public static void SetSprite(this Image image, string location, bool setNativeSize = false, CancellationToken cancellationToken = default)
    {
        ResourceExtComponent.Instance.SetAssetByResources<Sprite>(SetSpriteObject.Create(image, location, setNativeSize, cancellationToken)).Forget();
    }

    /// <summary>
    /// 设置图片。
    /// </summary>
    /// <param name="spriteRenderer">2D/SpriteRender。</param>
    /// <param name="location">资源定位地址。</param>
    /// <param name="cancellationToken">取消设置资源的Token。</param>
    public static void SetSprite(this SpriteRenderer spriteRenderer, string location, CancellationToken cancellationToken = default)
    {
        ResourceExtComponent.Instance.SetAssetByResources<Sprite>(SetSpriteObject.Create(spriteRenderer, location, cancellationToken)).Forget();
    }
}