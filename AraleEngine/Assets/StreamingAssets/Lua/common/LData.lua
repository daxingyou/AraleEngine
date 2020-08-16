if not LData then
--=========数据基类========
local M={ _listener={} }

function M:new(cs)
end

function M:AddOnDataChanged(callback)
	self._listener[callback] = callback
end

function M:RemoveOnDataChanged(callback)
	self._listener[callback] = nil
end

function M:Notify(mask, val)
	local listener = self._listener
	for k,v in pairs(listener) do
		if type(v) == 'function' then--注册的函数
			v(mask,val)
		else--注册的对象
			local func = v[mask]
			if func~= nil then func(v,val) end
		end
	end
end
--=======================
LData = M
createClass("LData",LData);
end

--测试demo
--[[
local Test =
{
}

function Test:TestLData()
	local data = instantiate(LData)
	self.callback = function(mask,val)
		print(tostring(mask)..","..tostring(val))
	end
	data:AddOnDataChanged(self.callback)
	data:AddOnDataChanged(self)
	data:Notify(1982,"test LData ok")
end

Test[1982]=function(self,val)
	print(val)
end

Test:TestLData()
]]