if not LSkillWindow then

local M =
{
}

function M:new(csobj)
	csobj.luaOnStart = function() self:Start(); end
end

function M:Start()
end
--========================
LSkillWindow = M
createClass("LSkillWindow",LSkillWindow)
end