#if UNITY_EDITOR && CVR_CCK_EXISTS
using ABI.CCKEditor.ContentBuilder;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace NAK.LuaTools
{
    [UsedImplicitly]
    public class LuaClientBuildProcessor : CCKBuildProcessor
    {
        public override void OnPreProcessAvatar(GameObject avatar) => ProcessWrappers();
        public override void OnPreProcessSpawnable(GameObject prop) => ProcessWrappers();
        public override void OnPreProcessWorld(GameObject world) => ProcessWrappers();
        private static void ProcessWrappers()
        {
            var wrappers = GetAllComponents<NAKLuaClientBehaviourWrapper>();
            foreach (var wrapper in wrappers)
            {
                LuaScriptGenerator.CreateLuaClientBehaviourFromWrapper(wrapper);
                Object.DestroyImmediate(wrapper);
            }
        }
    }
}
#endif