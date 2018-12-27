if not LBag then
--======================
local M=
{
	_items;
}
function M:AddItem(item)
	self._items:Add(item)
end

function M:DelItem(itemID)

end

function M:GetItem(itemID)
end

function M:GetItems(itemType)
	local items = self._items
	local r = {}
	local j = 1;
	for i=1, items.Count do
		local it = items[i-1]
		if it._type==itemType then
			r[j] = it
			j=j+1 
		end
	end
	return r
end

function M:Init()
	self._items = List_object()
	local it = newLuaObject("LBagItem")
	it._id = 1001
	it._type= 0
	it._name= "测试道具1"
	it._icon= "UI/Icon/Equp/dao"
	it._count=10
	self:AddItem(it)
	local it = newLuaObject("LBagItem")
	it._id = 1002
	it._type= 0
	it._name= "测试道具2"
	it._icon= "UI/Icon/Equp/dun"
	it._count=10
	self:AddItem(it)
	local it = newLuaObject("LBagItem")
	it._id = 1003
	it._type= 0
	it._name= "测试道具3"
	it._icon= "UI/Icon/Equp/yifu"
	it._count=10
	self:AddItem(it)
end
--=======================
LBag = M
end