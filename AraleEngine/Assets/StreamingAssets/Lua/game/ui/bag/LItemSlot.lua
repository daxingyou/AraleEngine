if not LItemSlot then
--======================
local M=
{
}
function M:new(cs)
	cs.luaOnAwake = function() self:Awake(); end
end

function M:Awake()
	if self.luaIcon ~= nil then
		self.luaIcon = self.luaIcon:GetComponent("Image");
	end
	if self.luaNum ~= nil then
		self.luaNum = self.luaNum:GetComponent("Text");
	end
end

function M:SetData( data )
	if self.luaIcon ~= nil then
		AssetRef.setImage(self.luaIcon, data._icon);
	end
	if self.luaNum ~= nil then
		self.luaNum.text = tostring(data._count)
	end
end
--=======================
LItemSlot = M
createClass("LItemSlot",LItemSlot);
end