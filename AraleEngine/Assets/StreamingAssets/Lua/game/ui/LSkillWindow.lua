if LSkillWindow then print("same lua, reload ignore!!!") end

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
	local buffs = GHelper.toIntArray(csTB.skills)
	for i=1,skills.Length do
		local buff = TableMgr.single:GetDataByKey(typeof(TBBuff),buffs[i-1])
		local it = list:addItem(buff)
		if i == 1 then
			it.selected = true
			self:ShowRight(buff)
		end
		it.mLO.mLT:SetData(buff)
	end
	list.onSelectedChange = function(selItem)
		local item = selItem.data
		self:ShowRight(item)
	end

	--bottom
	local hasSkills = self._player.skill.skills
	for i=1,hasSkills.Count do
		local gs = hasSkills[i-1].GS
		local icon = self._slots[i]
		if icon~= nil then
			icon.gameObject:SetActive(true)
			AssetRef.setImage(icon, gs.icon)
		end
	end
end

function M:ShowRight(buff)
	local gs = GameSkill.get(buff.param,buff.lua)
	AssetRef.setImage(self.luaIcon, gs.icon)
	self.luaName.text = "...."
	self.luaDesc.text = "...."
	self.luaCast.text = "消耗1个技能点"
end

function M:OnSkillList(dragItem,receiver)
	local it = dragItem:GetComponentInParent(typeof(UISListItem))
	local buff = it.data
	local gs = GameSkill.get(buff.param,buff.lua)
	local slotId = tonumber(receiver.name)
	local icon = self._slots[slotId]
	AssetRef.setImage(icon, gs.icon);
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
			skill2 = TableMgr.single:GetDataByKey(typeof(Buff),skill2.TB.id)
			AssetRef.setImage(slot1, skill2.icon);
			slot1.gameObject:SetActive(true)
		else
			slot1.gameObject:SetActive(false)
		end

		if skill2 ~= nil then
			skill1 = TableMgr.single:GetDataByKey(typeof(TBSkill),skill1.TB.id)
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