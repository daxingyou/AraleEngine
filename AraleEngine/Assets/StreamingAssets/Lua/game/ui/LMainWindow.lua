if not LMainWindow then

local M =
{
	_hero;
}

function M:new(csobj)
	csobj.luaOnStart = self.Start
end

function M:Start()
	EventListener.Get(self.luaPlayer):AddOnClick(function(evt)  WindowMgr.single:GetWindow("PlayerWindow", true) end)
	EventListener.Get(self.luaTask):AddOnClick(function(evt)  WindowMgr.single:GetWindow("TaskWindow", true) end)
	EventListener.Get(self.luaBag):AddOnClick(function(evt)  WindowMgr.single:GetWindow("BagWindow", true) end)
	EventListener.Get(self.luaShop):AddOnClick(function(evt)  WindowMgr.single:GetWindow("ShopWindow", true) end)
	EventListener.Get(self.luaForgin):AddOnClick(function(evt)  WindowMgr.single:GetWindow("ForginWindow", true) end)
	EventListener.Get(self.luaSkill):AddOnClick(function(evt)  WindowMgr.single:GetWindow("SkillWindow", true) end)

	EventListener.Get(self.luaHero):AddOnClick(function(evt)  WindowMgr.single:GetWindow("RoleCreateWindow", true) end)
	EventListener.Get(self.luaLogout):AddOnClick(function(evt)  EventMgr.single:SendEvent("Game.Logout") end)

	if _hero == nil then
		WindowMgr.single:GetWindow("RoleCreateWindow", true)
	end
end
--========================
LMainWindow = M
createClass("LMainWindow",LMainWindow)
end