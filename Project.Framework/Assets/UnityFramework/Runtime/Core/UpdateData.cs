
namespace UnityFramework
{
    /// <summary>
    /// 服务器状态。
    /// </summary>
    public enum ServerStatus
    {
        None = 0,

        /// <summary>
        /// 正常
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 维护中
        /// </summary>
        Maintained = 2,
    }

    /// <summary>
    /// APP更新类型。
    /// </summary>
    public enum UpdateType
    {
        None = 0,

        //资源更新
        ResourceUpdate = 1,

        //底包更新
        PackageUpdate = 2,
    }


    public enum GameStatus
    {
        First = 0,
        AssetLoad = 1
    }

    public class ServiceUpdateData
    {

        /// <summary>
        /// 服务器是否停服
        /// </summary>
        public ServerStatus ServerStatus;

        /// <summary>
        /// 停服公告内容简中
        /// </summary>
        public string ServerMaintainedContentChineseSimplified;

        /// <summary>
        /// 停服公告内容繁中
        /// </summary>
        public string ServerMaintainedContentChineseTraditional;

        /// <summary>
        /// 停服公告内容英文
        /// </summary>
        public string ServerMaintainedContentEnglish;

        /// <summary>
        /// 停服公告内容日文
        /// </summary>
        public string ServerMaintainedContentJapanese;

        /// <summary>
        /// 停服公告内容韩文
        /// </summary>
        public string ServerMaintainedContentKorean;

        /// <summary>
        /// 停服公告内容俄文
        /// </summary>
        public string ServerMaintainedContentRussian;

    }

    /// <summary>
    /// 版本更新数据。
    /// </summary>
    public class UpdateData
    {
        /// <summary>
        /// 当前版本信息。
        /// </summary>
        public string CurrentVersion;

        /// <summary>
        /// 是否底包更新。
        /// </summary>
        public UpdateType UpdateType;

        /// <summary>
        /// 是否强制更新。
        /// </summary>
        public UpdateStyle UpdateStyle;

        /// <summary>
        /// 是否提示。
        /// </summary>
        public UpdateNotice UpdateNotice;

        /// <summary>
        /// 热更资源地址。
        /// </summary>
        public string HostServerURL;

        /// <summary>
        /// 备用热更资源地址。
        /// </summary>
        public string FallbackHostServerURL;
    }
}