if LHeadInfo then print("same lua, reload ignore!!!") end

local M =
{
	_unit;
	[Enum.UnitEvent.HeadInfoInit] = function(self, unit)
		self._unit = unit;
		local tb;
		if unit.type == UnitType.Player then
			tb = LTBPlayer[unit.tid]
		else
			tb = LTBMonster[unit.tid]
		end
		self.luaName.text = tb.name
		self.luaHP.value = unit.attr.HP/100;
		unit:addListener(self._onUnitListener)
	end;

	[Enum.UnitEvent.HeadInfoDeinit] = function(self)
		self._unit:removeListener(self._onUnitListener)
		self._unit = nil
	end;
}

function M:new(cs)
	cs.luaOnAwake = self.Awake
	cs.luaOnEvent = self.OnEvent
	self._onUnitListener = function(evt, param)
		if evt ~= Enum.UnitEvent.AttrChanged then return end
		if Enum.AttrID.HP == param.attrId then
			self.luaHP.value = param.val/100;
		end
	end;
end

function M:Awake()
	self.luaName = self.luaName:GetComponent(typeof(UI.Text))
	self.luaHP = self.luaHP:GetComponent(typeof(UI.Slider))
end

function M:OnEvent(evt,unit)
	local f = self[evt]
	if f==nil then return false end
	f(self, unit)
	return true
end
--========================
LHeadInfo = M
createClass("LHeadInfo",LHeadInfo)