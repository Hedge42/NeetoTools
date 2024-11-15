using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
#endif


namespace Neeto
{
    [ExecuteInEditMode]
    public class LoadingScreen : MonoBehaviour
    {
        public const string SCENE_NAME = "Loading Screen";

        static LoadingScreen _instance;
        public static LoadingScreen instance => _instance ??= GameObject.FindObjectOfType<LoadingScreen>();

        static LoadingSpriteIcon _icon;
        public static LoadingSpriteIcon icon => _icon ??= GameObject.FindObjectOfType<LoadingSpriteIcon>();


        public CanvasGroup background;
        public Behaviour[] subs;

        public static bool isLoading;

        CancellationTokenSource cts;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;

            cts = new CancellationTokenSource();
        }
        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;

            cts.Kill();
        }
        private void OnEnable()
        {
            foreach (var sub in subs)
                sub.enabled = true;

            instance.background.alpha = 0f;
        }
        private void OnDisable()
        {
            foreach (var sub in subs)
                sub.enabled = false;
        }

        public static async UniTask ActivateAsync()
        {
            LoadingScreen.isLoading = true;

            //instance.cts = instance.cts.Refresh();

            var current = SceneManager.GetSceneByName(SCENE_NAME);

            await UniTask.WaitWhile(() => current.isLoaded);

            var loadingScreenHandle = SceneManager.LoadSceneAsync(LoadingScreen.SCENE_NAME, LoadSceneMode.Additive);
            loadingScreenHandle.allowSceneActivation = true;
            await loadingScreenHandle;
            var loadingScene = SceneManager.GetSceneByName(LoadingScreen.SCENE_NAME);
            await Easing.EaseUnscaled(0, 1, 1, Easing.Method.EaseInSine, f => instance.background.alpha = f, token: Token.global);

        }
        public static async UniTask DeactivateAsync()
        {
            LoadingScreen.isLoading = false;

            await TaskHelper.LerpAsync(instance.background.alpha, 1f, PlayerLoopTiming.PostLateUpdate, true, Token.global, _ => instance.background.alpha = _)
                            .AttachExternalCancellation(Token.global);


            var handle = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(SCENE_NAME));
            //await handle.WithCancellation(instance.cts.Token);
            while (!handle.isDone)
                await UniTask.Yield();
        }

        public static async UniTask LoadAsync(ILoadAsync[] tasks)
        {
            await LoadingScreen.ActivateAsync();

            foreach (var t in tasks)
            {
                await t.LoadAsync();
                await UniTask.Yield();
            }

            await LoadingScreen.DeactivateAsync();
        }
    }
}