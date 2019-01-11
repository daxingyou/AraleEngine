if not LBagWindow then
--======================
local M=
{
	_cs;
}

function M:new(cs)
	self._cs = cs;
	cs.luaOnStart = function() self:Start(); end
end

function M:Start()
	self.luaContent = self.luaContent:GetComponent("UISList");
	
	local sbs = UISwitch.getGroupSwitch ("bag1");
	for i=1, sbs.Count do
		sbs[i-1].onValueChange = function(sb) self:OnSwitchChange(sb) end
	end
	sbs[0].isOn = true
	--self.luaPaiming:GetComponent(typeof(CU.UI.Text)).text = "未上榜"
end

function M:OnSwitchChange(sb)
	if sb.isOn ~= true then return end
	self:ShowItems(sb._userData)
end

function M:ShowItems(itemType)
	local list = self.luaContent
	list:clearItem()
	local bag = NetMgr.client.bag
	local items = bag:getItems(itemType)
	for i=1,items.Count do
		local item = items[i-1]
		local it = list:addItem(item)
		if i == 1 then it.selected = true end
		it.mLO.mLT:SetData(item)
	end
end

--=======================
LBagWindow = M
createClass("LBagWindow",LBagWindow);
end