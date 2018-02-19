﻿using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using UniLinq;
using UnityEngine;

namespace LunaClient.Windows.Debug
{
    public partial class DebugWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DisplayFast = GUILayout.Toggle(DisplayFast, "Fast debug update", ButtonStyle);

            DisplayVectors = GUILayout.Toggle(DisplayVectors, "Display vessel vectors", ButtonStyle);
            if (DisplayVectors)
                GUILayout.Label(VectorText, LabelStyle);

            DisplayOrbit = GUILayout.Toggle(DisplayOrbit, "Display orbit info", ButtonStyle);
            if (DisplayOrbit)
                GUILayout.Label(OrbitText, LabelStyle);

            DisplayVesselStoreData = GUILayout.Toggle(DisplayVesselStoreData, "Display vessel store data", ButtonStyle);
            if (DisplayVesselStoreData)
                GUILayout.Label(VesselStoreText, LabelStyle);

            DisplayNtp = GUILayout.Toggle(DisplayNtp, "Display NTP/Subspace statistics", ButtonStyle);
            if (DisplayNtp)
                GUILayout.Label(NtpText, LabelStyle);

            DisplayConnectionQueue = GUILayout.Toggle(DisplayConnectionQueue, "Display connection statistics", ButtonStyle);
            if (DisplayConnectionQueue)
                GUILayout.Label(ConnectionText, LabelStyle);

            if (GUILayout.Button("Reload all vessels", ButtonStyle))
            {
                var protos = VesselsProtoStore.AllPlayerVessels.Values.Select(v => v.ProtoVessel);
                foreach (var proto in protos)
                {
                    VesselLoader.ReloadVessel(proto);
                }
            }


            GUILayout.EndVertical();
        }
    }
}