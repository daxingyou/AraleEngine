
serialize = {}

local loadstring = loadstring
local select = select
local pairs = pairs
local tostring = tostring
local mark = {}
local assign = {}
local assign_i
--[[
local function fmt_string(s)

	s = s:gsub("\n", "\\n")
	s = s:gsub("\t", "\\t")
	s = s:gsub("\"", "\\\"")
	return ("\"%s\""):format(s)

end
--]]
local function make_gsub(t)

	local keys = '['
	for k in pairs(t) do
		keys = keys .. k
	end
	keys = keys .. ']'

	local function exchange(c)
		return t[c]
	end

	return function(s)
		s = s:gsub(keys, exchange)
		return ("\"%s\""):format(s)
	end

end

local fmt_string = make_gsub{
    ['"']  = '\\"',
    ['\\'] = '\\\\',
    ['\b'] = '\\b',
    ['\f'] = '\\f',
    ['\n'] = '\\n',
    ['\r'] = '\\r',
    ['\t'] = '\\t'
}

local function ser_table(tbl,parent)
	mark[tbl]=parent
	local tmp={}
	for k,v in pairs(tbl) do
		local key
		local k_type = type(k)
		local v_type = type(v)
		if v_type ~= "function" then 
			if k_type == "number" then
				key = ('[%s]'):format(k)
			elseif k_type == "string" then
				key = ("['%s']"):format(k)
			else
				key = k
			end

			if v_type=="table" then
				local dotkey= parent..key
				local mark_v = mark[v]
				if mark_v then
					assign_i = assign_i + 1
					assign[assign_i] = ("%s=%s"):format(dotkey, mark_v)
				else
					table.insert(tmp, ("%s=%s"):format(key,ser_table(v,dotkey)))
				end
			else
				if v_type == "string" then
					table.insert(tmp, ("%s=%s"):format(key, fmt_string(v)))
				else
					table.insert(tmp, ("%s=%s"):format(key, tostring(v)))
				end
			end
		end
	end
	return ("{%s}"):format(table.concat(tmp,","))
end
	
local function serialize_table(t)
	
	for k in pairs(mark) do 
		mark[k] = nil 
	end
	
	assign_i = 0
	local ret = ser_table(t, "ret")
	
	if assign_i > 0 then
		return "function() local ret=" .. ret .. table.concat(assign," ",1,assign_i) .. " return ret end"
	else
		return ret
	end
	
end

local t_serialize = {}
function serialize.encode(...)
	
	local n = select('#',...)
	for i=1,n do
		local v = select(i,...)
		if type(v) == "table" then
			v = serialize_table(v)
		else
			if type(v) == "string" then
				v = fmt_string(v)--("[[%s]]"):format(v)
			else
				v = tostring(v)
			end
		end
		t_serialize[i] = v
	end

	local result = table.concat(t_serialize, ',', 1, n)

	return result

end

local function decode_ret(n, i, ...)

	if not n then
		n = select('#', ...)
	end
	
	if i > n then return end
	local v = select(i, ...)
	if type(v) == "function" then v = v() end
	return v, decode_ret(n, i+1, ...)

end

function serialize.decode(s)

	local chunk,err = loadstring("return " .. s)
	if not chunk then 
		PrintLog("serialize.decode loadstring failed : " .. s, "ERROR")
		return 
	end
	
	return decode_ret(nil, 1, chunk())

end


---------------------------------------------------------------------------
-- 序列化成可读的格式
local serial_val
local function serial_tab(t, depth)

	local tmp = {}
	local size = #t

	depth = depth or 1
	local have_tab
	for k,v in pairs(t) do
		if type(k) ~= "number" or k > size or k < 1 then
			local ks
			if type(k) == "string" then
				ks = k
			else
				ks = ("[%s]"):format(k)
			end
			if type(v) == "table" then have_tab = true end
			table.insert(tmp, ("%s = %s"):format(ks, serial_val(v, depth+1)))
		end
	end

	for i=1,size do
		local v = t[i]
		if type(v) == "table" then have_tab = true end
		table.insert(tmp, serial_val(v, depth+1))
	end

	local tab_parent
	local tab_child
	local sep
	local side
	if have_tab then
		tab_parent = ("\t"):rep(depth-1)
		tab_child = ("\t"):rep(depth)
		sep = ",\n" .. tab_child
		side = "\n"
	else
		tab_parent = ''
		tab_child = ''
		sep = ', '
		side = ''
	end
	return ("{%s%s%s%s%s}"):format(side, tab_child, table.concat(tmp, sep), side, tab_parent)

end

function serial_val(v, ...)

	local tp = type(v)
	if tp == "string" then
		return fmt_string(v)
	elseif tp == "table" then
		return serial_tab(v, ...)
	else
		return tostring(v)
	end

end

function serialize.encode_view(v)

	return serial_val(v)

end
