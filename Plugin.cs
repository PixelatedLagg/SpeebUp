using System;
using UnityEngine;
using BepInEx;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;

namespace SpeebUp
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    class SpeebUp : BaseUnityPlugin
    {
        public const string pluginGuid = "plagg.peeb.speebup";
        public const string pluginName = "SpeebUp";
        public const string pluginVersion = "1.0";
        private string text = "";
        private readonly double[] bestSpeeds = new double[10];
        private double avgSum = 0;
        private int avgNum = 0;
        private bool recording = false;
        private int section = 0;
        private double best;
        private const string info = "Q: toggle recording, R: teleport (stops recording), Z: set teleport, 0-9: select section, T: reset section speed";
        private Vector3? teleport = null;
        private string teleportScene = "";
        //private readonly Vector3 greenSkipPosition = new(44.17f, -8.47f, 127.7f);

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
            GUIStyle style = new()
            {
                normal = new GUIStyleState()
            };
            style.normal.textColor = Color.black;
            style.normal.background = Texture2D.whiteTexture;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, 0, Screen.width, 20), text, style);
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

        public void Update()
        {
            if (SceneManager.GetActiveScene().name.Contains("Scn_Level")) //not in menu
            {
                /*if (Input.GetKeyDown(KeyCode.X)) //practice level 2 skip //CANNOT GET WORKING ATM
                {
                    StopRecording();
                    LoadGreen.Load();
                }*/
                if (Input.GetKeyDown(KeyCode.R) && teleport != null && SceneManager.GetActiveScene().name == teleportScene) //teleport
                {
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
                    text = $"speed: <color=\"green\">{speed:00.00}</color> current avg: <color=\"green\">{avgSum / avgNum:00.00}</color> section best: <color=\"green\">{best:00.00}</color> section: <color=\"green\">{section}</color> || <color=\"green\">RECORDING?</color> || {info}";
                }
                else
                {
                    text = $"speed: <color=\"red\">{speed:00.00}</color> current avg: <color=\"red\">00.00</color> section best: <color=\"red\">{best:00.00}</color> section: <color=\"red\">{section}</color> || <color=\"red\">RECORDING?</color> || {info}";
                }
            }
        }
    }
}

/*
Scn_Level_2_Sub_Green
44.17
-8.47
127.7
*/