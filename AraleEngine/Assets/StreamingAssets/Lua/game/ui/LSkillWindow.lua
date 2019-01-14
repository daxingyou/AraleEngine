if not LSkillWindow then

local M =
{
	_player;
	_slots;
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
	self._slots = {self.luaSkill1; self.luaSkill2; self.luaSkill3; self.luaSkill4;}
	self.luaContent:GetComponent("UIDrag").onDragReceived = function(dragItem,receiver) self:OnSkillList(dragItem, receiver) end
	self.luaSkillSlot:GetComponent("UIDrag").onDragReceived = function(dragItem,receiver) self:OnSkillSlot(dragItem, receiver) end
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
	for i=1,hasSkills.Count do
		local skill = TableMgr.single:GetDataByKey(typeof(TBSkill),hasSkills[i-1].mTID)
		local icon = self._slots[i]
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

function M:OnSkillList(dragItem,receiver)
	local it = dragItem:GetComponentInParent(typeof(UISListItem))
	local skill = it.data
	local slotId = tonumber(receiver.name)
	local icon = self._slots[slotId]
	AssetRef.setImage(icon, skill.icon);
	icon.gameObject:SetActive(true)
	GameObject.Destroy(dragItem.gameObject)
end

function M:OnSkillSlot(dragItem,receiver)
	local hasSkills = self._player.skill.skills
	local  slotId = tonumber(dragItem.transform.parent.name);
	local icon = self._slots[slotId]
	if receiver.dealType == 1 then--技能槽清除
		icon.transform.localPosition = Vector3.zero
		icon.gameObject:SetActive(false)
	else--技能槽替换
		local slotswap = tonumber(receiver.name)
		local hasSkills = self._player.skill.skills
		local skill1 = hasSkills[slotId - 1]
		local skill2 = hasSkills[slotswap - 1]
		hasSkills[slotId - 1] = skill2
		hasSkills[slotswap - 1] = skill1

		local slot1 = self._slots[slotId]
		local slot2 = self._slots[slotswap]
		slot1.transform.localPosition = Vector3.zero
		slot2.transform.localPosition = Vector3.zero
		if skill1 ~= nil then
			skill2 = TableMgr.single:GetDataByKey(typeof(TBSkill),skill2.mTID)
			AssetRef.setImage(slot1, skill2.icon);
			slot1.gameObject:SetActive(true)
		else
			slot1.gameObject:SetActive(false)
		end

		if skill2 ~= nil then
			skill1 = TableMgr.single:GetDataByKey(typeof(TBSkill),skill1.mTID)
			AssetRef.setImage(slot2, skill1.icon);
			slot2.gameObject:SetActive(true)
		else
			slot2.gameObject:SetActive(false)
		end
	end
	WindowMgr.SendWindowMessage ("MainWindow", "UpdateSkills", nil);
end
--========================
LSkillWindow = M
createClass("LSkillWindow",LSkillWindow)
end