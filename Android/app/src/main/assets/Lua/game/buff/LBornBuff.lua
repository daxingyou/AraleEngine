if LBornBuff then print("same lua, reload ignore!!!") end

local M = 
{
	_cs  = nil;
	_unit= nil;
	[0] = function(this, param)
		this._unit = param;
		this._cs:decUnitState(this._unit,UnitState.Move);
		this._cs:decUnitState(this._unit,UnitState.Skill,true);
		this._cs.state = 1;
		local ta = this._cs.timer
		action = ta:AddAction(TimeMgr.Action());
		action.doTime = 2;
		action.onAction = function()
			this._cs.state = 0;
		end
	end;

	[1] = function(this, param)
		this._cs:addUnitState(this._unit,UnitState.Move);
		this._cs:addUnitState(this._unit,UnitState.Skill,true);
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
LBornBuff = M
createClass("LBornBuff",LBornBuff)
--======