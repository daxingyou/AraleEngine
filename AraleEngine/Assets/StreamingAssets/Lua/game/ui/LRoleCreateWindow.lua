if not LRoleCreateWindow then
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

local Role = 
{
	[1] = {id=1001; typename="法师"; name="食人魔"; icon="UI/Icon/Header/guai"; };
	[2] = {id=1002; typename="战士"; name="铁男"; icon="UI/Icon/Header/nan"; };
	[3] = {id=1003; typename="牧师"; name="雪女"; icon="UI/Icon/Header/nv"; };
}

function M:ShowItems()
	local list = self.luaContent
	list:clearItem()
	for i=1,#Role do
		local it = list:addItem(Role[i])
		if i == 1 then
			it.selected = true
		end
		it.mLO.mLT:SetData(Role[i])
	end
end

function M:OnCreateClick()
	local sel = self.luaContent:getFirstSelected()
	local heroID = sel.mLO.mLT.role.id

	local msg = MsgReqCreateHero()
	msg.heroID = heroID
	NetMgr.client:sendMsg(Enum.MyMsgId.ReqCreateHero, msg)
	self._cs:Close(true)
end

--=======================
LRoleCreateWindow = M
createClass("LRoleCreateWindow",LRoleCreateWindow);
end