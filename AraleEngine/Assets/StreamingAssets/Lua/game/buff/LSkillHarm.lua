if not LSkillHarm then

LSkillHarm = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;
	new= function(this,cs)
		this._cs = cs;
		this._cs.luaOnEvent = function(evt,param) this:onEvent(evt,param); end
		this._param = BuffParam.SkillHarm[cs.table.param];
	end;
	--========================
	onEvent = function(this, evt, param)
		this[evt](this, evt, param);
	end;

	[0] = function(this, evt, param)
		this._unit = param;
		this._unit:forward(this._unit.skill.targetPos);
		this._unit:addState(Unit.STMove);
		this._unit.anim:sendEvent(AnimPlugin.PlayAnim, this._param.anim);

		local ta = this._cs.timer
		action = ta:addAction(TimeMgr.Action());
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

	[1] = function(this, evt, param)
		this._unit:decState(Unit.STMove,true);
	end;
}

--must--
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