using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MyTools.Services
{
    /// <summary>
    /// Data model for a saved category selection set.
    /// </summary>
    public class SelectionSetData
    {
        public string Name { get; set; } = string.Empty;
        public List<string> CategoryNames { get; set; } = new List<string>();
    }

    /// <summary>
    /// Container for persisted selection sets.
    /// </summary>
    public class SelectionSetsFile
    {
        public SelectionSetData? Set1 { get; set; }
        public SelectionSetData? Set2 { get; set; }
    }

    /// <summary>
    /// Manages saving and loading of category selection sets.
    /// </summary>
    public static class SelectionSetService
    {
        private const string SettingsFileName = "SelectionSets.json";
        private const string AppFolderName = "MyTools";

        private static string GetSettingsFilePath()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string myToolsFolder = Path.Combine(appDataFolder, AppFolderName);

            if (!Directory.Exists(myToolsFolder))
                Directory.CreateDirectory(myToolsFolder);

            return Path.Combine(myToolsFolder, SettingsFileName);
        }

        public static SelectionSetsFile LoadSelectionSets()
        {
            string filePath = GetSettingsFilePath();

            if (!File.Exists(filePath))
                return new SelectionSetsFile();

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<SelectionSetsFile>(json) ?? new SelectionSetsFile();
            }
            catch
            {
                return new SelectionSetsFile();
            }
        }

        public static void SaveSelectionSet(int setNumber, string name, List<string> categoryNames)
        {
            if (setNumber < 1 || setNumber > 2)
                throw new ArgumentException("Set number must be 1 or 2", nameof(setNumber));

            SelectionSetsFile sets = LoadSelectionSets();

            SelectionSetData newSet = new SelectionSetData
            {
                Name = name,
                CategoryNames = categoryNames
            };

            if (setNumber == 1)
                sets.Set1 = newSet;
            else
                sets.Set2 = newSet;

            string json = JsonConvert.SerializeObject(sets, Formatting.Indented);
            File.WriteAllText(GetSettingsFilePath(), json);
        }

        public static SelectionSetData? GetSelectionSet(int setNumber)
        {
            if (setNumber < 1 || setNumber > 2)
                return null;

            SelectionSetsFile sets = LoadSelectionSets();
            return setNumber == 1 ? sets.Set1 : sets.Set2;
        }

        public static void ApplySelectionSet(SelectionSetData set, IEnumerable<CategoryItem> categories)
        {
            foreach (var cat in categories)
                cat.IsSelected = set.CategoryNames.Contains(cat.Name);
        }
    }
}
