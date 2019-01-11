if not LBagWindow then
--======================
local M={}
function M:new(cs)
	cs.luaOnStart =self.Start
end

function M:Start()
	self.luaName = self.luaName:GetComponent("Text");
	self.luaDesc = self.luaDesc:GetComponent("Text");
	self.luaIcon = self.luaIcon:GetComponent("Image");
	EventListener.Get(self.luaUse):AddOnClick(function(evt)  self:UseItem() end)
	self.luaContent = self.luaContent:GetComponent("UISList");
	self.luaContent.onSelectedChange = function(selItem)
		local item = selItem.mLO.mLT._item
		self:ShowItemDesc(item)
	end

	local sbs = UISwitch.getGroupSwitch ("bag1");
	for i=1, sbs.Count do
		sbs[i-1].onValueChange = function(sb) self:OnSwitchChange(sb) end
	end
	sbs[0].isOn = true
end

function M:OnSwitchChange(sb)
	if sb.isOn ~= true then return end
	self:ShowItems(sb._userData)
end

function M:ShowItems(itemType)
	local list = self.luaContent
	local bag = NetMgr.client.bag
	local items = bag:getItems(itemType)
	local count = items.Count
	list:clearItem()
	for i=1,bag.bagSize do
		local it = list:addItem(nil)
		if i>count then
			it.mLO.mLT:Show(false)
		else
			local item = items[i-1]
			it.mLO.mLT:SetData(item)
			it.mLO.mLT:Show(true)
			
			if i == 1 then
			 	it.selected = true 
			 	self:ShowItemDesc(item)
			end
		end
	end
end

function M:ShowItemDesc(bagItem)
	self.luaSelItem:SetActive(bagItem~=nil)
	if bagItem == nil then return end
	local item = LTBItem[bagItem.id]
	self.luaName.text = item.name
	self.luaDesc.text = item.desc
	AssetRef.setImage(self.luaIcon, item.icon);
end

function M:UseItem()
	WindowMgr.single:GetWindow("ForginWindow", true)
end
--=======================
LBagWindow = M
createClass("LBagWindow",LBagWindow);
end