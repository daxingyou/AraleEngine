if LHero then print("same lua, reload ignore!!!") end

LHero = 
{
	_cs= nil;
	new= function(self,cs)
		self._cs = cs;
		local ta = self._cs.timer
		action = ta:AddAction(TimeMgr.Action());
		action.doTime = 0;
		action.onAction = function(act)
			self:doAI();
			act:Loop(0.1);
		end
	end;
	--========================
	doAI = function(self)
		local cs = self._cs;
		local su = cs.unit;
		 if su.attr.HP < 20 then
		 	--反向逃离
		 	cs:doFlee(1);
		 else
		 	if cs.target == nil then
		 		--没有目标查找目标
		 		cs:doTarget(2);
		 	else
		 		--攻击目标
		 		cs:doSkill(1);
		 	end
		 end
	end;
}

--=======================
createClass("LHero",LHero)
--======