using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PositionSaver
{
    public class Main : Script
    {
        private string saveFilePath = "scripts/prop_positions.txt";
        private float searchRange = 100f; // Adjust as needed
        private ScriptSettings propPositionIni;
        private int propHash;
        private Keys saveKey = Keys.J;
        private bool readyToSave;
        private enum SwapStates { SetTimer, SaveProps, End }
        private SwapStates swapStates;
        private int timer;
        private List<Vector3> savedPositions = new List<Vector3>();
        int i;

        public Main()
        {
            KeyDown += OnKeyDown;
            Tick += OnTick;
            propPositionIni = ScriptSettings.Load("./scripts/PropPositions.ini");
            propHash = propPositionIni.GetValue<int>("SETTINGS", "Prop Hash", 0);
            searchRange = propPositionIni.GetValue<float>("SETTINGS", "Search Range", 10);
            saveKey = propPositionIni.GetValue<Keys>("SETTINGS", "Save Key", Keys.J);
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == saveKey)
            {
                GTA.UI.Notification.Show("Saving props...");
                i = 0;
                swapStates = SwapStates.SetTimer;
                readyToSave = true;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (readyToSave)
            {
                switch (swapStates)
                {
                    case SwapStates.SetTimer:
                        timer = Game.GameTime;
                        swapStates = SwapStates.SaveProps;
                        break;

                    case SwapStates.SaveProps:
                        if (Game.Player.Character.IsAlive)
                        {
                            Prop[] nearbyProps = World.GetNearbyProps(Game.Player.Character.Position, searchRange);
                            foreach (Prop prop in nearbyProps)
                            {
                                if (prop.Model.Hash == (uint)propHash)
                                {
                                    Vector3 propPosition = prop.Position;
                                    if (!savedPositions.Contains(propPosition))
                                    {
                                        i++;
                                        SavePropPosition(propPosition);
                                        savedPositions.Add(propPosition);
                                    }
                                }
                            }
                        }
                        if (Game.GameTime >= timer + 5000)
                        {
                            GTA.UI.Notification.Show("Saved any new props in range");
                            readyToSave = false;
                            savedPositions.Clear(); // Clear saved positions after saving
                        } 
                        break;
                }
            }
        }

        private void SavePropPosition(Vector3 position)
        {
            using (StreamWriter writer = File.AppendText(saveFilePath))
            {
                writer.WriteLine($"{i}: {position}");
            }
        }
    }
}
