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
        GameObject[] gamObjects = Resources.LoadAll<GameObject>("Prefabs/Map");
        foreach (var go in gamObjects)
        {
            Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);
            
            using (var writer = File.CreateText($"Assets/Resources/Map/{go.name}.text"))
            {
                writer.WriteLine(tm.cellBounds.xMin);
                writer.WriteLine(tm.cellBounds.xMax);
                writer.WriteLine(tm.cellBounds.yMin);
                writer.WriteLine(tm.cellBounds.yMax);

                for (int y = tm.cellBounds.yMax; y >= tm.cellBounds.yMin; y--)
                {
                    for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
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

