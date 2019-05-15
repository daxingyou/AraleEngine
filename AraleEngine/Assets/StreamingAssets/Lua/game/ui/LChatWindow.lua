if LChatWindow then print("same lua, reload ignore!!!") end

local M =
{
}

function M:new(csobj)
	csobj.luaOnStart = self.Start
end

function M:Start()
end
--========================
LChatWindow = M
createClass("LChatWindow",LChatWindow)