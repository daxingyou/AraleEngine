if not LJinGang then

LJinGang = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;
	new= function(this,cs)
		this._cs = cs;
		this._cs.luaOnEvent = function(evt,param) return this:onEvent(evt,param); end
		this._param = BuffParam.JinGang[cs.table.param];
	end;
	--========================
	onEvent = function(this, evt, param)
		local func = this[evt];
		if func == nil then return false end;
		func(this, evt, param);
		return true;
	end;

	[0] = function(this, evt, param)
		this._unit = param;
		this._unit.buff:clearBuff(0x0000ffff);
		this._cs.state = 1;
		local ta = this._cs.timer
		action = ta:addAction(TimeMgr.Action());
		action.doTime = this._param.duration;
		action.onAction = function()
			this._cs.state = 0;
		end
	end;
}

--must--
createClass("LJinGang",LJinGang)
--======
BuffParam.JinGang=
{
	[0]=
	{
		duration = 10,
	};
	[1]=
	{
		duration = 1;
	};
}
end