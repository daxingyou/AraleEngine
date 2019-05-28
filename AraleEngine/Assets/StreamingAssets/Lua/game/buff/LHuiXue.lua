if LHuiXue then print("same lua, reload ignore!!!") end

local M = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;
	_times=0;

	[0] = function(this, param)
		this._unit = param;
		this._cs:decUnitState(this._unit,UnitState.Skill,true);
		local ta = this._cs.timer
		
		action = ta:AddAction(TimeMgr.Action());
		action.onAction = function(act)
			this._unit.attr.HP = this._unit.attr.HP + this._param.hp;
			this._unit.attr:sync();
			this._times=this._times+1;
			if this._times >= this._param.times then
				this._cs.state = 0;
			else
				act:Loop(this._param.interval);
			end
		end
	end;

	[1] = function(this, param)
		this._cs:addUnitState(this._unit,UnitState.Skill,true);
	end;
}

function M:new(cs)
	self._cs = cs
	self._param = BuffParam.HuiXue[cs.table.param]
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
LHuiXue = M
createClass("LHuiXue",LHuiXue)
--======
BuffParam.HuiXue=
{
	[0]=
	{
		hp       = 10;
		interval = 1,
		times    = 5,
	};
	[1]=
	{
		hp       = 3;
		interval = 1,
		times    = 10,
	};
}