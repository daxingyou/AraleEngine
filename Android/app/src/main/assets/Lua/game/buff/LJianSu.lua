if LJianSu then print("same lua, reload ignore!!!") end
--叠加时启用最大减速效果
local M = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;
	_fallbackSpeed=1;

	[0] = function(this, param)
		this._unit = param;
		local speed = this._unit.attr.speed;
		if speed > this._param.speed then
			this._unit.attr.speed = this._param.speed;
			this._unit.attr:sync();
		end
		this._cs.state = 1;
		local ta = this._cs.timer
		action = ta:AddAction(TimeMgr.Action());
		action.doTime = this._param.duration;
		action.onAction = function()
			this._cs.state = 0;
		end
	end;

	[1] = function(this, param)
		this._unit.buff:sendEvent(Buff.Filter.Kind, 0x02, Buff.EvtOverlyingEnd, this);
		this._unit.attr.speed = this._fallbackSpeed;
		this._unit.attr:sync();
	end;

	[Buff.EvtOverlyingEnd] = function (this, eventBuff)
		local speed = eventBuff._fallbackSpeed;
		if speed > this._param.speed then
			eventBuff._fallbackSpeed = this._param.speed;
		end
	end;
}

function M:new(cs)
	self._cs = cs
	self._param = BuffParam.JianSu[cs.table.param]
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
LJianSu = M
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