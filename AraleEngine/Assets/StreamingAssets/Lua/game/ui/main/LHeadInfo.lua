if not LHeadInfo then

local M =
{
	[Enum.UnitEvent.HeadInfoInit] = function(self, unit)
		local tb;
		if unit.type == UnitType.Player then
			tb = LTBPlayer[unit.tid]
		else
			tb = TableMgr.single:GetDataByKey(typeof(TBMonster), unit.tid)
		end
		self.luaName.text = tb.name
	end;
}

function M:new(cs)
	cs.luaOnAwake = self.Awake
	cs.luaOnEvent = self.OnEvent
end

function M:Awake()
	self.luaName = self.luaName:GetComponent(typeof(UI.Text))
end

function M:OnEvent(evt,unit)
	local f = self[Enum.UnitEvent.HeadInfoInit]
	if f==nil then return false end
	f(self, unit)
	return true
end
--========================
LHeadInfo = M
createClass("LHeadInfo",LHeadInfo)
end