using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine.Tilemaps;

#endif
public class MapEditor 
{
    
#if UNITY_EDITOR

    // 단축키 
    // % (Ctrl), # (Shift), & (Alt)
    // 메뉴 경로 설정
    [MenuItem("Tools/GenerateMap %#g")]  
    private static void GenerateMap()
    {
        GenerateByBath("Assets/Resources/Map");
        GenerateByBath("../Common/MapData");
    }

    private static void GenerateByBath(string pathPrefix)
    {
        GameObject[] gamObjects = Resources.LoadAll<GameObject>("Prefabs/Map");
        foreach (var go in gamObjects)
        {
            Tilemap tmBase = Util.FindChild<Tilemap>(go, "Tilemap_Base", true);
            Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);
            
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}.txt"))
            {
                writer.WriteLine(tmBase.cellBounds.xMin);
                writer.WriteLine(tmBase.cellBounds.xMax);
                writer.WriteLine(tmBase.cellBounds.yMin);
                writer.WriteLine(tmBase.cellBounds.yMax);

                for (int y = tmBase.cellBounds.yMax; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if(tile != null)
                            writer.Write("1");
                        else
                            writer.Write("0");
                    }
                    writer.WriteLine();
                }
            }
        }
    }
#endif
    
}

