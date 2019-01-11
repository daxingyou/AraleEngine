if not LShopItem then

local M =
{
	goods=nil;
	token=nil;
}

function M:new(csobj)
	csobj.luaOnAwake = function() self:Awake(); end
end

function M:Awake()
	self.luaItem = self.luaItem:GetComponent("UIItemSlot")
	EventListener.Get(self.luaBuy):AddOnClick(function(evt) self:Buy() end)
end

function M:SetData(goods)
	self.goods = goods;
	local tokenName = "元"
	if self.token == 1 then tokenName = "金币" end
	self.luaPrice:GetComponent("Text").text = tostring(goods.price)..tokenName
	local item = LTBItem[goods.itemid]
	self.luaItem:SetData(item.icon,item.name,goods.num)
end

function M:Buy()
	local goods = self.goods
	local tokenName = "元"
	if self.token == 1 then tokenName = "金币" end
	local item = LTBItem[goods.itemid]
	local msg = WindowMgr.single:GetWindow("MessageWindow", true).mLO.mLT
	if self.token == 1 then
		local  gold = LClient:GetProp(1)
		if gold < goods.price then msg:ShowMessage("金币不足"); return end
	end

	msg:ShowMessage("是否花"..tostring(goods.price)..tokenName.."购买"..tostring(goods.num)..item.name, function() self:ReqBuy() end):ShowNo()
end

function M:ReqBuy()
	local msg = MsgItem()
	msg.itemId = self.goods.itemid
	msg.count = self.goods.num
	NetMgr.client:sendMsg(Enum.MyMsgId.ReqBuyItem, msg)
end
--========================
LShopItem = M
createClass("LShopItem",LShopItem)
end