﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Drawing;
using System.IO;
using System.Threading;


namespace MSniper.Settings
{
    public interface IConfigs
    {
        string TranslationLanguageCode { get; set; }
        int CloseDelaySec { get; set; }
        bool DeleteTempFolder { get; set; }
        bool DownloadNewVersion { get; set; }
        Color Error { get; }
        string ErrorColor { get; set; }
        Color Highlight { get; }
        string HighlightColor { get; set; }
        Color Notification { get; }
        string NotificationColor { get; set; }
        bool ShowActiveBots { get; set; }
        Color Success { get; }
        string SuccessColor { get; set; }
        Color Warning { get; }
        string WarningColor { get; set; }
        void Save(string fullPath);
    }

    public class Configs : IConfigs
    {
        public string TranslationLanguageCode { get; set; } = "en";
        public int CloseDelaySec { get; set; } = 10;
        public bool DeleteTempFolder { get; set; } = true;
        public bool DownloadNewVersion { get; set; } = true;
        public bool ShowActiveBots { get; set; } = true;

        public string ErrorColor { get; set; } = ColorToHex(Color.FromArgb(255, 0x20, 0));

        public string HighlightColor { get; set; } = ColorToHex(Color.White);

        public string NotificationColor { get; set; } = ColorToHex(Color.FromArgb(0, 0xfa, 0xbf));

        public string SuccessColor { get; set; } = ColorToHex(Color.FromArgb(0, 255, 0));

        public string WarningColor { get; set; } = ColorToHex(Color.FromArgb(0xff, 0xc8, 0));


        [JsonIgnore]
        public Color Error { get { return HexToColor(ErrorColor); } }
        [JsonIgnore]
        public Color Highlight { get { return HexToColor(HighlightColor); } }
        [JsonIgnore]
        public Color Notification { get { return HexToColor(NotificationColor); } }
        [JsonIgnore]
        public Color Success { get { return HexToColor(SuccessColor); } }
        [JsonIgnore]
        public Color Warning { get { return HexToColor(WarningColor); } }

        public static string ColorToHex(Color c)
        {
            return $"#{c.R.ToString("X2")}{c.G.ToString("X2")}{c.B.ToString("X2")}";
        }
        public static Color HexToColor(string hex)
        {
            return ColorTranslator.FromHtml(hex);
        }

        public static Configs Load()
        {
            Configs _settings = new Configs();
            string configFile = Variables.SettingPath;
            if (File.Exists(configFile))
            {
                try
                {
                    //if the file exists, load the settings
                    string input = "";
                    int count = 0;
                    while (true)
                    {
                        try
                        {
                            input = File.ReadAllText(configFile);

                            break;
                        }
                        catch (Exception exception)
                        {
                            if (count > 10)
                            {
                                //sometimes we have to wait close to config.json for access
                                Program.frm.Console.WriteLine("configFile: " + exception.Message, Color.Red);
                            }
                            count++;
                            Thread.Sleep(1000);
                        }
                    };

                    var jsonSettings = new JsonSerializerSettings();
                    jsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                    jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;

                    try
                    {
                        _settings = JsonConvert.DeserializeObject<Configs>(input, jsonSettings);
                        SaveFiles(_settings);
                    }
                    catch (JsonSerializationException exception)
                    {
                        Program.frm.Console.WriteLine("Settings.json WRONG FORMAT: " + exception.Message, Color.Red);
                        Program.frm.Delay(30);
                    }
                }
                catch (JsonReaderException exception)
                {
                    Program.frm.Console.WriteLine("JSON Exception: " + exception.Message, Color.Red);
                    return _settings;
                }
            }

            return _settings;
        }

        public static void SaveFiles(Configs settings)
        {
            settings.Save(Variables.SettingPath);
        }

        public void Save(string fullPath)
        {
            var output = JsonConvert.SerializeObject(this, Formatting.Indented,
                new StringEnumConverter { CamelCaseText = true });

            var folder = Path.GetDirectoryName(fullPath);
            if (folder != null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(fullPath, output);
        }
    }
}