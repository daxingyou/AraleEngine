if not LRoleCreateWindowItem then print("same lua, reload ignore!!!") end
--======================
local M=
{
}

function M:new(cs)
end

function M:SetData(role)
	self.role = role
	AssetRef.setImage(self.luaIcon:GetComponent("Image"), role.icon)
	self.luaName:GetComponent("Text").text = role.name
	self.luaType:GetComponent("Text").text = role.typename
end

--=======================
LRoleCreateWindowItem = M
createClass("LRoleCreateWindowItem",LRoleCreateWindowItem);