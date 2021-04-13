using Google.Protobuf;
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
		
		Managers.Obj.Clear();
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
		
		if(Managers.Obj.MyPlayer.Id == movePacket.ObjectId)
			return;

		BaseController bc = go.GetComponent<BaseController>();
		if(bc == null)
			return;

		bc.PosInfo = movePacket.PosInfo;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;
		
		GameObject go = Managers.Obj.FindById(skillPacket.ObjectId);
		if(go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null)
		{
			cc.UseSkill(skillPacket.Info.SkillId);
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
			cc.Hp = changePacket.Hp;
		}
	}
	
	public static void S_DieHandler(PacketSession session, IMessage packet)
	{
		S_Die diePacket = packet as S_Die;
		
		GameObject go = Managers.Obj.FindById(diePacket.ObjectId);
		if(go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null)
		{
			cc.Hp = 0;
			cc.OnDead();
		}
	}
	
	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
		Debug.Log("S_ConnectedHandler");
		C_Login loginPacket = new  C_Login();
		
		// 시스템에 따라 얻을수 있는 유니크 키
		// 나중에 같은 기기에서 여러 계정을 사용하면 문제가 될 수 있다.
		loginPacket.UniqueId = SystemInfo.deviceUniqueIdentifier;
		
		Managers.Network.Send(loginPacket);
	}
	
	// 로그인 OK + 캐릭터 목록
	public static void S_LoginHandler(PacketSession session, IMessage packet)
	{
		S_Login LoginPacket = packet as S_Login;
		Debug.Log($"LoginOk : {LoginPacket.LoginOk}");
		
		// 로비 UI - 캐릭터 보여주고 선택

		if (LoginPacket.Players == null || LoginPacket.Players.Count == 0)
		{
			C_CreatePlayer createPacket = new C_CreatePlayer();
			createPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
			Managers.Network.Send(createPacket);
		}
		else
		{
			// 첫번째 캐릭터 로그인
			LobbyPlayerInfo info = LoginPacket.Players[0];
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = info.Name;
			Managers.Network.Send(enterGamePacket);
		}
	}
	
	public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
	{
		S_CreatePlayer createOkPacket = packet as S_CreatePlayer;
		if (createOkPacket.Player == null)
		{
			C_CreatePlayer createPacket = new C_CreatePlayer();
			createPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
			Managers.Network.Send(createPacket);
		}
		else
		{
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = createOkPacket.Player.Name;
			Managers.Network.Send(enterGamePacket);
		}
	}
	
	public static void S_ItemListHandler(PacketSession session, IMessage packet)
	{
		S_ItemList itemListPacket = packet as S_ItemList;
		
		Managers.Inven.Clear();
		
		// 메모리에 아이템 정보 적용
		foreach (ItemInfo itemInfo in itemListPacket.Items)
		{
			Item item = Item.MakeItem(itemInfo);
			Managers.Inven.Add(item);
		}

		// UI 에서 표시
		// UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		// UI_Inventory invenUI = gameSceneUI.InvenUI;
		//
		// invenUI.gameObject.SetActive(true);
		// invenUI.RefreshUI();
	}
	
	public static void S_AddItemHandler(PacketSession session, IMessage packet)
	{
		S_AddItem itemListPacket = packet as S_AddItem;

		// 메모리에 아이템 정보 적용
		foreach (ItemInfo itemInfo in itemListPacket.Items)
		{
			Item item = Item.MakeItem(itemInfo);
			Managers.Inven.Add(item);
		}

		// UI 에서 표시
		// UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		// UI_Inventory invenUI = gameSceneUI.InvenUI;
		//
		// invenUI.gameObject.SetActive(true);
		// invenUI.RefreshUI();
		
		Debug.Log("아이템을 획득했습니다.");
	}
}

