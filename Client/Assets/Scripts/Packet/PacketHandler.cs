﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
		
		Managers.Obj.Add(enterGamePacket.Player, true);
	}
	
	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
		
		Managers.Obj.RemoveMyPlayer();
	}
	
	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
		
		foreach (ObjectInfo obj in spawnPacket.Objects)
			Managers.Obj.Add(obj, false);
	}
	
	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
		
		foreach (int id in despawnPacket.ObjectIds)
			Managers.Obj.Remove(id);
	}
	
	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;

		GameObject go = Managers.Obj.FindById(movePacket.ObjectId);
		if(go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if(cc == null)
			return;

		cc.PosInfo = movePacket.PosInfo;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;
		
		GameObject go = Managers.Obj.FindById(skillPacket.ObjectId);
		if(go == null)
			return;

		PlayerController pc = go.GetComponent<PlayerController>();
		if (pc != null)
		{
			pc.UseSkill(skillPacket.Info.SkillId);
		}
	}
	
	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeHp changePacket = packet as S_ChangeHp;
		
		GameObject go = Managers.Obj.FindById(changePacket.ObjectId);
		if(go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null)
		{
			cc.Stat.Hp = changePacket.Hp;
			// UI 갱신
			Debug.Log($"Change HP : {changePacket.Hp}");
		}
	}
}

