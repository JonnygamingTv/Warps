﻿using Rocket.API;
using Rocket.Core.Logging;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Warps
{
    public class WarpDataManager
    {
        private Dictionary<string, Warp> WarpsData = new Dictionary<string, Warp>(StringComparer.InvariantCultureIgnoreCase);


        public WarpDataManager()
        {
            Load();
        }

        private void Load()
        {
            // Try to load the records from the config file to the Warps Dictionary.
            foreach (Warp warpData in Warps.Instance.Configuration.Instance.Warps)
            {
                try
                {
                    // sanity checks for warps data.
                    if (warpData.Name == null || warpData.Name == string.Empty)
                    {
                        Logger.LogWarning("Error: No warp name on record, Skipping!");
                        continue;
                    }
                    if (warpData.World == null || warpData.World == string.Empty)
                    {
                        Logger.LogWarning("Error: No world set to record, Skipping!");
                        continue;
                    }
                    if (warpData.SetterCharName == null)
                        warpData.SetterCharName = "";
                    if (warpData.SetterSteamName == null)
                        warpData.SetterSteamName = "";

                    WarpsData.Add(warpData.GetKey(), warpData);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Error: Unable to load a warp record.");
                }
            }
        }

        private void SaveWarps()
        {
            // Save the warps out to the config file.
            Warps.Instance.Configuration.Instance.Warps = WarpsData.Values.ToList();
            Warps.Instance.Configuration.Save();

        }

        public List<Warp> SearchWarps(string name, IRocketPlayer caller)
        {
            if (name == null)
            {
                return WarpsData.Values.Where(warpData => warpData.World.ToLower() == Warps.MapName && caller.HasPermission("warp." + warpData.Name)).OrderBy(warp => warp.Name).ToList();
            }
            return WarpsData.Values.Where(warpData => warpData.Name.Contains(name.ToLower()) && warpData.World.ToLower() == Warps.MapName && caller.HasPermission("warp." + warpData.Name)).OrderBy(warp => warp.Name).ToList();
        }

        public List<Warp> SearchWarps(CSteamID cSteamID)
        {
            return WarpsData.Values.Where(warpData => warpData.SetterCSteamID == cSteamID && warpData.World.Sanitze().ToLower() == Warps.MapName.Sanitze().ToLower()).OrderBy(warp => warp.Name).ToList();
        }

        public Warp GetWarp(string name)
        {
            if(WarpsData.TryGetValue((Warps.MapName + "." + name).Sanitze(), out Warp val)) return val;
            return WarpsData.Values.FirstOrDefault(warpData => warpData.Name.Sanitze().ToLower() == name.Sanitze().ToLower() && warpData.World.Sanitze().ToLower() == Warps.MapName.Sanitze().ToLower());
        }

        public bool SetWarp(Warp warpData)
        {
            try
            {
                if (WarpsData.ContainsKey(warpData.GetKey().Sanitze()))
                {
                    WarpsData.Remove(warpData.GetKey().Sanitze());
                }
                WarpsData.Add(warpData.GetKey().Sanitze(), warpData);
                SaveWarps();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error: Unable to set warp.");
                return false;
            }
        }

        public bool RemoveWarp(string key)
        {
            if (WarpsData.ContainsKey(key.Sanitze()))
            {
                WarpsData.Remove(key.Sanitze());
                SaveWarps();
                return true;
            }
            return false;
        }

        public int RemoveWarpAll(string mapName)
        {
            List<Warp> list = WarpsData.Values.Where(warpData => warpData.World.ToLower() == mapName.ToLower()).ToList();
            if (list.Count > 0)
            {
                foreach ( Warp entry in list)
                {
                    WarpsData.Remove(entry.GetKey());
                }
                SaveWarps();
                return list.Count;
            }
            return 0;
        }
    }
}
