if not LClient then
--=================
LClient = {}
function LClient:Login()
	self.prop={}
	self._onPropChange = function (evt) self:OnPropChange(evt) end
	EventMgr.single:AddListener("Player.Prop", self._onPropChange)
end

function LClient:Logout()
	EventMgr.single:RemoveListener("Player.Prop", self._onPropChange)
end

function LClient:GetProp(propid)
	local val = self.prop[propid]
	if val==nil then return 0 end
	return val
end

function LClient:OnPropChange(evt)
	local item = evt.data
	self.prop[item.itemId] = item.count
end
--=================
end