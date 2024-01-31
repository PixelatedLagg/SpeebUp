using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeebUp
{
    public static class SceneLoader
    {
        public static GameObject player;
        public static readonly Vector3 greenSkipPosition = new(44.17f, -8.47f, 127.7f);
        public static bool isLoading = false;
        public static SceneData sceneLoading;
        public static void LoadScene(SceneData data)
        {
            sceneLoading = data;
            SceneManager.LoadScene(sceneLoading.Name);
            GlideController.PauseSet(false);
            GlideController.fading = false;
            isLoading = true;
        }
    }
}