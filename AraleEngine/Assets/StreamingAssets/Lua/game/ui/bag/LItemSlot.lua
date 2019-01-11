if not LItemSlot then
--======================
local M=
{
	_item;
}
function M:new(cs)
	cs.luaOnAwake = function() self:Awake(); end
end

function M:Awake()
	if self.luaIcon ~= nil then
		self.luaIcon = self.luaIcon:GetComponent("Image");
	end
	if self.luaNum ~= nil then
		self.luaNum = self.luaNum:GetComponent("Text");
	end
end

function M:SetData( bagItem )
	self._item = bagItem
	local item = LTBItem[bagItem.id]
	if self.luaIcon ~= nil then
		AssetRef.setImage(self.luaIcon, item.icon);
	end
	if self.luaNum ~= nil then
		self.luaNum.text = tostring(bagItem.count)
	end
end

function M:Show(show)
	if not show then self._item=nil end 
	if self.luaIcon ~= nil then self.luaIcon.gameObject:SetActive(show) end
	if self.luaNum ~= nil then self.luaNum.gameObject:SetActive(show) end
end
--=======================
LItemSlot = M
createClass("LItemSlot",LItemSlot);
end