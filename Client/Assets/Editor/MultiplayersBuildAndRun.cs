using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MultiplayersBuildAndRun
{
    [MenuItem("Tools/Run Multiplayer/2 Players")]
    static void PerformWin64Build2()
    {
        PerformWin64Build(2);
    }
    
    [MenuItem("Tools/Run Multiplayer/3 Players")]
    static void PerformWin64Build3()
    {
        PerformWin64Build(3);
    }
    
    [MenuItem("Tools/Run Multiplayer/4 Players")]
    static void PerformWin64Build4()
    {
        PerformWin64Build(4);
    }

    static void PerformWin64Build(int playerCount)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
        for (int i = 0; i < playerCount; i++)
        {
            string buildPath = "Builds/Win64/" + GetProjectName() + i.ToString();
            string buildFile = GetProjectName() + i.ToString() + ".exe";
            
            BuildPipeline.BuildPlayer(GetScenePaths(), buildPath + "/" + buildFile,
                BuildTarget.StandaloneWindows64, BuildOptions.AutoRunPlayer);
        }
    }

    static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }

        return scenes;
    }
}
