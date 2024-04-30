using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Watermelon
{
    public static class DefineManager
    {
        public static bool HasDefine(string define)
        {
            string definesLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            return Array.FindIndex(definesLine.Split(';'), x => x == define) != -1;
        }

        public static void EnableDefine(string define)
        {
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            if (Array.FindIndex(defineLine.Split(';'), x => x == define) != -1)
            {
                return;
            }

            defineLine = defineLine.Insert(0, define + ";");

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), defineLine);
        }

        public static void DisableDefine(string define)
        {
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            string[] splitedDefines = defineLine.Split(';');

            int tempDefineIndex = Array.FindIndex(splitedDefines, x => x == define);
            string tempDefineLine = "";
            if (tempDefineIndex != -1)
            {
                for (int i = 0; i < splitedDefines.Length; i++)
                {
                    if (i != tempDefineIndex)
                    {
                        defineLine = defineLine.Insert(0, splitedDefines[i]);
                    }
                }
            }

            if (defineLine != tempDefineLine)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), tempDefineLine);
            }
        }

        public static void CheckAutoDefines()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<DefineState> markedDefines = new List<DefineState>();
            RegisteredDefine[] registeredDefines = DefinesSettings.REGISTERED_DEFINES;
            for (int i = 0; i < registeredDefines.Length; i++)
            {
                RegisteredDefine registeredDefine = registeredDefines[i];

                foreach (Assembly assembly in assemblies)
                {
                    if (assembly != null)
                    {
                        Type targetType = assembly.GetType(registeredDefine.AssemblyType, false);
                        if (targetType != null)
                        {
                            markedDefines.Add(new DefineState(registeredDefine.Define, true));

                            break;
                        }
                    }
                }
            }

            ChangeAutoDefinesState(markedDefines);
        }

        public static void ChangeAutoDefinesState(List<DefineState> defineStates)
        {
            if (defineStates.IsNullOrEmpty())
                return;

            DefinesString definesString = new DefinesString();
            foreach (DefineState defineState in defineStates)
            {
                if (defineState.State)
                {
                    if (!definesString.HasDefine(defineState.Define))
                    {
                        definesString.AddDefine(defineState.Define);
                    }
                }
                else
                {
                    if (definesString.HasDefine(defineState.Define))
                    {
                        definesString.RemoveDefine(defineState.Define);
                    }
                }
            }

            definesString.ApplyDefines();
        }
    }
}

// -----------------
// Define Manager v0.3
// -----------------

// Changelog
// v 0.3
// • Added auto toggle for specific defines
// • UI moved from scriptable object editor to editor window
// v 0.2.1
// • Added link to the documentation
// • Enable define function fix
// v 0.1
// • Added basic version