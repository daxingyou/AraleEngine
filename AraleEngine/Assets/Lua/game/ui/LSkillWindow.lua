if not LSkillWindow then

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
LSkillWindow = M
createClass("LSkillWindow",LSkillWindow)
end