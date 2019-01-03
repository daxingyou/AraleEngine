if not LLeiGongBaoFeng then

local M = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;

	[0] = function(self, param)
		print(param)
		self._unit = param
		self._unit:addState(UnitState.Skill,true)
		self._cs.state = 1
		local ta = self._cs.timer
		action = ta:AddAction(TimeMgr.Action())
		action.doTime = self._param.duration
		action.onAction = function()
			self._cs.state = 0
		end

		action = ta:AddAction(TimeMgr.Action())
		action.onAction = function(act)
			local area  = GameArea.fromString(self._param.area)
			local mt = self._unit.localToWorld.inverse
			local units = self._unit.mgr:getUnitInArea(2, area, mt)
			for i=1,units.Count do
				local  u = units[i-1]
				if u.guid ~= self._unit.guid then
					u:backward(self._unit.pos)
					u.pos = Vector3.MoveTowards(u.pos, self._unit.pos, 0.2)
					u:syncState()
				end
			end
			act:Loop(self._param.interval)
		end
	end;

	[1] = function(self, param)
		self._unit:decState(UnitState.Skill,true)
	end;
}

function M:new(cs)
	self._cs = cs
	self._param = BuffParam.LeiGongBaoFeng[cs.table.param]
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
LLeiGongBaoFeng = M
createClass("LLeiGongBaoFeng",LLeiGongBaoFeng)
--======
BuffParam.LeiGongBaoFeng=
{
	[0]=
	{
		area = "0,4.00";
		interval = 0.1;
		duration = 20,
	};
	[1]=
	{
		area = "0,4.00";
		interval = 0.1;
		duration = 20;
	};
}
end