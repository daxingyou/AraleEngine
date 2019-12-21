if LWindow then print("same lua, reload ignore!!!") end
--======================
local M=
{
}

function M:new(cs)
end

function M:OnWindowMessage(metho,param)
	local f = self[metho]
	if f==nil then return end
	f(self, param)
end

function M:OnWindowEvent(windEvent)
	local f = self[metho]
	if f==nil then return end
	f(self)
end

function M:OnEvent(evt,param)
	local f = self[evt]
	if f==nil then return false end
	f(self, param)
	return true
end
--=======================
LWindow = M
createClass("LWindow",LWindow);