if not LMonster then

LMonster = 
{
	_csobject = nil;
	_aiCT = nil;
	data = nil;
	new= function(self,csobject)
		self._csobject = csobject;
		csobject.luaStart = function() self:start(); end
		csobject.luaOnEvent = function(evt,param) self:onEvent(evt,param); end
	end;
	--========================
	start = function(self)
	end;

	onEvent = function(self, evt, param)
		if evt==1 then
		--[[self._csobject:StartCoroutine(function()
			print("a");
		 	LuaBehaviour.yieldReturn(0);
			print("b");
			end
		);]]
		end
	end;

	aiFunc = function()
		print("a");
		 coroutine.yield();
		print("b");
	end;
}

--must--
createClass("LMonster",LMonster)
--======
end