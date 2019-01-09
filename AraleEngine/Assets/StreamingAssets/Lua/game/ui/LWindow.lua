if not LWindow then
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

--=======================
LWindow = M
createClass("LWindow",LWindow);
end