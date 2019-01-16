if not LJinGu then print("same lua, reload ignore!!!") end

local M = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;

	[0] = function(this, param)
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

	[1] = function(this, param)
		this._unit:decState(Unit.STMove);
		this._unit:decState(Unit.STAnim);
		this._unit:decState(Unit.STSkill,true);
	end;
}

function M:new(cs)
	self._cs = cs
	self._param = BuffParam.JinGu[cs.table.param]
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
LJinGu = M
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