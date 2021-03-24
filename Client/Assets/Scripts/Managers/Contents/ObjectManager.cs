using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    // Start is called before the first frame update
    List<GameObject> _object = new List<GameObject>();

    public void Add(GameObject go)
    {
        _object.Add(go);
    }
    
    public void Remove(GameObject go)
    {
        _object.Remove(go);
    }

    public GameObject Find(Vector3Int cellPos)
    {
        foreach (var obj in _object)
        {
            CreatureController cc = obj.GetComponent<CreatureController>();
            if(cc == null)
                continue;

            if (cc.CellPos == cellPos)
                return obj;
        }

        return null;
    }

    public void Clear()
    {
        _object.Clear();
    }
}
