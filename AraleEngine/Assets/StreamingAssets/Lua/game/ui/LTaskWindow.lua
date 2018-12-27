if not LTaskWindow then

local M =
{
}

function M:new(csobj)
	csobj.luaOnStart = function() self:Start(); end
end

function M:Start()
	local ls = UISwitch.getGroupSwitch("task1")
	for i=0,ls.Count do
		
	end
	ls[0].isOn = true
end
--========================
LTaskWindow = M
createClass("LTaskWindow",LTaskWindow)
end