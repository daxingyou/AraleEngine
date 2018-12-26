if not LJianXue then

LJianXue = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;
	_times=0;
	new= function(this,cs)
		this._cs = cs;
		this._cs.luaOnEvent = function(evt,param) this:onEvent(evt,param); end
		this._param = BuffParam.JianXue[cs.table.param];
	end;
	--========================
	onEvent = function(this, evt, param)
		this[evt](this, evt, param);
	end;

	[0] = function(this, evt, param)
		this._unit = param;
		this._unit:addState(Unit.STSkill,true);
		local ta = this._cs.timer
		
		action = ta:addAction(TimeMgr.Action());
		action.doTime = this._param.duration;
		action.onAction = function(act)
			this._unit.attr.HP = this._unit.attr.HP + this._param.hp;
			this._unit.attr:sync();
			this._times=this._times+1;
			if this._times >= this._param.times then
				this._cs.state = 0;
			else
				act:loop(this._param.interval);
			end
		end
	end;

	[1] = function(this, evt, param)
		this._unit:decState(Unit.STSkill,true);
	end;
}

--must--
createClass("LJianXue",LJianXue)
--======
BuffParam.JianXue=
{
	[0]=
	{
		hp       = -10;
		interval = 1,
		times    = 5,
	};
	[1]=
	{
		hp       = -3;
		interval = 1,
		times    = 10,
	};
}
end