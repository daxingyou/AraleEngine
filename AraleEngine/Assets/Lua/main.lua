--====================================================
--don't modify the code in this block
--don't modify the lua file in common,permit only System developer
--main.lua is lua code entry
print(package.path)
require "common/LClass"
require "common/LObject"
--=====================your code begin=================
--WHelper = luanet.import_type("WHelper");
--local a = newLuaObject("LPrefabItem");
GRoot = luanet.import_type("Arale.Engine.GRoot")
WindowMgr = luanet.import_type("Arale.Engine.WindowMgr")
EventListener = luanet.import_type("Arale.Engine.EventListener")
SceneMgr = luanet.import_type("Arale.Engine.SceneMgr")
EventMgr = luanet.import_type("Arale.Engine.EventMgr")
AssetRef = luanet.import_type("Arale.Engine.AssetRef")
UISwitch = luanet.import_type("UISwitch")
LHelp    = luanet.import_type("LHelp")
--通过Debug.Log(typeof(List<object>)模板类的真实名称
List_object= luanet.import_type("System.Collections.Generic.List`1[System.Object]")
require "game/ui/LStartWindow"
require "game/ui/LLoginWindow"
require "game/ui/LMainWindow"
require "game/ui/LPlayerWindow"
require "game/ui/LTaskWindow"
require "game/ui/LSkillWindow"
require "game/ui/LShopWindow"
require "game/ui/shop/LShopItem"
require "game/ui/LForginWindow"
require "game/ui/LBagWindow"
require "game/ui/bag/LBag"
require "game/ui/bag/LBagItem"
require "game/ui/bag/LItemSlot"
require "game/ui/LRoleCreateWindow"
require "game/ui/LRoleCreateWindowItem"

print("require all ok");
--=====================================================
function  main( ... )
	WindowMgr.SetWindowRes ("StartWindow", "UI/StartWindow")
	WindowMgr.SetWindowRes ("UpdateWindow", "UI/UpdateWindow")
	WindowMgr.SetWindowRes ("LoginWindow", "UI/LoginWindow")
	WindowMgr.SetWindowRes ("MainWindow", "UI/MainWindow")
	WindowMgr.SetWindowRes ("ShopWindow", "UI/ShopWindow")
	WindowMgr.SetWindowRes ("SkillWindow", "UI/SkillWindow")
	WindowMgr.SetWindowRes ("TaskWindow", "UI/TaskWindow")
	WindowMgr.SetWindowRes ("PlayerWindow", "UI/PlayerWindow")
	WindowMgr.SetWindowRes ("ForginWindow", "UI/ForginWindow")
	WindowMgr.SetWindowRes ("ImbedWindow", "UI/ImbedWindow")
	WindowMgr.SetWindowRes ("BagWindow", "UI/BagWindow")
	WindowMgr.SetWindowRes ("RoleCreateWindow", "UI/RoleCreateWindow")

	LBag:Init()

	EventMgr.single:AddListener(GRoot.EventSceneLoad, onSceneLoaded)
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
--====================================================
