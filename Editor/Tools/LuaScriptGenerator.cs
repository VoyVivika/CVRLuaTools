﻿#if UNITY_EDITOR && CVR_CCK_EXISTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ABI.CCK.Components;
using ABI.CCK.Components.ScriptableObjects;
using UnityEditor;

namespace NAK.LuaTools
{
    public static class LuaScriptGenerator
    {
        private const string LUA_ASSET_OUTPUT_PATH = "Assets/NotAKid/LuaTools.Generated/";

        #region Component Creation
        
        public static (string, string) CreateScriptTextFromWrapper(NAKLuaClientBehaviourWrapper wrapper)
        {
            var scriptPath = GenerateCleanScriptPath(wrapper.scriptName);
            var scriptText = GenerateScriptText(wrapper.boundEntries, wrapper.scriptEntries);
            
            wrapper.OutputScriptText = scriptText;
            //wrapper.scriptPath = scriptPath;
            return (scriptPath, scriptText);
        }

        public static void CreateLuaClientBehaviourFromWrapper(NAKLuaClientBehaviourWrapper wrapper)
        {
            (string scriptPath, string scriptText) = CreateScriptTextFromWrapper(wrapper);
            CVRLuaScript luaAsset = CreateLuaAsset(scriptPath, scriptText);

            //if (!wrapper.gameObject.TryGetComponent(out CVRLuaClientBehaviour luaClientBehaviour))
            CVRLuaClientBehaviour luaClientBehaviour = wrapper.gameObject.AddComponent<CVRLuaClientBehaviour>();

            luaClientBehaviour.enabled = wrapper.enabled;
            luaClientBehaviour.localOnly = wrapper.localOnly;
            luaClientBehaviour.asset = luaAsset;
            
            if (wrapper.boundEntries?.Length > 0)
                luaClientBehaviour.boundObjects = CreateBoundObjects(wrapper.boundEntries);

            return;
            static CVRBaseLuaBehaviour.BoundObject[] CreateBoundObjects(NAKLuaClientBehaviourWrapper.BoundItem[] boundEntries)
            {
                var boundObjects = new List<CVRBaseLuaBehaviour.BoundObject>();
                ProcessEntries(boundEntries);
                return boundObjects.ToArray();
                #region Bound Item Processing
                
                void ProcessEntries(NAKLuaClientBehaviourWrapper.BoundItem[] entries)
                {
                    foreach (var entry in entries)
                    {
                        if (!entry.IsValid) continue;
                        switch (entry.type)
                        {
                            case NAKLuaClientBehaviourWrapper.BoundItemType.Object:
                                boundObjects.Add(new CVRBaseLuaBehaviour.BoundObject
                                {
                                    name = entry.name,
                                    boundThing = entry.objectReference
                                });
                                break;
                            case NAKLuaClientBehaviourWrapper.BoundItemType.Table when entry.boundEntries != null:
                                ProcessEntries(entry.boundEntries);
                                break;
                        }
                    }
                }
                
                #endregion Bound Item Processing
            }
        }

        private static string GenerateCleanScriptPath(string scriptName)
        {
            var sanitizedScriptName = LuaScriptUtility.SanitizeFileName(scriptName);
            return Path.Combine(LUA_ASSET_OUTPUT_PATH, "generated." + sanitizedScriptName + ".lua");
        }

        private static CVRLuaScript CreateLuaAsset(string scriptPath, string scriptText)
        {
            LuaScriptUtility.EnsureDirectoryExists(scriptPath);
            File.WriteAllText(scriptPath, scriptText); // create a text file
            AssetDatabase.Refresh(); // lame, but needed to hit the scripted importer
            return AssetDatabase.LoadAssetAtPath<CVRLuaScript>(scriptPath); // load the asset
        }

        #endregion Component Creation

        #region Script Generator

        public static string GenerateScriptText(
            NAKLuaClientBehaviourWrapper.BoundItem[] boundEntries,
            NAKLuaClientBehaviourWrapper.ScriptEntry[] scriptEntries)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            
            List<string> scriptTexts = new List<string>();

            if (boundEntries != null)
            {
                // generate our bound entries script
                StringBuilder boundEntriesScript = new();
                if (boundEntries.Length > 0)
                {
                    boundEntriesScript.AppendLine("UnityEngine = require(\"UnityEngine\")"); // default require
                    boundEntriesScript.AppendLine("BoundEntries = {");
                    
                    foreach (NAKLuaClientBehaviourWrapper.BoundItem entry in boundEntries)
                        AppendBoundItem(boundEntriesScript, entry, 1);
                    
                    boundEntriesScript.AppendLine("}");
                    scriptTexts.Add(boundEntriesScript.ToString());
                }
                else
                {
                    // no bound entries, just create an empty table
                    boundEntriesScript.AppendLine("BoundEntries = {}");
                }
            }
            
            // add our script entries
            if (scriptEntries != null)
            {
                foreach (NAKLuaClientBehaviourWrapper.ScriptEntry entry in scriptEntries)
                {
                    if (!entry.IsValidAndActive) continue;
                    
                    string assetPath = AssetDatabase.GetAssetPath(entry.script);
                    string absolutePath = Path.GetFullPath(assetPath);
                    
                    scriptTexts.Add(File.ReadAllText(absolutePath));
                }
            } 

            // merge everything
            string mergedScript = LuaScriptMerger.MergeScripts(scriptTexts.ToArray());
            
            //stopwatch.Stop();
            //Debug.Log($"Merging scripts took {stopwatch.ElapsedMilliseconds:F2}ms");

            return mergedScript;
            
            #region Bound Item Appender
            
            void AppendBoundItem(StringBuilder script, NAKLuaClientBehaviourWrapper.BoundItem entry, int indentLevel)
            {
                if (!entry.IsValid) return;

                string boundName = LuaScriptUtility.PreventEscape(entry.name);
                string indent = new string(' ', indentLevel * 4);

                switch (entry.type)
                {
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Object:
                        script.AppendLine($"{indent}[\"{boundName}\"] = BoundObjects[\"{boundName}\"],");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Boolean:
                        script.AppendLine($"{indent}[\"{boundName}\"] = {entry.boolValue.ToString().ToLower()},");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Integer:
                        script.AppendLine($"{indent}[\"{boundName}\"] = {entry.intValue},");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Float:
                        script.AppendLine($"{indent}[\"{boundName}\"] = {entry.floatValue.ToString(CultureInfo.InvariantCulture)},");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.String:
                        script.AppendLine($"{indent}[\"{boundName}\"] = \"{LuaScriptUtility.PreventEscape(entry.stringValue)}\",");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Color:
                        script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewColor({entry.colorValue.r}, {entry.colorValue.g}, {entry.colorValue.b}, {entry.colorValue.a}),");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Vector2:
                        script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewVector2({entry.vector2Value.x}, {entry.vector2Value.y}),");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Vector3:
                        script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewVector3({entry.vector3Value.x}, {entry.vector3Value.y}, {entry.vector3Value.z}),");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Rect:
                        script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewRect({entry.rectValue.x}, {entry.rectValue.y}, {entry.rectValue.width}, {entry.rectValue.height}),");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Bounds:
                        script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewBounds(UnityEngine.NewVector3({entry.boundsValue.center.x}, {entry.boundsValue.center.y}, {entry.boundsValue.center.z}), UnityEngine.NewVector3({entry.boundsValue.size.x}, {entry.boundsValue.size.y}, {entry.boundsValue.size.z})),");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Quaternion:
                        script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewQuaternion({entry.quaternionValue.x}, {entry.quaternionValue.y}, {entry.quaternionValue.z}, {entry.quaternionValue.w}),");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Vector2Int:
                        script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewVector2Int({entry.vector2IntValue.x}, {entry.vector2IntValue.y}),");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Vector3Int:
                        script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewVector3Int({entry.vector3IntValue.x}, {entry.vector3IntValue.y}, {entry.vector3IntValue.z}),");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.RectInt:
                        script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewRectInt({entry.rectIntValue.x}, {entry.rectIntValue.y}, {entry.rectIntValue.width}, {entry.rectIntValue.height}),");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.BoundsInt:
                        script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewBoundsInt(UnityEngine.NewVector3Int({entry.boundsIntValue.position.x}, {entry.boundsIntValue.position.y}, {entry.boundsIntValue.position.z}), UnityEngine.NewVector3Int({entry.boundsIntValue.size.x}, {entry.boundsIntValue.size.y}, {entry.boundsIntValue.size.z})),");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Vector4:
                        script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewVector4({entry.vector4Value.x}, {entry.vector4Value.y}, {entry.vector4Value.z}, {entry.vector4Value.w}),");
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.LayerMask:
                        //script.AppendLine($"{indent}[\"{boundName}\"] = UnityEngine.NewLayerMask({entry.layerMaskValue.value}),");
                        //Debug.LogWarning("LayerMask constructor is not yet exposed to LUA!");
                        script.AppendLine($"{indent}[\"{boundName}\"] = {entry.layerMaskValue.value},"); // int value works fine
                        break;
                    case NAKLuaClientBehaviourWrapper.BoundItemType.Table:
                        script.AppendLine($"{indent}[\"{boundName}\"] = {{");
                        foreach (var subEntry in entry.boundEntries)
                        {
                            AppendBoundItem(script, subEntry, indentLevel + 1);
                        }
                        script.AppendLine($"{indent}}},");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            #endregion Bound Item Appender
        }

        #endregion Script Generator

    }
}
#endif