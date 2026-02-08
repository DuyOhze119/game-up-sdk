using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace GameUpSDK.Editor
{
    /// <summary>
    /// Editor utility to install GameUp SDK 3rd party dependencies via Unity Package Manager (UPM).
    /// Modifies Packages/manifest.json to add scoped registries and dependencies, then triggers resolve.
    /// </summary>
    public static class SDKInstaller
    {
        private const string MenuPath = "GameUp SDK/Install All Dependencies";
        private const string ManifestRelativePath = "Packages/manifest.json";

        private static readonly (string Name, string Url, string[] Scopes)[] Registries =
        {
            ("Google Game SDKs", "https://unity-sdk-registry.json.googleapis.com/", new[] { "com.google" }),
            ("LevelPlay", "https://ll.ads.unity3d.com/registry", new[] { "com.unity.services.levelplay", "com.ironsource.sdk" }),
        };

        private static readonly Dictionary<string, string> Packages = new Dictionary<string, string>
        {
            { "com.google.ads.mobile", "24.9.0" },
            { "com.google.firebase.analytics", "13.7.0" },
            { "com.google.firebase.app", "13.7.0" },
            { "com.unity.services.levelplay", "9.3.0" },
            { "com.appsflyer.unity", "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#upm" },
        };

        [MenuItem(MenuPath)]
        public static void InstallAllDependencies()
        {
            if (!EditorUtility.DisplayDialog(
                "GameUp SDK – Install Dependencies",
                "This will add Google, IronSource, and AppsFlyer dependencies to your project. Continue?",
                "Continue",
                "Cancel"))
                return;

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string manifestPath = Path.Combine(projectRoot, ManifestRelativePath);

            if (!File.Exists(manifestPath))
            {
                Debug.LogError("[GameUpSDK] manifest.json not found at: " + manifestPath);
                return;
            }

            string content;
            try
            {
                content = File.ReadAllText(manifestPath);
            }
            catch (Exception e)
            {
                Debug.LogError("[GameUpSDK] Failed to read manifest: " + e.Message);
                return;
            }

            if (!TryMergeManifest(content, out string newContent))
            {
                Debug.LogError("[GameUpSDK] Failed to merge manifest (invalid JSON or structure).");
                return;
            }

            try
            {
                File.WriteAllText(manifestPath, newContent, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogError("[GameUpSDK] Failed to write manifest: " + e.Message);
                return;
            }

            Client.Resolve();
            Debug.Log("[GameUpSDK] Dependencies installed successfully. Unity is resolving packages...");
        }

        /// <summary>
        /// Merges registries and packages into manifest JSON. Preserves existing entries and formatting where possible.
        /// </summary>
        private static bool TryMergeManifest(string content, out string result)
        {
            result = null;
            string trimmed = content.Trim();
            if (!trimmed.StartsWith("{") || !trimmed.EndsWith("}"))
                return false;

            // 1) Parse dependencies block
            if (!TryParseDependencies(trimmed, out var deps))
                return false;

            // 2) Parse existing scoped registries (optional)
            List<RegistryEntry> registries = ParseScopedRegistries(trimmed);

            // 3) Merge in our packages
            foreach (var kv in Packages)
            {
                if (!deps.ContainsKey(kv.Key))
                    deps[kv.Key] = kv.Value;
            }

            // 4) Merge in our registries (by URL to avoid duplicates)
            var existingUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in registries)
                existingUrls.Add(r.url?.TrimEnd('/') ?? "");

            foreach (var (name, url, scopes) in Registries)
            {
                string normalizedUrl = url.TrimEnd('/');
                if (existingUrls.Contains(normalizedUrl))
                    continue;
                existingUrls.Add(normalizedUrl);
                registries.Add(new RegistryEntry { name = name, url = url, scopes = scopes });
            }

            // 5) Build output with pretty-print
            result = BuildManifestJson(registries, deps);
            return true;
        }

        private static bool TryParseDependencies(string json, out Dictionary<string, string> deps)
        {
            deps = new Dictionary<string, string>();
            int depKeyIndex = json.IndexOf("\"dependencies\"", StringComparison.OrdinalIgnoreCase);
            if (depKeyIndex < 0)
                return false;

            int colonIndex = json.IndexOf(':', depKeyIndex);
            if (colonIndex < 0)
                return false;

            int openBrace = json.IndexOf('{', colonIndex);
            if (openBrace < 0)
                return false;

            int depth = 1;
            int i = openBrace + 1;
            while (i < json.Length && depth > 0)
            {
                char c = json[i];
                if (c == '{') depth++;
                else if (c == '}') depth--;
                i++;
            }

            if (depth != 0)
                return false;

            string block = json.Substring(openBrace, i - openBrace);

            // Match "key":"value" (value can be version or URL; we don't allow unescaped " in value)
            var regex = new Regex("\"([^\"]+)\"\\s*:\\s*\"([^\"]*)\"");
            foreach (Match m in regex.Matches(block))
            {
                string key = m.Groups[1].Value.Trim();
                string value = m.Groups[2].Value;
                if (!string.IsNullOrEmpty(key))
                    deps[key] = value;
            }

            return true;
        }

        private static List<RegistryEntry> ParseScopedRegistries(string json)
        {
            var list = new List<RegistryEntry>();
            int regKeyIndex = json.IndexOf("\"scopedRegistries\"", StringComparison.OrdinalIgnoreCase);
            if (regKeyIndex < 0)
                return list;

            int arrayStart = json.IndexOf('[', regKeyIndex);
            if (arrayStart < 0)
                return list;

            int depth = 1;
            int i = arrayStart + 1;
            while (i < json.Length && depth > 0)
            {
                char c = json[i];
                if (c == '[' || c == '{') depth++;
                else if (c == ']' || c == '}') depth--;
                i++;
            }

            if (depth != 0)
                return list;

            string arrayContent = json.Substring(arrayStart + 1, i - arrayStart - 2);

            // Parse each {"name":"...","url":"...","scopes":[...]}
            var nameRegex = new Regex("\"name\"\\s*:\\s*\"([^\"]*)\"");
            var urlRegex = new Regex("\"url\"\\s*:\\s*\"([^\"]*)\"");
            var scopesRegex = new Regex("\"scopes\"\\s*:\\s*\\[([^\\]]*)\\]");

            int objStart = 0;
            while (objStart < arrayContent.Length)
            {
                int nextObj = arrayContent.IndexOf("},", objStart, StringComparison.Ordinal);
                int end = nextObj < 0 ? arrayContent.Length : nextObj + 1;
                string obj = arrayContent.Substring(objStart, end - objStart).Trim().TrimEnd(',');
                objStart = end;

                if (string.IsNullOrWhiteSpace(obj))
                    continue;

                var nameMatch = nameRegex.Match(obj);
                var urlMatch = urlRegex.Match(obj);
                var scopesMatch = scopesRegex.Match(obj);
                if (!urlMatch.Success)
                    continue;

                var entry = new RegistryEntry
                {
                    name = nameMatch.Success ? nameMatch.Groups[1].Value : "",
                    url = urlMatch.Groups[1].Value,
                    scopes = Array.Empty<string>()
                };
                if (scopesMatch.Success)
                {
                    string scopeStr = scopesMatch.Groups[1].Value;
                    var scopeList = new List<string>();
                    foreach (Match sm in Regex.Matches(scopeStr, "\"([^\"]+)\""))
                        scopeList.Add(sm.Groups[1].Value);
                    entry.scopes = scopeList.ToArray();
                }
                list.Add(entry);
            }

            return list;
        }

        private static string BuildManifestJson(List<RegistryEntry> registries, Dictionary<string, string> deps)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");

            if (registries.Count > 0)
            {
                sb.Append("  \"scopedRegistries\": [\n");
                for (int i = 0; i < registries.Count; i++)
                {
                    var r = registries[i];
                    sb.Append("    {\n");
                    sb.Append("      \"name\": \"").Append(EscapeJson(r.name)).Append("\",\n");
                    sb.Append("      \"url\": \"").Append(EscapeJson(r.url)).Append("\",\n");
                    sb.Append("      \"scopes\": [");
                    for (int s = 0; s < r.scopes.Length; s++)
                    {
                        if (s > 0) sb.Append(", ");
                        sb.Append("\"").Append(EscapeJson(r.scopes[s])).Append("\"");
                    }
                    sb.Append("]\n");
                    sb.Append(i < registries.Count - 1 ? "    },\n" : "    }\n");
                }
                sb.Append("  ],\n");
            }

            sb.Append("  \"dependencies\": {\n");
            int index = 0;
            foreach (var kv in deps)
            {
                sb.Append("    \"").Append(EscapeJson(kv.Key)).Append("\": \"").Append(EscapeJson(kv.Value)).Append("\"");
                sb.Append(index < deps.Count - 1 ? ",\n" : "\n");
                index++;
            }
            sb.Append("  }\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        private struct RegistryEntry
        {
            public string name;
            public string url;
            public string[] scopes;
        }
    }
}
