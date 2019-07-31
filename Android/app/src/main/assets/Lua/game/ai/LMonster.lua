if LMonster then print("same lua, reload ignore!!!") end

local M=
{
	_cs;
	_table;
	_interval;
	[Enum.UnitEvent.StateChanged] = function(self,param)
		print("state change="..tostring(param))
	end;

	[Enum.UnitEvent.NavEnd] = function(self,param)
		local ai = self._cs.ai
		local unit = self._cs
		ai.busy=false;
	end;

	[Enum.UnitEvent.BeHit] = function(self,param)
		local ai = self._cs.ai
		local unit = self._cs
		local guid = tonumber(param)
		if ai.target == nil then ai.target = unit.mgr:getUnit(guid) end
		if unit.attr.HP < 50 then
			ai:doFlee(1)
		else
			ai:doSkill(1)
		end
	end;
}

function M:new(cs)
	self._interval = Randoms.rang(5,15)
	self._cs = cs;
	self._table = TableMgr.single:GetDataByKey(typeof(TBMonster), cs.tid)
	cs.luaOnEvent=self.OnUnitEvent
	local ai = cs.ai
	ai:setPatrolArea(cs.pos,10)

	local ta = cs.ai.timer
	local action = ta:AddAction(TimeMgr.Action())
	action.doTime = Randoms.rang(5,15)
	action.onAction = function(act)
		self:DoAI()
		act:Loop(self._interval)
	end
end

function M:OnUnitEvent(evt,param)
	local func = self[evt]
	if func == nil then return false end
	func(self, param)
	return true
end

function M:DoAI()
		local ai = self._cs.ai
		if ai.busy then return end
		local unit = self._cs
		if unit.attr.HP < 50 then return end
	 	if self._table.aggression > 0 then
	 		if ai.target == nil then ai:doTarget(1) end
	 	end
	 	if ai.target == nil then
	 		self._interval = Randoms.rang(5,15)
	 		ai:doPatrol(3)
	 	else
	 		self._interval = 2
	 		ai:doSkill(1)
	 	end
end;

--=======================
LMonster = M
createClass("LMonster",LMonster)