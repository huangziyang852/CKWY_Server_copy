using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Common
{
    public class TableLoader
    {
        public string Dir { get; }

        private readonly Dictionary<string, string> cache = new();

        private static readonly Lazy<TableLoader> instance = new(() => new TableLoader());

        private Dictionary<string, string> jsonCache = new Dictionary<string, string>();

        public static TableLoader Instance => instance.Value;

        private cfg.Tables masterTables;

        public cfg.Tables MasterTables
        {
            get
            {
                return masterTables;
            }
        }

        public TableLoader()
        {
            string baseDir = AppContext.BaseDirectory;

            Dir = Path.Combine(baseDir, "Master", "json");

            LoadMasterTable();
        }

        public void LoadMasterTable()
        {
            masterTables = new cfg.Tables(file =>
            {
                if (!jsonCache.TryGetValue(file, out string jsonString))
                { 
                    if (file == null)
                    {
                        Logger.Instance.Error($"未找到 JSON 文件: {file}.json");
                        return null;
                    }
                    jsonString = File.ReadAllText($"{Dir}/{file}.json");
                    jsonCache[file] = jsonString; // 缓存数据
                    Logger.Instance.Information("加载了数据表"+file);
                }
                return JSON.Parse(jsonString);
            });
        }
    }
}
