if LPlayerWindow then print("same lua, reload ignore!!!") end

local M =
{
	_player;
}

function M:new(cs)
	cs.luaOnStart = self.Start
end

function M:Start()
	local csTB  = TableMgr.single:GetDataByKey(typeof(TBPlayer), self._player.tid)
	local luaTB = LTBPlayer[self._player.tid]
	local mod = ResLoad.get(csTB.model):gameObject()
	GameObject.Destroy(mod:GetComponent(typeof(Rigidbody)))
	GameObject.Destroy(mod:GetComponent(typeof(NavMeshAgent)))
	GameObject.Destroy(mod:GetComponent(typeof(Collider)))
	mod.transform:SetParent(self.luaModel.transform, false)
	GHelper.SetLayer(mod.transform, "UI")

	EventListener.Get(self.luaE1):AddOnClick(function(evt)  self:OnEquipClick(evt) end)
	EventListener.Get(self.luaE2):AddOnClick(function(evt)  self:OnEquipClick(evt) end)
	EventListener.Get(self.luaE3):AddOnClick(function(evt)  self:OnEquipClick(evt) end)
	EventListener.Get(self.luaE4):AddOnClick(function(evt)  self:OnEquipClick(evt) end)
	EventListener.Get(self.luaE5):AddOnClick(function(evt)  self:OnEquipClick(evt) end)
	EventListener.Get(self.luaE6):AddOnClick(function(evt)  self:OnEquipClick(evt) end)
	EventListener.Get(self.luaE7):AddOnClick(function(evt)  self:OnEquipClick(evt) end)
	EventListener.Get(self.luaE8):AddOnClick(function(evt)  self:OnEquipClick(evt) end)
end

function M:OnEquipClick(evt)
	WindowMgr.single:GetWindow("BagWindow", true)
end
--========================
LPlayerWindow = M
createClass("LPlayerWindow",LPlayerWindow)