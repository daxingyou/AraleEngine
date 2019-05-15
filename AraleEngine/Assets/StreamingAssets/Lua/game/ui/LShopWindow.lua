if LShopWindow then print("same lua, reload ignore!!!") end

local M =
{
	goods1 = 
	{
		token = 0;--人民币购买,服务器器下发
		[1]={id="1001";itemid=1;num=1000;price=10;};
		[2]={id="1002";itemid=2;num=100;price=10;};
	};
	
	goods2 = 
	{
		token = 1;--金币购买
		[1]={id="2002";itemid=2001;num=10;price=100;};
		[2]={id="2003";itemid=2002;num=10;price=100;};
		[3]={id="2004";itemid=2003;num=10;price=100;};
		[4]={id="2005";itemid=2004;num=10;price=1000;};
	};
}

function M:new(csobj)
	csobj.luaOnStart = function() self:Start(); end
	csobj.luaOnDestroy = function() self:Destroy(); end
	self._onPropChange = function (evt) self:OnPropChange(evt) end
end

function M:Start()
	EventMgr.single:AddListener("Player.Prop", self._onPropChange)
	self.luaGold:GetComponent("Text").text = tostring(LClient:GetProp(1))
	self.luaDiamond:GetComponent("Text").text = tostring(LClient:GetProp(2))
	self.luaContent = self.luaContent:GetComponent("UISList")
	local ls = UISwitch.getGroupSwitch("shop1")
	local i = 0
	while i<ls.Count do
		ls[i].onValueChange = function(sb) self:onSwichChange(sb) end
		i = i+1
	end
	ls[0].isOn = true
end

function M:Destroy()
	EventMgr.single:RemoveListener("Player.Prop", self._onPropChange)
end

function M:onSwichChange(sb)
	if sb.isOn~=true then return end
	self:initPage(sb._userData)
end

function M:initPage(tabId)
	local goods = self.goods1
	if tabId == 1 then goods = self.goods2 end
	local list = self.luaContent
	list:clearItem()
	for i=1,#goods do
		local it = list:addItem(goods[i])
		it = it.mLO.mLT
		it.token = goods.token
		it:SetData(goods[i])
	end
end

function M:OnPropChange(evt)
	local item = evt.data
	if item.itemId == 1 then
		self.luaGold:GetComponent("Text").text = tostring(item.count)
	end

	if item.itemId == 2 then
		self.luaDiamond:GetComponent("Text").text = tostring(item.count)
	end
end
--========================
LShopWindow = M
createClass("LShopWindow",LShopWindow)