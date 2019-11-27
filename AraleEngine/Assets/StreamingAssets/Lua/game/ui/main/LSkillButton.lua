if LSkillButton then print("same lua, reload ignore!!!") end

local M =
{
	_cs;
	_skillTB;
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
	local tb = TableMgr.single:GetDataByKey(typeof(TBSkill), skill.mTID)
	AssetRef.setImage(self.luaIcon, tb.icon)
	self._skillTB = tb
end

function M:Play(hero)
	if self.luaMask.isCD then return end
	self.luaMask:Play(3)
	hero.skill:play(self._skillTB._id)
end

function M:PlaySkill(evt)
	local mw = WindowMgr.single:GetWindow("MainWindow").mLO.mLT;
	local hero = mw._hero;
	if hero == nil then return end
	local indicator = mw.luaIndicator
	indicator.transform.position = self._cs.transform.position
	indicator.gameObject:SetActive(true)
	indicator.onEvent = function(dir,disPercent,dragEnd)
		hero.skill:showIndicator (dir,self._skillTB.distance,disPercent,dragEnd)
		if dragEnd == true then self:Play(hero) end
	end
end
--========================
LSkillButton = M
createClass("LSkillButton",LSkillButton)