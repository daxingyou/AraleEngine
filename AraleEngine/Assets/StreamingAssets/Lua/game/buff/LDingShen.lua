if not LDingShen then

local M = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;

	[0] = function(this, param)
		this._unit = param;
		this._unit:addState(UnitState.Move);
		this._unit:addState(UnitState.Anim);
		this._unit:addState(UnitState.Skill,true);
		this._unit.attr.speed = 0;
		this._unit.attr:sync();
		this._cs.state = 1;
		local ta = this._cs.timer
		action = ta:AddAction(TimeMgr.Action());
		action.doTime = this._param.duration;
		action.onAction = function()
			this._cs.state = 0;
		end
	end;

	[1] = function(this, param)
		this._unit:decState(UnitState.Move);
		this._unit:decState(UnitState.Anim);
		this._unit:decState(UnitState.Skill,true);
		this._unit.attr.speed = 1;
		this._unit.attr:sync();
	end;
}

function M:new(cs)
	self._cs = cs
	this._param = BuffParam.DingShen[cs.table.param]
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
LDingShen = M
createClass("LDingShen",LDingShen)
--======
BuffParam.DingShen=
{
	[0]=
	{
		duration = 5,
	};
	[1]=
	{
		duration = 1;
	};
}
end