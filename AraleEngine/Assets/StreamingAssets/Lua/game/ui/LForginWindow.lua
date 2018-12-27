if not LForginWindow then

local M =
{
}

function M:new(csobj)
	csobj.luaOnStart = function() self:Start(); end
end

function M:Start()
end
--========================
LForginWindow = M
createClass("LForginWindow",LForginWindow)
end