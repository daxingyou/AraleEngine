if not LLoginWindow then print("same lua, reload ignore!!!") end

local M =
{
}

function M:new(csobj)
	csobj.luaOnStart = function() self:Start(); end
end

function M:Start()
	EventListener.Get(self.luaYes):AddOnClick(function(evt)  self:OnYes() end)
end

function M:OnYes()
	local account = self.luaAccount:GetComponent("InputField").text
	local pw = self.luaPassword:GetComponent("InputField").text
	print("account="..account..",pw="..pw)
end
--========================
LLoginWindow = M
createClass("LLoginWindow",LLoginWindow)