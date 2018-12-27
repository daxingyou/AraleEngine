if not LJinGu then

LJinGu = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;
	new= function(this,cs)
		this._cs = cs;
		this._cs.luaOnEvent = function(evt,param) this:onEvent(evt,param); end
		this._param = BuffParam.JinGu[cs.table.param];
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
	end;
}

--must--
createClass("LJinGu",LJinGu)
--======
BuffParam.JinGu=
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