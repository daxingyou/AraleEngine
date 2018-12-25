if not LStartWindow then

local M =
{
}

function M:new(csobj)
	csobj.luaOnStart = function() self:Start(); end
end

function M:Start()
	self.luaVersion:GetComponent("Text").text = "v1.0.0 r10"
	EventListener.Get(self.luaStart):AddOnClick(function(evt)  WindowMgr.single:GetWindow("LoginWindow", true) end)
end
--========================
LStartWindow = M
createClass("LStartWindow",LStartWindow)
end