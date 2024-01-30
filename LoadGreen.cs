using UnityEngine.SceneManagement;

namespace SpeebUp
{
    public static class LoadGreen
    {
        public static void Load()
        {
            SceneManager.LoadSceneAsync("Scn_Level_2_Sub_Green"); //bugged
        }
    }
}