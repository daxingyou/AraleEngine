if not LJinGang then print("same lua, reload ignore!!!") end

local M = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;

	[0] = function(this, param)
		this._unit = param;
		this._unit.buff:clearBuff(0x0000ffff);
		this._cs.state = 1;
		local ta = this._cs.timer
		action = ta:AddAction(TimeMgr.Action());
		action.doTime = this._param.duration;
		action.onAction = function()
			this._cs.state = 0;
		end
	end;
}

function M:new(cs)
	self._cs = cs
	self._param = BuffParam.JinGang[cs.table.param]
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
LJinGang = M
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