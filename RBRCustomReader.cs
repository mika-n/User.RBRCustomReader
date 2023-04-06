//
// RBRCustomReader - Custom plugin for Richard Burns Rally racing game.
//
// Copyright (c) 2023 mika-n, www.rallysimfans.hu. No promises and/or warranty given what so ever. This may or may not work. Use at your own risk.
//
// WTFPL licensed to public domain, free for commercial and personal use, modifications and redistribution allowed. http://www.wtfpl.net/
// If you modify and create a derivated version using this app then please leave the above shown copyright text in the derived work as a credit to the original work (readme.txt and source code files).
//
// SimHub addon installation:
// - Copy bin/release/RBRCustomReader.dll file into the SimHub application folder (ie. the folder where you have SimHubWPF.exe)
// - Launch SimHub and it will probably notify you it detected a new addon plugin.
//    - Enable the RBRCustomReader plugin
//    - No need to tick "show in left menu" option 
// - The plugin should be now shown as enabled in SimHub/Settings/Plugins tab page
// 
// How to use RSFCarName and RSFCarID custom properties in SimHub dashboard layout?
// - Add a text object into SimHub dashboard layout and set the text object to use NCalc formula with RSFCarName or RSFCarID property reference
// 
using GameReaderCommon;
using SimHub.Plugins;
using System;
using System.Windows.Media;
using System.Diagnostics;
using SimpleIniFile;

namespace User.RBRCustomReader
{
    [PluginDescription("RBR Custom property reader for Rallysimfans RBR mod")]
    [PluginAuthor("mika-n. www.rallysimfans.hu")]
    [PluginName("RBR Custom Reader")]
    public class RBRCustomReader : IPlugin, IDataPlugin, IWPFSettingsV2
    {
        // Custom RBR properties shown for SimHub NCalc expressions
        public int RSFCarID;       // RSF car ID (0...N)
        public string RSFCarName;  // The true car name and not just a model name as by default in SimHub (for example "Citroen DS3 R5" instead of just "DS3_R5")

        // The timestamp of the previous RBR racing session. Car attributes are updated only once per new session (=new session start time)
        private DateTime prevSessionStartTime;

        // RBR game path (initialized only once because no one probably re-installs RBR while SimHub rbr dashboard is running
        private string RBRGamePath;
        private string GetRBRGamePath()
        {
            if (string.IsNullOrEmpty(RBRGamePath))
            {
                Process[] rbrProcessArray = Process.GetProcessesByName("RichardBurnsRally_SSE");
                if (rbrProcessArray != null && rbrProcessArray.Length > 0)
                {
                    RBRGamePath = System.IO.Path.GetDirectoryName(rbrProcessArray[0].MainModule.FileName);
                    rbrProcessArray[0].Dispose();
                    
                    SimHub.Logging.Current.Info($"[RBRCustomReader] {RBRGamePath}\\RichardBurnsRally_SSE.exe");
                }
            }
            return RBRGamePath;
        }

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
        /// </summary>
        public ImageSource PictureIcon => this.ToIcon(Properties.Resources.sdkmenuicon);

        /// <summary>
        /// Gets a short plugin title to show in left menu. Return null if you want to use the title as defined in PluginName attribute.
        /// </summary>
        public string LeftMenuTitle => "RBR Custom Reader";

        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionnally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        ///
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
        ///
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data">Current game data, including current and previous data frame.</param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            //if (data.GameRunning && data.NewData != null && data.NewData.CarModel != oldCarModelName)
            if (data.SessionStartDate > prevSessionStartTime && data.NewData != null)
            {
                // A new car loaded in RBR. Refresh car attributes up-to-date
                //SimHub.Logging.Current.Info("[RBRCustomReader] New car selection. Refreshing car attributes");
                prevSessionStartTime = data.SessionStartDate.AddSeconds(2);

                if (string.IsNullOrEmpty(data.NewData.CarModel))
                {
                    RSFCarID = 0;
                    RSFCarName = "";
                }
                else
                {
                    // New car selection. Refresh settinsg up-to-date
                    int rsfCarSlotID = IniFile.ReadInt("cars", "slot", 5, $"{GetRBRGamePath()}\\rallysimfans.ini");

                    string carsIniFile = $"{GetRBRGamePath()}\\Cars\\Cars.ini";
                    RSFCarID = IniFile.ReadInt($"Car0{rsfCarSlotID}", "RSFCarID", 0, carsIniFile);
                    RSFCarName = IniFile.ReadString($"Car0{rsfCarSlotID}", "CarName", "", carsIniFile);
                }
            }
        }

        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here !
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {
            // Do nothing
        }

        /// <summary>
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return null;
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            SimHub.Logging.Current.Info("[RBRCustomReader] Starting the plugin");

            RSFCarID = 0;
            RSFCarName = "";

            prevSessionStartTime = DateTime.MinValue;

            // Declare a property available in the property list, this gets evaluated "on demand" (when shown or used in formulas)
            this.AttachDelegate("RSFCarID", () => RSFCarID);
            this.AttachDelegate("RSFCarName", () => RSFCarName);
        }
    }
}
