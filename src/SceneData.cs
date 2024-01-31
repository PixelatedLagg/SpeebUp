using UnityEngine;

namespace SpeebUp
{
    public class SceneData
    {
        public string Name;
        public Vector3 PlayerPosition;
        public SceneData(string name, Vector3 playerPosition)
        {
            Name = name;
            PlayerPosition = playerPosition;
        }
        public static readonly SceneData[] Scenes = new SceneData[]
        {
            new SceneData("Scn_Level_1", new Vector3(19.29f, 42.939f, 144.63f)),
            new SceneData("Scn_Level_2", new Vector3(4.09f, -0.913f, -43.3494f)),
            new SceneData("Scn_Level_2_Sub", new Vector3(76.357f, 1.8f, -34.0232f)),
            new SceneData("Scn_Level_2_Sub_Blue", new Vector3(30.0844f, -9.719f, -0.5785f)),
            new SceneData("Scn_Level_2_Sub_Green", new Vector3(-0.3988f, -8.574f, 91.65f)),
            new SceneData("Scn_Level_3", new Vector3(-0.3683f, 0.542f, -26.7358f)),
            new SceneData("Scn_Level_4", new Vector3(-0.4f, 0.371f, -0.22f))
        };
    }
}