if not LStartWindow then

local M =
{
}

function M:new(csobj)
	csobj.luaOnStart = self.Start
end

function M:Start()
	self.luaVersion:GetComponent(typeof(UI.Text)).text = "v1.0.0 r10"
	EventListener.Get(self.luaStart):AddOnClick(function(evt)  WindowMgr.single:GetWindow("LanLoginWindow", true) end)
end
--========================
LStartWindow = M
createClass("LStartWindow",LStartWindow)
end