if LMainWindow then print("same lua, reload ignore!!!") end

local M =
{
	_hero;
	_timeGap = 0;
	_cs;
}

function M:new(cs)
	self._cs = cs
	cs.luaOnStart = self.Start
	cs.luaOnDestroy=self.OnDestroy
	cs.luaOnUpdate=self.OnUpdate
	cs.luaOnEvent = self.OnEvent
	self._onBindPlayer = function(evt) self:OnBindPlayer(evt) end
	self._onUnitListener = function(evt, param)
		if evt ~= Enum.UnitEvent.AttrChanged then return end
		if Enum.AttrID.HP == param.attrId then
			self.luaHP.value = param.val/100;
		elseif Enum.AttrID.MP == param.attrId then
			self.luaMP.value = param.val/100;
		end	
	end;
end

function M:Start()
	self.luaPlayer:SetActive(false)
	self.luaHP = self.luaHP:GetComponent(typeof(UI.Slider))
	self.luaMP = self.luaMP:GetComponent(typeof(UI.Slider))
	EventMgr.single:AddListener("Game.Player", self._onBindPlayer);
	EventListener.Get(self.luaPlayer):AddOnClick(function(evt)   WindowMgr.single:GetWindow("PlayerWindow", true).mLO.mLT._player = self._hero end)
	EventListener.Get(self.luaTask):AddOnClick(function(evt)  WindowMgr.single:GetWindow("TaskWindow", true) end)
	EventListener.Get(self.luaBag):AddOnClick(function(evt)  WindowMgr.single:GetWindow("BagWindow", true) end)
	EventListener.Get(self.luaShop):AddOnClick(function(evt)  WindowMgr.single:GetWindow("ShopWindow", true) end)
	EventListener.Get(self.luaForgin):AddOnClick(function(evt)  WindowMgr.single:GetWindow("ForginWindow", true) end)
	EventListener.Get(self.luaSkill):AddOnClick(function(evt)  WindowMgr.single:GetWindow("SkillWindow", true).mLO.mLT._player = self._hero end)

	EventListener.Get(self.luaMail):AddOnClick(function(evt)  WindowMgr.single:GetWindow("MailWindow", true) end)
	EventListener.Get(self.luaChat):AddOnClick(function(evt)  WindowMgr.single:GetWindow("ChatWindow", true) end)
	EventListener.Get(self.luaHero):AddOnClick(function(evt)  WindowMgr.single:GetWindow("RoleCreateWindow", true) end)
	EventListener.Get(self.luaLogout):AddOnClick(function(evt)  EventMgr.single:SendEvent("Game.Logout") end)
	self.luaIndicator = self.luaIndicator:GetComponent("UIIndicator")
	self.luaJoyStick = self.luaJoyStick:GetComponent("UIStick")
	self.luaSkillBtn = self.luaSkillBtn:GetComponent("UISList")

	if self._hero == nil then
		WindowMgr.single:GetWindow("RoleCreateWindow", true)
	end
end

function M:OnDestroy()
	EventMgr.single:RemoveListener("Game.Player", self._onBindPlayer);
end

function M:OnBindPlayer(evt)
	self:UnBindPlayer(self._hero)
    self._hero = evt.data
    local cameraCtr = CameraMgr.single:GetCamera("MainCamera"):GetComponent("CameraController")
    cameraCtr.mTarget = self._hero.transform

	self:UpdateSkills()

    self:SetPlayer()
    self._hero:addListener(self._onUnitListener)
end

function M:UnBindPlayer(unit)
	if unit == nil then return end
	unit:removeListener(self._onUnitListener)
end

function M:SetPlayer()
	self.luaPlayer:SetActive(true)
	local tb = LTBPlayer[self._hero.tid]
	self.luaPlayer:GetComponent(typeof(PlayerHeader)):SetData(tb.name, tb.icon, 1)
	local attr = self._hero.attr
	self.luaHP.value = attr.HP/100;
	self.luaMP.value = attr.MP/100;
end

function M:OnUpdate()
	self._timeGap = self._timeGap + Time.deltaTime
	if self._timeGap < 0.1 then return end
	self._timeGap = self._timeGap - 0.1

	local  hero = self._hero
	if hero == nil then return end
	local  joystick = self.luaJoyStick
	local  dir = Vector3(joystick.mDir.x, 0.0, joystick.mDir.y).normalized
	if dir.z == 0 and dir.x==0 then
		hero.move:moveStop ()
	else
    	hero.move:move (dir)
    end
end

function M:ShowDrop(dropID)
	local item = LTBItem[dropID]
	if item == nil then return end
	local fly = ResLoad.get("UI/FlyItem"):gameObject()
	print(item.icon)
	AssetRef.setImage(fly:GetComponent("Image"), item.icon)
	local t = fly.transform
	t:SetParent(self._cs.transform, false)

	t:DOScale(0.5, 1):Complete()
	local seq = DOTween.Sequence()
	seq:Append(t:DOScale(1, 0.3))
	seq:Insert(0.8, t:DOMove(self.luaBag.transform.position, 1))
	seq:Insert(1.3, t:DOScale(0.5, 0.5))
	seq:LPlay():LOnComplete(function() GameObject.Destroy(fly) end)
end

function M:UpdateSkills()
	local list = self.luaSkillBtn
	list:clearItem()
    local skills = self._hero.skill.skills;
    for i=1,skills.Count do
    	local skill = skills[i-1]
    	local it = list:addItem(skill)
    	it.mLO.mLT:SetData(skill)
    end
end
--========================
LMainWindow = M
createClass("LMainWindow",LMainWindow,LWindow)