if not LShopWindow then

local M =
{
}

function M:new(csobj)
	csobj.luaOnStart = function() self:Start(); end
end

function M:Start()
	self.luaContent = self.luaContent:GetComponent("UISList")
	local ls = UISwitch.getGroupSwitch("shop1")
	local i = 0
	while i<ls.Count do
		ls[i].onValueChange = function(sb) self:onSwichChange(sb) end
		i = i+1
	end
	ls[0].isOn = true
end

function M:onSwichChange(sb)
	if sb.isOn~=true then return end
	self:initPage(sb._userData)
end

function M:initPage(tabId)
	local goods1 = 
	{
		[1]={id="1001";name="金币";price=500;icon="ui/icon/shop/jinbi";};
		[2]={id="1002";name="钻石";price=500;icon="ui/icon/shop/zuanshi";};
		
	}
	local goods2 = 
	{
		[1]={id="2001";name="赤铁矿";price=100;icon="ui/icon/shop/chitie";};
		[2]={id="2002";name="黑铁矿";price=100;icon="ui/icon/shop/heitie";};
		[3]={id="2003";name="褐铁矿";price=100;icon="ui/icon/shop/hetie";};
		[4]={id="2004";name="黄铁矿";price=1000;icon="ui/icon/shop/huangtie";};
	}
	local goods = goods1
	if tabId == 1 then goods = goods2 end
	local list = self.luaContent
	list:clearItem()
	for i=1,#goods do
		local it = list:addItem(goods[i])
		it = it.mLO.mLT
		it:SetData(goods[i])
	end
end
--========================
LShopWindow = M
createClass("LShopWindow",LShopWindow)
end