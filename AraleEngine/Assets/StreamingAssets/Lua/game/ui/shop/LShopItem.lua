if not LShopItem then

local M =
{
	goods=nil;
}

function M:new(csobj)
	csobj.luaOnAwake = function() self:Awake(); end
end

function M:Awake()
	self.luaItem = self.luaItem:GetComponent("UIItemSlot")
	EventListener.Get(self.luaBuy):AddOnClick(function(evt)  print("buy goods id="..self.goods.id) end)
end

function M:SetData(goods)

	self.goods = goods;
	self.luaItem:SetData(goods.icon,goods.name,goods.price)
end
--========================
LShopItem = M
createClass("LShopItem",LShopItem)
end