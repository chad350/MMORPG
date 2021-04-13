using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Skill> SkillDict { get; private set; } = new Dictionary<int, Skill>();
    public Dictionary<int, ItemData> ItemDict { get; private set; } = new Dictionary<int, ItemData>();

    public void Init()
    {
        SkillDict = LoadJson<SkillData, int, Skill>("SkillData").MakeDict();
        ItemDict = LoadJson<ItemLoader, int, ItemData>("ItemData").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
		TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonUtility.FromJson<Loader>(textAsset.text);
	}
}
