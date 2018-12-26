if not LBagItem then
--======================
local M=
{
	_id;
	_type;
	_name;
	_count;
	_icon;
}
function M:new()
end

--=======================
LBagItem = M
createClass("LBagItem",LBagItem);
end