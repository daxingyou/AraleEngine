if not LDingShen then

LDingShen = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;
	new= function(this,cs)
		this._cs = cs;
		this._cs.luaOnEvent = function(evt,param) this:onEvent(evt,param); end
		this._param = BuffParam.DingShen[cs.table.param];
	end;
	--========================
	onEvent = function(this, evt, param)
		this[evt](this, evt, param);
	end;

	[0] = function(this, evt, param)
		this._unit = param;
		this._unit:addState(Unit.STMove);
		this._unit:addState(Unit.STAnim);
		this._unit:addState(Unit.STSkill,true);
		this._unit.attr.speed = 0;
		this._unit.attr:sync();
		this._cs.state = 1;
		local ta = this._cs.timer
		action = ta:addAction(TimeMgr.Action());
		action.doTime = this._param.duration;
		action.onAction = function()
			this._cs.state = 0;
		end
	end;

	[1] = function(this, evt, param)
		this._unit:decState(Unit.STMove);
		this._unit:decState(Unit.STAnim);
		this._unit:decState(Unit.STSkill,true);
		this._unit.attr.speed = 1;
		this._unit.attr:sync();
	end;
}

--must--
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