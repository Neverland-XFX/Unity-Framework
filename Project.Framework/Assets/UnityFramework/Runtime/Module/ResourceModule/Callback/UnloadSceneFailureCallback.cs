﻿namespace UnityFramework
{
    /// <summary>
    /// 卸载场景失败回调函数。
    /// </summary>
    /// <param name="sceneAssetName">要卸载的场景资源名称。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void UnloadSceneFailureCallback(string sceneAssetName, object userData);
}
