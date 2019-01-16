if not LImbedWindow then print("same lua, reload ignore!!!") end

local M =
{
}

function M:new(csobj)
	csobj.luaOnStart = function() self:Start(); end
end

function M:Start()
	print("Start:"..self.test..self.Cube.name);
end
--========================
LImbedWindow = M
createClass("LImbedWindow",LImbedWindow)