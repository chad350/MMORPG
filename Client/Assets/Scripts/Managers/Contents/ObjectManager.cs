﻿using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    public static GameObjectType GetObjectTypeById(int Id)
    {
        int type = (Id >> 24) & 0x7F;
        return (GameObjectType) type;
    }

    public void Add(ObjectInfo info, bool isMyPlayer = false)
    {
        GameObjectType objectType = GetObjectTypeById(info.ObjectId);
        
        if (objectType == GameObjectType.Player)
        {
            if (isMyPlayer == true)
            {
                GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                MyPlayer = go.GetComponent<MyPlayerController>();
                MyPlayer.Id = info.ObjectId;
                MyPlayer.PosInfo = info.PosInfo;
                MyPlayer.Stat = info.StatInfo;
                MyPlayer.SyncPos();
            }
            else
            {
                GameObject go = Managers.Resource.Instantiate("Creature/Player");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                PlayerController pc = go.GetComponent<PlayerController>();
                pc.Id = info.ObjectId;
                pc.PosInfo = info.PosInfo;
                pc.Stat = info.StatInfo;
                pc.SyncPos();
            }
        }
        else if (objectType == GameObjectType.Monster)
        {
        }
        else if (objectType == GameObjectType.Projectile)
        {
            GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
            go.name = "Arrow";
            _objects.Add(info.ObjectId, go);

            ArrowController ac = go.GetComponent<ArrowController>();
            ac.PosInfo = info.PosInfo;
            ac.Stat = info.StatInfo;
            ac.SyncPos();
        }

    }

    
    public void Remove(int id)
    {
        GameObject go = FindById(id);
        if(go == null)
            return;
        
        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public void RemoveMyPlayer()
    {
        if(MyPlayer == null)
            return;
        
        Remove(MyPlayer.Id);
        MyPlayer = null;
    }

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public GameObject Find(Vector3Int cellPos)
    {
        foreach (var obj in _objects.Values)
        {
            CreatureController cc = obj.GetComponent<CreatureController>();
            if(cc == null)
                continue;

            if (cc.CellPos == cellPos)
                return obj;
        }

        return null;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (var obj in _objects.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }

        return null;
    }
    
    public void Clear()
    {
        foreach (var obj in _objects.Values)
            Managers.Resource.Destroy(obj);
        _objects.Clear();
    }
}
