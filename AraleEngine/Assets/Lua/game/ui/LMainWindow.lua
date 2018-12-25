if not LMainWindow then

local M =
{
}

function M:new(csobj)
	csobj.luaOnStart = function() self:Start(); end
end

function M:Start()
	EventListener.Get(self.luaPlayer):AddOnClick(function(evt)  WindowMgr.single:GetWindow("PlayerWindow", true) end)
	EventListener.Get(self.luaTask):AddOnClick(function(evt)  WindowMgr.single:GetWindow("TaskWindow", true) end)
	EventListener.Get(self.luaBag):AddOnClick(function(evt)  WindowMgr.single:GetWindow("BagWindow", true) end)
	EventListener.Get(self.luaShop):AddOnClick(function(evt)  WindowMgr.single:GetWindow("ShopWindow", true) end)
	EventListener.Get(self.luaForgin):AddOnClick(function(evt)  WindowMgr.single:GetWindow("ForginWindow", true) end)
	EventListener.Get(self.luaSkill):AddOnClick(function(evt)  WindowMgr.single:GetWindow("SkillWindow", true) end)
end
--========================
LMainWindow = M
createClass("LMainWindow",LMainWindow)
end