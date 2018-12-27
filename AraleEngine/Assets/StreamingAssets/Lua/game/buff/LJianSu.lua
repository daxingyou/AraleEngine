if not LJianSu then
--叠加时启用最大减速效果
LJianSu = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;
	_fallbackSpeed=1;
	new= function(this,cs)
		this._cs = cs;
		this._cs.luaOnEvent = function(evt,param) return this:onEvent(evt,param); end
		this._param = BuffParam.JianSu[cs.table.param];
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
		local speed = this._unit.attr.speed;
		if speed > this._param.speed then
			this._unit.attr.speed = this._param.speed;
			this._unit.attr:sync();
		end
		this._cs.state = 1;
		local ta = this._cs.timer
		action = ta:addAction(TimeMgr.Action());
		action.doTime = this._param.duration;
		action.onAction = function()
			this._cs.state = 0;
		end
	end;

	[1] = function(this, evt, param)
		this._unit.buff:sendEvent(Buff.Filter.Kind, 0x02, Buff.EvtOverlyingEnd, this);
		this._unit.attr.speed = this._fallbackSpeed;
		this._unit.attr:sync();
	end;

	[Buff.EvtOverlyingEnd] = function (this, evt, eventBuff)
		local speed = eventBuff._fallbackSpeed;
		if speed > this._param.speed then
			eventBuff._fallbackSpeed = this._param.speed;
		end
	end;
}

--must--
createClass("LJianSu",LJianSu)
--======
BuffParam.JianSu=
{
	[0]=
	{
		duration = 5;
		speed = 0.5;
	};
	[1]=
	{
		duration = 10;
		speed = 0.3;
	};
}
end