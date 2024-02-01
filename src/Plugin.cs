using System;
using UnityEngine;
using BepInEx;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;
using System.Reflection;

namespace SpeebUp
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    class SpeebUp : BaseUnityPlugin
    {
        public const string pluginGuid = "plagg.peeb.speebup";
        public const string pluginName = "SpeebUp";
        public const string pluginVersion = "1.1";

        private string displayText = "";
        private readonly double[] bestSpeeds = new double[10];
        private double avgSum = 0;
        private int avgNum = 0;
        private bool recording = false;
        private int section = 0;
        private double best;
        private Vector3? teleport = null;
        private string teleportScene = "";
        private int sceneSelected = 0;
        private GUIStyle style;
        private GUIStyle styleLeft;
        private const string helpText = "Q: toggle recording, R: teleport (stops recording), Z: set teleport, 0-9: select section, T: reset section speed, Y: select scene, U: teleport scene, X: toggle green timer";
        private bool toggleHelp = false;
        private bool toggleGreen = false;
        private string displayIndicators = "selected scene: <color=\"blue\">Scn_Level_1</color>";
        private FieldInfo intervalTimer;
        private Flipper flipper;

        public void Awake()
        {
            Application.quitting += WriteToFile;
            if (!File.Exists("sections.txt"))
            {
                File.WriteAllText("sections.txt", $"SpeebUp manages section best speeds in this file. Speeds below go from sections 0 to 9. DO NOT EDIT :){new StringBuilder().Insert(0, $"{Environment.NewLine}00:00", 10)}");
            }
            else
            {
                bool error = false;
                string[] lines = File.ReadAllLines("sections.txt");
                if (lines.Length == 11) //10 speeds plus line text
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (double.TryParse(lines[i + 1], out double result))
                        {
                            bestSpeeds[i] = result;
                        }
                        else
                        {
                            error = true;
                            bestSpeeds[i] = 0;
                        }
                    }
                }
                else
                {
                    error = true;
                }
                if (error) //error has occurred!
                {
                    WriteToFile();
                }
            }
            best = bestSpeeds[0];
            SceneLoader.player = Resources.Load<GameObject>("Prefabs/Playerkit");
            style = new()
            {
                normal = new GUIStyleState(),
                fontSize = 14
            };
            style.normal.textColor = Color.black;
            style.normal.background = Texture2D.whiteTexture;
            style.alignment = TextAnchor.MiddleCenter;
            styleLeft = new()
            {
                normal = new GUIStyleState(),
                fontSize = 14
            };
            styleLeft.normal.textColor = Color.black;
            styleLeft.normal.background = Texture2D.whiteTexture;
            styleLeft.alignment = TextAnchor.MiddleLeft;
        }
        private void WriteToFile() //write all speeds to file
        {
            string text = "SpeebUp manages section best speeds in this file. Speeds below go from sections 0 to 9. DO NOT EDIT :)";
            for (int i = 0; i < 10; i++) //add speeds
            {
                text += $"{Environment.NewLine}{bestSpeeds[i]:00.00}";
            }
            File.WriteAllText("sections.txt", text);
        }

        public void OnGUI()
        {
            if (!SceneManager.GetActiveScene().name.Contains("Scn_Level")) //in menu
            {
                return;
            }
            if (toggleHelp)
            {
                GUI.Label(new Rect(0, 0, Screen.width, 20), helpText, style);
            }
            else
            {
                GUI.Label(new Rect(0, 0, Screen.width, 20), displayText, style);
                GUI.Label(new Rect(0, 0, 20, 20), displayIndicators, styleLeft);
            }
        }

        private void StopRecording(bool set = false)
        {
            if (recording)
            {
                if (bestSpeeds[section] < avgSum / avgNum) //new best!
                {
                    bestSpeeds[section] = best = Math.Round(avgSum / avgNum, 2);
                }
                avgSum = avgNum = 0;
                recording = set;
            }
        }

        private void LateUpdate()
        {
            if (SceneLoader.isLoading && SceneManager.GetActiveScene().name == SceneLoader.sceneLoading.Name) //finished loading scene
            {
                GlideController componentInChildren = Instantiate(SceneFader.playerPrefab, SceneLoader.greenSkipPosition, Quaternion.identity).GetComponentInChildren<GlideController>();
                componentInChildren.transform.position = SceneLoader.sceneLoading.PlayerPosition;
                componentInChildren.m_goalAngles = Vector3.zero;
                componentInChildren.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                SceneLoader.isLoading = false;
            }
        }
        private void Update()
        {
            if (SceneManager.GetActiveScene().name.Contains("Scn_Level")) //not in menu
            {
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    sceneSelected = (sceneSelected + 1) % 7; //restrict to max 6
                    displayIndicators = $"selected scene: <color=\"blue\">{SceneData.Scenes[sceneSelected].Name}</color>";
                }
                if (Input.GetKeyDown(KeyCode.X)) //toggle green timer
                {
                    toggleGreen = !toggleGreen;
                    if (toggleGreen)
                    {
                        intervalTimer = typeof(Flipper).GetField("m_intervalTimer");
                        flipper = GameObject.Find("Flipper").GetComponent<Flipper>();
                    }
                }
                if (Input.GetKeyDown(KeyCode.U) && !SceneLoader.isLoading && SceneManager.GetActiveScene().name != SceneData.Scenes[sceneSelected].Name)
                {
                    SceneLoader.LoadScene(SceneData.Scenes[sceneSelected]);
                }
                if (Input.GetKeyDown(KeyCode.H)) //reset section speed
                {
                    toggleHelp = !toggleHelp;
                }
                if (Input.GetKeyDown(KeyCode.R) && teleport != null && SceneManager.GetActiveScene().name == teleportScene) //teleport
                {
                    StopRecording();
                    GlideController.current.gameObject.transform.position = (Vector3)teleport;
                }
                if (Input.GetKeyDown(KeyCode.Z)) //set teleport position/scene
                {
                    teleport = GlideController.current.gameObject.transform.position;
                    teleportScene = SceneManager.GetActiveScene().name;
                }
                if (Input.GetKeyDown(KeyCode.T)) //reset section speed
                {
                    bestSpeeds[section] = best = 0;
                }
                if (!recording) //cannot switch sections when recording
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (Input.GetKeyDown((KeyCode)(i + 48))) //alpha0 = 48
                        {
                            section = i;
                            best = bestSpeeds[i];
                        }
                    }
                }
                if (Input.GetKeyDown(KeyCode.Q)) //toggle
                {
                    StopRecording(true);
                    recording = !recording;
                }
                double speed = Math.Round(GlideController.current.GetComponent<Rigidbody>().velocity.magnitude, 2); //get current speed
                if (recording)
                {
                    avgSum += speed;
                    avgNum++;
                    displayText = $"speed: <color=\"green\">{speed:00.00}</color> current avg: <color=\"green\">{avgSum / avgNum:00.00}</color> section best: <color=\"green\">{best:00.00}</color> section: <color=\"green\">{section}</color> || <color=\"green\">RECORDING?</color> || H: help";
                }
                else
                {
                    displayText = $"speed: <color=\"red\">{speed:00.00}</color> current avg: <color=\"red\">00.00</color> section best: <color=\"red\">{best:00.00}</color> section: <color=\"red\">{section}</color> || <color=\"red\">RECORDING?</color> || H: help";
                }
            }
            if (toggleGreen && SceneManager.GetActiveScene().name == "Scn_Level_2_Sub_Green")
            {
                displayIndicators += $"green timer: <color=\"blue\">{(float)intervalTimer.GetValue(flipper):00.00}</color>";
            }
        }
    }
}