if LDieBuff then print("same lua, reload ignore!!!") end

local M = 
{
	_cs  = nil;
	_unit= nil;
	[0] = function(this, param)
		this._unit = param;
		this._cs:decUnitState(this._unit,UnitState.Alive)
		this._cs:decUnitState(this._unit,UnitState.Move)
		this._cs:decUnitState(this._unit,UnitState.Skill)
		this._unit.anim:sendEvent (AnimPlugin.Die)
		this._cs:decUnitState(this._unit,UnitState.Anim)
		this._cs.state = 1
	end;
}

function M:new(cs)
	self._cs = cs
	cs.luaOnEvent = self.OnEvent
end

function M:OnEvent(evt, param)
		local func = self[evt]
		if func == nil then return false end
		func(self, param)
		return true
end
--must--
--========================
LDieBuff = M
createClass("LDieBuff",LDieBuff)
--======