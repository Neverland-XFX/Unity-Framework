using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace UnityFramework
{
    /// <summary>
    /// 资源组件拓展。
    /// </summary>
    [DisallowMultipleComponent]
    internal partial class ResourceExtComponent : MonoBehaviour
    {
        public static ResourceExtComponent Instance { private set; get; }

        private readonly TimeoutController _timeoutController = new TimeoutController();

        /// <summary>
        /// 正在加载的资源列表。
        /// </summary>
        private readonly HashSet<string> _assetLoadingList = new HashSet<string>();

        /// <summary>
        /// 检查是否可以释放间隔
        /// </summary>
        [SerializeField]
        private float checkCanReleaseInterval = 30f;

        private float _checkCanReleaseTime = 0.0f;

        /// <summary>
        /// 对象池自动释放时间间隔
        /// </summary>
        [SerializeField]
        private float autoReleaseInterval = 60f;

        /// <summary>
        /// 保存加载的图片对象
        /// </summary>
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private LinkedList<LoadAssetObject> _loadAssetObjectsLinkedList;

        /// <summary>
        /// 散图集合对象池
        /// </summary>
        private IObjectPool<AssetItemObject> _assetItemPool;


#if UNITY_EDITOR
        public LinkedList<LoadAssetObject> LoadAssetObjectsLinkedList
        {
            get => _loadAssetObjectsLinkedList;
            set => _loadAssetObjectsLinkedList = value;
        }
#endif
        private IEnumerator Start()
        {
            Instance = this;
            yield return new WaitForEndOfFrame();
            IObjectPoolModule objectPoolComponent = ModuleSystem.GetModule<IObjectPoolModule>();
            _assetItemPool = objectPoolComponent.CreateMultiSpawnObjectPool<AssetItemObject>(
                "SetAssetPool",
                autoReleaseInterval, 16, 60, 0);
            _loadAssetObjectsLinkedList = new LinkedList<LoadAssetObject>();

            InitializedResources();
        }

        private void Update()
        {
            _checkCanReleaseTime += Time.unscaledDeltaTime;
            if (_checkCanReleaseTime < (double)checkCanReleaseInterval)
            {
                return;
            }

            ReleaseUnused();
        }

        /// <summary>
        /// 回收无引用的缓存资产。
        /// </summary>
#if ODIN_INSPECTOR
        [Button("Release Unused")]
#endif
        public void ReleaseUnused()
        {
            if (_loadAssetObjectsLinkedList == null)
            {
                return;
            }

            LinkedListNode<LoadAssetObject> current = _loadAssetObjectsLinkedList.First;
            while (current != null)
            {
                var next = current.Next;
                if (current.Value.AssetObject.IsCanRelease())
                {
                    _assetItemPool.Unspawn(current.Value.AssetTarget);
                    MemoryPool.Release(current.Value.AssetObject);
                    _loadAssetObjectsLinkedList.Remove(current);
                }

                current = next;
            }

            _checkCanReleaseTime = 0f;
        }

        private void SetAsset(ISetAssetObject setAssetObject, Object assetObject)
        {
            _loadAssetObjectsLinkedList.AddLast(new LoadAssetObject(setAssetObject, assetObject));
            setAssetObject.SetAsset(assetObject);
        }

        private async UniTask TryWaitingLoading(string assetObjectKey)
        {
            if (_assetLoadingList.Contains(assetObjectKey))
            {
                try
                {
                    await UniTask.WaitUntil(
                            () => !_assetLoadingList.Contains(assetObjectKey))
#if UNITY_EDITOR
                        .AttachExternalCancellation(_timeoutController.Timeout(TimeSpan.FromSeconds(60)));
                    _timeoutController.Reset();
#else
                    ;
#endif
                }
                catch (OperationCanceledException ex)
                {
                    if (_timeoutController.IsTimeout())
                    {
                        Log.Error($"LoadAssetAsync Waiting {assetObjectKey} timeout. reason:{ex.Message}");
                    }
                }
            }
        }
    }
}