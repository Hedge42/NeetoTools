using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Collections.Generic;
using Neeto;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif


[Serializable]
public class LoadingTask
{
    [SerializeReference, Polymorphic]
    public ILoadAsync[] tasks = new ILoadAsync[]
    {
        new UnloadScenes(),
        new LoadScene()
    };

    public void Load()
    {
        if (!LoadingScreen.isLoading)
            LoadingScreen.LoadAsync(tasks).Forget();
    }
}

public interface ILoadAsync
{
    UniTask LoadAsync();
}

[Serializable]
public class LoadScene : ILoadAsync
{
    public Neeto.SceneReference scene;
    public bool setActive;

    public async UniTask LoadAsync()
    {
        if (SceneRef.IsSceneAvailable(scene))
        {
            Debug.Log($"Tried loading '{scene.name}' but it was already loaded");
            goto Activate;
        }

        Debug.Log($"Loading scene '{scene.name}'");

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
            goto Activate;
        }
#endif
        var handle = SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);
        handle.allowSceneActivation = true;
        while (!handle.isDone)
            await UniTask.Yield();

        Activate:
        if (setActive)
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene.name));

    }
}

[Serializable]
public class LoadScriptableScene : ILoadAsync
{
    public ScriptableLoadingTask loader;

    public async UniTask LoadAsync()
    {
        foreach (var t in loader.task.tasks)
            await t.LoadAsync();
    }
}

[Serializable]
public class UnloadScene : ILoadAsync
{
    public Neeto.SceneReference scene;
    public async UniTask LoadAsync()
    {
        if (!SceneRef.IsSceneAvailable(scene))
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorSceneManager.CloseScene(SceneManager.GetSceneByName(scene), true);
            return;
        }
#endif

        var handle = SceneManager.UnloadSceneAsync(scene);
        while (!handle.isDone)
            await UniTask.Yield();
    }
}

[Serializable]
public struct UnloadScenes : ILoadAsync
{
    public async UniTask LoadAsync()
    {
        List<UnityEngine.SceneManagement.Scene> unload = new List<UnityEngine.SceneManagement.Scene>();

        UnityEngine.SceneManagement.Scene scene;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            scene = SceneManager.GetSceneAt(i);

            // don't unload the loading screen
            if (!scene.name.Equals("Loading Screen") && scene.IsValid())
                unload.Add(scene);
        }
        while (unload.Count > 0)
        {
            scene = unload[0];
            Debug.Log($"Unloading scene '{scene.name}'");
            var handle = SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

            while (!handle.isDone)
            {
                await UniTask.Yield();
            }
            unload.Remove(scene);
        }
    }
}

[Serializable]
public class LoadSceneByName : ILoadAsync
{
    public string sceneName;
    public bool setActive;
    public async UniTask LoadAsync()
    {
        Debug.Log($"Loading scene '{sceneName}'");
        var handle = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!handle.isDone)
            await UniTask.Yield();

        if (setActive)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }
    }
}

[Serializable]
public class UnloadSceneByName : ILoadAsync
{
    public string sceneName;
    public async UniTask LoadAsync()
    {
        Debug.Log($"Unloading scene '{sceneName}'");


        var handle = SceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        while (!handle.isDone)
        {
            await UniTask.Yield();
        }
    }
}

[Serializable]
public class UnloadAddressableScene : ILoadAsync
{
    public AssetReference sceneReference;
    public async UniTask LoadAsync()
    {
        if (!sceneReference.RuntimeKeyIsValid())
        {
            Debug.LogError("Invalid scene reference");
            return;
        }

        // await find scene in addressables
        var location = Addressables.LoadResourceLocationsAsync(sceneReference);
        await location;
        var result = location.Result;
        if (result.Count == 0)
        {
            Debug.LogError("Scene not found in Addressables");
            return;
        }

        // is it already loaded?
        var scenePath = result[0].InternalId;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.path == scenePath)
            {
                await SceneManager.UnloadSceneAsync(scene);
                return;
            }
        }
    }
}

[Serializable]
public class LoadAddressableScene : ILoadAsync
{
    public AssetReference sceneReference;
    public bool setActive;
    public async UniTask LoadAsync()
    {
        if (!sceneReference.RuntimeKeyIsValid())
        {
            Debug.LogError("Invalid scene reference");
            return;
        }

        // await find scene in addressables
        var location = Addressables.LoadResourceLocationsAsync(sceneReference);

        while (!location.IsDone)
        {
            await UniTask.Yield();
        }

        var result = location.Result;
        if (result.Count == 0)
        {
            Debug.LogError("Scene not found in Addressables");
            return;
        }

        // is it already loaded?
        var scenePath = result[0].InternalId;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.path == scenePath)
            {
                //scenes.Add(SceneManager.GetSceneByPath(scenePath));
                Debug.Log($"Scene {scene.name} already loaded");
                return;
            }
        }

        // load the scene
        Debug.Log($"Loading scene '{location.Result[0].InternalId}'");
        var handle = Addressables.LoadSceneAsync(sceneReference, LoadSceneMode.Additive);

        while (!handle.IsDone)
        {
            await UniTask.Yield();
        }

        //await handle.ToUniTask();

        if (setActive)
        {
            SceneManager.SetActiveScene(handle.Result.Scene);
        }
    }
}
