if not LJingHua then

local M = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;

	[0] = function(this, param)
		this._unit = param;
		this._unit.buff:clearBuff(this._param.clear);
		local ta = this._cs.timer;
		action = ta:AddAction(TimeMgr.Action());
		action.doTime = this._param.duration;
		action.onAction = function(act)
			this._cs.state=0;
		end
	end;

	[1] = function(this, param)
	end;
}

function M:new(cs)
	self._cs = cs
	self._param = BuffParam.JingHua[cs.table.param]
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
LJingHua = M
createClass("LJingHua",LJingHua)
--======
BuffParam.JingHua=
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