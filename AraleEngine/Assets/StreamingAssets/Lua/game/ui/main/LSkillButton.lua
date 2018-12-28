if not LSkillButton then

local M =
{
	_skill;
}

function M:new(cs)
	cs.luaOnAwake = self.Awake
end

function M:Awake()
	self.luaIcon = self.luaIcon:GetComponent("Image")
	self.luaMask = self.luaIcon:GetComponent("Image")
end

function M:SetData(skill)
	local tb = TableMgr.single:GetDataByKey(typeof(TBSkill), skill.mTID)
	AssetRef.setImage(self.luaIcon, tb.icon)
	self.luaMask.fillAmount = 1
	self._skill = skill
end
--========================
LSkillButton = M
createClass("LSkillButton",LSkillButton)
end