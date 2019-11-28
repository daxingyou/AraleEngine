if LSkillButton then print("same lua, reload ignore!!!") end

local M =
{
	_cs;
	_skill;
}

function M:new(cs)
	self._cs = cs;
	cs.luaOnAwake = self.Awake
end

function M:Awake()
	self.luaIcon = self.luaIcon:GetComponent("Image")
	self.luaMask = self.luaMask:GetComponent("UICDImage")
	EventListener.Get(self._cs.gameObject):AddOnPointDown(function(evt) self:PlaySkill(evt) end)
end

function M:SetData(skill)
	local tb = skill.TB
	AssetRef.setImage(self.luaIcon, tb.icon)
	self._skill = skill
end

function M:PlaySkill(evt)
	local mw = WindowMgr.single:GetWindow("MainWindow").mLO.mLT;
	local hero = mw._hero;
	if hero == nil then return end
	local indicator = mw.luaIndicator
	indicator.transform.position = self._cs.transform.position
	indicator.gameObject:SetActive(true)
	indicator.onEvent = function(dir,disPercent,dragEnd)
		hero.skill:showIndicator (self._skill,dir,disPercent,dragEnd)
	end
end
--========================
LSkillButton = M
createClass("LSkillButton",LSkillButton)