if not LSkillWindow then

local M =
{
	_player;
}

function M:new(cs)
	cs.luaOnStart = self.Start
end

function M:Start()
	if self._player == nil then return end
	self.luaIcon = self.luaIcon:GetComponent("Image");
	self.luaDesc = self.luaDesc:GetComponent("Text");
	self.luaName = self.luaName:GetComponent("Text");
	self.luaCast = self.luaCast:GetComponent("Text");
	self.luaSkill1 = self.luaSkill1:GetComponent("Image");
	self.luaSkill2 = self.luaSkill2:GetComponent("Image");
	self.luaSkill3 = self.luaSkill3:GetComponent("Image");
	self.luaSkill4 = self.luaSkill4:GetComponent("Image");
	EventListener.Get(self.luaBtn):AddOnClick(function(evt)   end)
	local list = self.luaContent:GetComponent("UISList");
	local csTB = TableMgr.single:GetDataByKey(typeof(TBPlayer),1001)
	local skills = GHelper.toIntArray(csTB.skills)
	for i=1,skills.Length do
		local skill = TableMgr.single:GetDataByKey(typeof(TBSkill),skills[i-1])
		local it = list:addItem(skill)
		if i == 1 then
			it.selected = true
			self:ShowRight(skill)
		end
		it.mLO.mLT:SetData(skill)
	end
	list.onSelectedChange = function(selItem)
		local item = selItem.data
		self:ShowRight(item)
	end

	--bottom
	local hasSkills = self._player.skill.skills
	local slot = {self.luaSkill1; self.luaSkill2; self.luaSkill3; self.luaSkill4;}
	for i=1,hasSkills.Count do
		local skill = TableMgr.single:GetDataByKey(typeof(TBSkill),hasSkills[i-1].mTID)
		local icon = slot[i]
		if icon~= nil then
			icon.gameObject:SetActive(true)
			AssetRef.setImage(icon, skill.icon)
		end
	end
end

function M:ShowRight(skill)
	AssetRef.setImage(self.luaIcon, skill.icon)
	self.luaName.text = "...."
	self.luaDesc.text = "...."
	self.luaCast.text = "消耗1个技能点"
end
--========================
LSkillWindow = M
createClass("LSkillWindow",LSkillWindow)
end