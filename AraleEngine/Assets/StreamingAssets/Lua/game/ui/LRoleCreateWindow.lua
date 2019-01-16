if not LRoleCreateWindow then print("same lua, reload ignore!!!") end
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
	EventListener.Get(self.luaCreate):AddOnClick(function(evt)  self:OnCreateClick() end)
	self:ShowItems()
end

function M:ShowItems()
	local list = self.luaContent
	list:clearItem()
	local csTB = TableMgr.single:GetDataPool(typeof(TBPlayer))
	local keys = csTB.Keys:GetEnumerator()
	while keys:MoveNext() do
		local key = keys.Current
		local luaTB = LTBPlayer[key]
		local it = list:addItem(luaTB)
		if i == 1 then
			it.selected = true
		end
		it.mLO.mLT:SetData(luaTB)
	end

	--for key, value in pairs(LTBPlayer) do
	--无法按声明排序输出，而是按key的hash值顺序,可以额外建索引表遍历
	--	value.id = key
	--	local it = list:addItem(value)
	--	if i == 1 then
	--		it.selected = true
	--	end
	--	it.mLO.mLT:SetData(value)
	--end
end

function M:OnCreateClick()
	local sel = self.luaContent:getFirstSelected()
	if sel == nil then 
		WindowMgr.SendWindowMessage("NoticeWindow","ShowNotice","请选择你要创建的英雄")
		return 
	end
	local heroID = sel.mLO.mLT.role.id

	local msg = MsgReqCreateHero()
	msg.heroID = heroID
	NetMgr.client:sendMsg(Enum.MyMsgId.ReqCreateHero, msg)
	self._cs:Close(true)
end

--=======================
LRoleCreateWindow = M
createClass("LRoleCreateWindow",LRoleCreateWindow);