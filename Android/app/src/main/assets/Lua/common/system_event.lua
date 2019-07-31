
LSystemEvent = {}

local safe_call = safe_call
local select = select
local default_level = 1000
local callbacks = {}

--注册服务器内部事件回调
-- event_id : 定义在 common/globaldef.lua 中的 SystemEventID
-- callback : 回调函数, 参数为Player对象,参数列表(string) ==> function callback(player, ...) end
-- level : 调用级别, 数字越小越优先调用, 若为nil则默认为1000, 负数优先, 默认优先正数, 数字相同的按注册顺序优先
-- ...   : 条件参数. 若未指定则无条件调用callback；否则，依次判断LSystemEvent.Dispatch的...参数是否与条件参数相等，相等才调用callback
--[[比如

LSystemEvent.Regist(1, print, nil)
LSystemEvent.Regist(1, print, nil, 1)

LSystemEvent.Dispatch(1,1,2,3)	-- 1,2,3
								-- 2,3   
LSystemEvent.Dispatch(1, 2)		-- 2

--]]
local function sort_by_level(cb1, cb2)

	return cb1[2] < cb2[2]

end

function LSystemEvent.Regist(event_id, callback, level, ...)

	local cbs = callbacks[event_id]
	if not cbs then
		cbs = {}
		callbacks[event_id] = cbs
	end

	table.insert(cbs, {callback, level and (default_level+level) or default_level, ...})
	table.sort(cbs, sort_by_level)

end

function LSystemEvent.Dispatch(event_id, ...)

	local cbs = callbacks[event_id]

	if cbs then

		for i = 1, #cbs do
			
			local cb = cbs[i]
			local disable
			for j=3, #cb do
				if cb[j] ~= select(j-2, ...) then
					disable = true
					break
				end
			end
			if not disable then
				--cbs[i][1](select(#cb-1, ...))
				safe_call(cb[1], select(#cb-1, ...))
			end

		end

	end

end

function OnSystemEventDispatch(event_name, ...)

	local event_id = assert(SystemEventID[event_name])
	LSystemEvent.Dispatch(event_id, ...)

end
