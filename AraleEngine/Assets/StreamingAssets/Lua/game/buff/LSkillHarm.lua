if not LSkillHarm then

local M = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;

	[0] = function(this, param)
		this._unit = param;
		this._unit:forward(this._unit.skill.targetPos);
		this._unit:addState(UnitState.Move);
		this._unit.anim:sendEvent(AnimPlugin.PlayAnim, this._param.anim);

		local ta = this._cs.timer
		action = ta:AddAction(TimeMgr.Action());
		action.doTime = this._param.delay;
		action.onAction = function()
			local area  = GameArea.fromString(this._param.area);
			local mt = this._unit.localToWorld.inverse;
			local units = this._unit.mgr:getUnitInArea(2, area, mt)
			for i=1,units.Count do
				local  u = units[i-1];
				if u.guid ~= this._unit.guid then
					u:forward(this._unit.pos);
					u.anim:sendEvent(AnimPlugin.Hit);
					local attr = u.attr;
					attr.HP = attr.HP + this._param.hp;
					attr:sync();
				end
			end
			this._cs.state = 0;
		end
	end;

	[1] = function(this, param)
		this._unit:decState(UnitState.Move,true);
	end;
}

function M:new(cs)
	self._cs = cs
	self._param = BuffParam.SkillHarm[cs.table.param]
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
LSkillHarm = M
createClass("LSkillHarm",LSkillHarm)
--======
BuffParam.SkillHarm=
{
	[0]=
	{
		anim = "attack";
		delay= 0.5;
		area = "2,3,3";
		hp   = -10;
	};

	[1]=
	{
		anim = "skill04";
		action={
			delay= 2.5;
			area = "0,3";
			hp   = -10;
		};
		action={
			delay= 2.5;
		}
	};
}
end