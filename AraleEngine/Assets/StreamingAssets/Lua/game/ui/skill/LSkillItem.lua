if not LSkillItem then
--======================
local M=
{
}
function M:new(cs)
	cs.luaOnAwake = self.Awake
end

function M:Awake()
	self.luaIcon = self.luaIcon:GetComponent("Image");
end

function M:SetData( skill )
	AssetRef.setImage(self.luaIcon, skill.icon);
end
--=======================
LSkillItem = M
createClass("LSkillItem",LSkillItem);
end