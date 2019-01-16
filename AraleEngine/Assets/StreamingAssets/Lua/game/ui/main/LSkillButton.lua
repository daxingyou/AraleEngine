if not LSkillButton then print("same lua, reload ignore!!!") end

local M =
{
	_skill;
}

function M:new(cs)
	cs.luaOnAwake = self.Awake
end

function M:Awake()
	self.luaIcon = self.luaIcon:GetComponent("Image")
	self.luaMask = self.luaMask:GetComponent("UICDImage")
end

function M:SetData(skill)
	local tb = TableMgr.single:GetDataByKey(typeof(TBSkill), skill.mTID)
	AssetRef.setImage(self.luaIcon, tb.icon)
	self._skill = skill
end

function M:Play(hero)
	if self.luaMask.isCD then return end
	self.luaMask:Play(3)
	hero.skill:play(self._skill.mTID)
end
--========================
LSkillButton = M
createClass("LSkillButton",LSkillButton)