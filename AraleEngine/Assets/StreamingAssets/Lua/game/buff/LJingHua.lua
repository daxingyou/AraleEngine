if not LJingHua then

LJingHua = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;
	new= function(this,cs)
		this._cs = cs;
		this._cs.luaOnEvent = function(evt,param) this:onEvent(evt,param); end
		this._param = BuffParam.LJingHua[cs.table.param];
	end;
	--========================
	onEvent = function(this, evt, param)
		this[evt](this, evt, param);
	end;

	[0] = function(this, evt, param)
		this._unit = param;
		this._unit.buff:clearBuff(this._param.clear);
		local ta = this._cs.timer;
		action = ta:addAction(TimeMgr.Action());
		action.doTime = this._param.duration;
		action.onAction = function(act)
			this._cs.state=0;
		end
	end;

	[1] = function(this, evt, param)
	end;
}

--must--
createClass("LJingHua",LJingHua)
--======
BuffParam.LJingHua=
{
	[0]=
	{
		clear=0x07;
		duration=0;
	};
	[1]=
	{
		clear=0x07;
		duration=10;
	};
}
end