using System;
using System.IO;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;

namespace DrawAnywhere.Models
{
    class AppConfig
    {
        public string ScreenShotPath { get; set; }
        public bool WindowsStartupEnabled { get; set; }
        public bool CleanCanvasWhenHide { get; set; }

        public int PenStrokeHeight { get; set; } = 6;
        public int PenStrokeWidth { get; set; } = 6;
        public bool HighlighterMode { get; set; } = false;
        public bool IgnoreStylusPressure { get; set; } = false;

        public static AppConfig Instance()
        {
            if (_instance == null)
                _instance = GetDefaultConfig();
            return _instance;
        }

        private static AppConfig _instance;

        public static AppConfig GenerateDefaultConfig()
        {
            var cfg = new AppConfig();

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "DrawAnywhere!");
            cfg.ScreenShotPath = path;

            cfg.WindowsStartupEnabled = false;

            var tomlDocument = new TomlTable
            {
                ["shell"] = new TomlTable
                {
                    ["screenshot_path"] = path,
                    ["on_hide_cleanup"] = true,
                    ["startup_enabled"] = false
                },
                ["pen"] = new TomlTable
                {
                    ["stroke_width"] = 6,
                    ["stroke_height"] = 6,
                    ["highlighter_mode"] = false,
                    ["ignore_stylus_pressure"] = false
                }
            };

            File.WriteAllText("config.toml", Toml.FromModel(tomlDocument));

            _instance = cfg;
            return cfg;
        }

        public static AppConfig GetDefaultConfig()
        {
            if (File.Exists("config.toml"))
                return FromToml("config.toml");

            return GenerateDefaultConfig();
        }

        public static AppConfig FromToml(string toml)
        {
            if (!File.Exists(toml))
                return null;
            
            var config = new AppConfig();

            var tomlDoc = Toml.Parse(File.ReadAllText(toml)).ToModel();

            if (tomlDoc is TomlTable table)
            {
                if (table.ContainsKey("shell") && table["shell"] is TomlTable shellTable)
                {
                    config.ScreenShotPath = shellTable["screenshot_path"]?.ToString();
                    config.WindowsStartupEnabled = bool.Parse(shellTable["startup_enabled"].ToString() ?? "false");
                    config.CleanCanvasWhenHide = bool.Parse(shellTable["on_hide_cleanup"].ToString() ?? "true");
                }

                if (table.ContainsKey("pen") && table["pen"] is TomlTable penTable)
                {
                    config.PenStrokeHeight = int.Parse(penTable["stroke_height"].ToString() ?? "6");
                    config.PenStrokeWidth = int.Parse(penTable["stroke_width"].ToString() ?? "6");
                    config.HighlighterMode = bool.Parse(penTable["highlighter_mode"].ToString() ?? "false");
                    config.IgnoreStylusPressure = bool.Parse(penTable["ignore_stylus_pressure"].ToString() ?? "false");
                }
            }

            _instance = config;
            return config;
        }

        public async Task SaveAsync()
        {
            var tomlDocument = new TomlTable
            {
                ["shell"] = new TomlTable
                {
                    ["screenshot_path"] = ScreenShotPath,
                    ["startup_enabled"] = WindowsStartupEnabled,
                    ["on_hide_cleanup"] = CleanCanvasWhenHide,
                },
                ["pen"] = new TomlTable
                {
                    ["stroke_width"] = PenStrokeWidth,
                    ["stroke_height"] = PenStrokeHeight,
                    ["highlighter_mode"] = HighlighterMode,
                    ["ignore_stylus_pressure"] = IgnoreStylusPressure
                }
            };

            var tomlContent = Toml.FromModel(tomlDocument);
            await File.WriteAllTextAsync("config.toml", tomlContent);
        }

        public async void Save()
        {
            await SaveAsync();
        }
    }
}
