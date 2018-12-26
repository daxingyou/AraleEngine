if not LLeiGongBaoFeng then

LLeiGongBaoFeng = 
{
	_cs  = nil;
	_unit= nil;
	_param=nil;
	new= function(this,cs)
		this._cs = cs;
		this._cs.luaOnEvent = function(evt,param) return this:onEvent(evt,param); end
		this._param = BuffParam.LeiGongBaoFeng[cs.table.param];
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
		this._unit:addState(Unit.STSkill,true);
		this._cs.state = 1;
		local ta = this._cs.timer
		action = ta:addAction(TimeMgr.Action());
		action.doTime = this._param.duration;
		action.onAction = function()
			this._cs.state = 0;
		end

		action = ta:addAction(TimeMgr.Action());
		action.onAction = function(act)
			local area  = GameArea.fromString(this._param.area);
			local mt = this._unit.localToWorld.inverse;
			local units = this._unit.mgr:getUnitInArea(2, area, mt)
			for i=1,units.Count do
				local  u = units[i-1];
				if u.guid ~= this._unit.guid then
					u:backward(this._unit.pos);
					u.pos = Vector3.MoveTowards(u.pos, this._unit.pos, 0.2);
					u:syncState();
				end
			end
			act:loop(this._param.interval);
		end
	end;

	[1] = function(this, evt, param)
		this._unit:decState(Unit.STSkill,true);
	end;
}

--must--
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