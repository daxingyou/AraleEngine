--====================================================
--don't modify the code in this block
--don't modify the lua file in common,permit only System developer
--main.lua is lua code entry
print(package.path)
require "common/LClass"
require "common/LObject"
require "common/LProtoWrite"
--=====================your code begin=================
--==============工具库
--local util = require 'lib/xlua/util'--不完整版，无lua导出delegate的接口
Json = require 'rapidjson'
Proto= require 'common/protoc'
PB   = require 'pb'
Lpeg = require 'lpeg'
--==============unity
CU = CS.UnityEngine
UI = CU.UI
GameObject = CS.UnityEngine.GameObject
Matrix4x4 = CS.UnityEngine.Matrix4x4
Quaternion = CS.UnityEngine.Quaternion
Vector3 = CS.UnityEngine.Vector3
Vector2 = CS.UnityEngine.Vector2
Time = CS.UnityEngine.Time
Rigidbody = CS.UnityEngine.Rigidbody
NavMeshAgent = CS.UnityEngine.NavMeshAgent
Collider = CS.UnityEngine.Collider
WaitForSeconds = CS.UnityEngine.WaitForSeconds
--==============DoTween
DOTween = CS.DG.Tweening.DOTween
--==============引擎
ResLoad  = CS.Arale.Engine.ResLoad
AssetRef = CS.Arale.Engine.AssetRef
GRoot = CS.Arale.Engine.GRoot
WindowMgr = CS.Arale.Engine.WindowMgr
CameraMgr = CS.Arale.Engine.CameraMgr
EventListener = CS.Arale.Engine.EventListener
SceneMgr = CS.Arale.Engine.SceneMgr
EventMgr = CS.Arale.Engine.EventMgr
TimeMgr = CS.Arale.Engine.TimeMgr
RTime   = CS.Arale.Engine.RTime
TableMgr = CS.Arale.Engine.TableMgr
NetworkMgr = CS.Arale.Engine.NetworkMgr
NetMgr = CS.NetMgr
Window = CS.Arale.Engine.Window
Log = CS.Arale.Engine.Log
GHelper = CS.Arale.Engine.GHelper
HeadInfo = CS.Arale.Engine.HeadInfo
UISwitch = CS.UISwitch
LuaHelp    = CS.LuaHelp
UnitState = CS.UnitState
UnitType = CS.UnitType
Unit = CS.Unit
Buff = CS.Buff
Bag  = CS.Bag
GameArea = CS.GameArea
UIItemSlot = CS.UIItemSlot
PlayerHeader = CS.PlayerHeader
UISListItem = CS.UISListItem
UIImageText = CS.UIImageText
--===============配表
TBSkill = CS.Arale.Engine.TBSkill
TBMonster = CS.Arale.Engine.TBMonster
TBPlayer = CS.Arale.Engine.TBPlayer
TBItem = CS.Arale.Engine.TBItem
--===============协议
ProtoWriter = CS.ProtoBuf.ProtoWriter
ProtoReader = CS.ProtoBuf.ProtoReader
WireType = CS.ProtoBuf.WireType
MsgReqCreateHero = CS.MsgReqCreateHero
MsgReqEnterBattle = CS.MsgReqEnterBattle
MsgItem = CS.MsgItem
--通过Debug.Log(typeof(List<object>)获取模板类的真实名称
--==============
require "LuaEnum"
require "game/data/LClient"
require "game/ui/LWindow"
require "game/ui/LStartWindow"
require "game/ui/LLoginWindow"
require "game/ui/LLanLoginWindow"
require "game/ui/LMainWindow"
require "game/ui/LNoticeWindow"
require "game/ui/LMessageWindow"
require "game/ui/LPlayerWindow"
require "game/ui/LTaskWindow"
require "game/ui/LSkillWindow"
require "game/ui/skill/LSkillItem"
require "game/ui/LShopWindow"
require "game/ui/LMailWindow"
require "game/ui/LChatWindow"
require "game/ui/mail/LMailItem"
require "game/ui/shop/LShopItem"
require "game/ui/LForginWindow"
require "game/ui/LBagWindow"
require "game/ui/bag/LItemSlot"
require "game/ui/LRoleCreateWindow"
require "game/ui/LRoleCreateWindowItem"
require "game/battle/LBattleSceneCtrl"
require "game/ui/main/LSKillButton"
require "game/ui/main/LHeadInfo"
require "game/ui/LRewardWindow"
require "game/ai/LHero"
require "game/ai/LMonster"
BuffParam={}
require "game/buff/LBornBuff"
require "game/buff/LChenMo"
require "game/buff/LDingShen"
require "game/buff/LHuiXue"
require "game/buff/LJianSu"
require "game/buff/LJianXue"
require "game/buff/LJinGang"
require "game/buff/LJingHua"
require "game/buff/LJinGu"
require "game/buff/LSkillHarm"
require "game/buff/LLeiGongBaoFeng"
print("require all ok");
--====================
require "pb/Proto.test"
--=====================================================
function  main( ... )
	if GRoot.single.mLaunchFlag~=0 then return end
	WindowMgr.SetWindowRes ("NoticeWindow", "UI/NoticeWindow")
	WindowMgr.SetWindowRes ("MessageWindow", "UI/MessageWindow")
	WindowMgr.SetWindowRes ("StartWindow", "UI/StartWindow")
	WindowMgr.SetWindowRes ("UpdateWindow", "UI/UpdateWindow")
	WindowMgr.SetWindowRes ("LoginWindow", "UI/LoginWindow")
	WindowMgr.SetWindowRes ("LanLoginWindow", "UI/LanLoginWindow")
	WindowMgr.SetWindowRes ("MainWindow", "UI/MainWindow")
	WindowMgr.SetWindowRes ("ShopWindow", "UI/ShopWindow")
	WindowMgr.SetWindowRes ("SkillWindow", "UI/SkillWindow")
	WindowMgr.SetWindowRes ("TaskWindow", "UI/TaskWindow")
	WindowMgr.SetWindowRes ("PlayerWindow", "UI/PlayerWindow")
	WindowMgr.SetWindowRes ("ForginWindow", "UI/ForginWindow")
	WindowMgr.SetWindowRes ("ImbedWindow", "UI/ImbedWindow")
	WindowMgr.SetWindowRes ("BagWindow", "UI/BagWindow")
	WindowMgr.SetWindowRes ("RoleCreateWindow", "UI/RoleCreateWindow")
	WindowMgr.SetWindowRes ("MailWindow", "UI/MailWindow")
	WindowMgr.SetWindowRes ("ChatWindow", "UI/ChatWindow")
	WindowMgr.SetWindowRes ("RewardWindow", "UI/RewardWindow")
	WindowMgr.single:GetWindow("NoticeWindow", true)

	TableMgr.TestModel = true
	load(TableMgr.single:GenLuaExtend(typeof(TBPlayer)))();
	load(TableMgr.single:GenLuaExtend(typeof(TBMonster)))();
	load(TableMgr.single:GenLuaExtend(typeof(TBItem)))();
	print(LTBItem)

	EventMgr.single:AddListener(GRoot.EventSceneLoad, onSceneLoaded)
	EventMgr.single:AddListener("Game.Login", onLogin)
	EventMgr.single:AddListener("Game.Logout", onLogout)
	
	SceneMgr.single:LoadScene("Login")
	print("main.lua ok");
end

function onSceneLoaded(evt)
	local levelName = evt.data
	print("scene:"..levelName);
	if levelName == "Login" then
		WindowMgr.single:GetWindow("StartWindow", true)
	elseif levelName == "Main" then
		WindowMgr.single:GetWindow("MainWindow", true)
	end
end

function onLogin(evt)
	LClient:Login()
	SceneMgr.single:LoadScene("Main")
end

function onLogout(evt)
	LClient:Logout()
	NetMgr.single:Deinit();
	SceneMgr.single:LoadScene("Login")
end
--====================================================
