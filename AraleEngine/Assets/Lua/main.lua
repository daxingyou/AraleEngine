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
UISwitch = luanet.import_type("UISwitch")
require "game/ui/LStartWindow"
require "game/ui/LLoginWindow"
require "game/ui/LMainWindow"
require "game/ui/LPlayerWindow"
require "game/ui/LTaskWindow"
require "game/ui/LSkillWindow"
require "game/ui/LShopWindow"
require "game/ui/LForginWindow"
require "game/ui/shop/LShopItem"
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
