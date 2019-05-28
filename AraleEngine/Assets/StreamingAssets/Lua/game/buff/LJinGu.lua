if LJinGu then print("same lua, reload ignore!!!") end

local M = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;

	[0] = function(this, param)
		this._unit = param;
		this._cs:decUnitState(this._unit, UnitState.Move);
		this._cs:decUnitState(this._unit, UnitState.Skill,true);
		this._cs.state = 1;
		local ta = this._cs.timer
		action = ta:AddAction(TimeMgr.Action());
		action.doTime = this._param.duration;
		action.onAction = function()
			this._cs.state = 0;
		end
	end;

	[1] = function(this, param)
		this._cs:addUnitState(this._unit, UnitState.Move);
		this._cs:addUnitState(this._unit, UnitState.Skill,true);
	end;
}

function M:new(cs)
	self._cs = cs
	self._param = BuffParam.JinGu[cs.table.param]
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
LJinGu = M
createClass("LJinGu",LJinGu)
--======
BuffParam.JinGu=
{
	[0]=
	{
		duration = 2,
	};
	[1]=
	{
		duration = 10;
	};
}