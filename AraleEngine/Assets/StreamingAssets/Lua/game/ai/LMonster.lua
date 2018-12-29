if not LMonster then
--======================
local M=
{
	_cs;
}

function M:new(cs)
	self._cs = cs;
	local ta = self._cs.timer
	local action = ta:AddAction(TimeMgr.Action())
	action.doTime = 0
	action.onAction = function(act)
		self:DoAI()
		act:Loop(0.1)
	end
end

	--========================
function M:DoAI()
		local cs = self._cs
		local su = cs.unit
		 if su.attr.HP < 20 then
		 	--反向逃离
		 	cs:doFlee(1)
		 else
		 	if cs.target == nil then
		 		--没有目标查找目标
		 		cs:doTarget(1)
		 	else
		 		--攻击目标
		 		cs:doSkill(1)
		 	end
		 end
	end;

--=======================
LMonster = M
createClass("LMonster",LMonster)
end