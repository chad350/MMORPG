using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestCollision : MonoBehaviour
{
    public Tilemap _tilemap;
    public TileBase _tilebase;
    
    // Start is called before the first frame update
    void Start()
    {
        _tilemap = GetComponent<Tilemap>();
        
        _tilemap.SetTile(new Vector3Int(0,0,0), _tilebase);
        
        
        
        List<Vector3Int> blocked = new List<Vector3Int>();

        foreach (var pos in _tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile =  _tilemap.GetTile(pos);
            if(tile != null)
                blocked.Add(pos);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
