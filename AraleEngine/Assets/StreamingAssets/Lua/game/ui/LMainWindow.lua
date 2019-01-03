if not LMainWindow then

local M =
{
	_hero;
	_timeGap = 0;
}

function M:new(cs)
	cs.luaOnStart = self.Start
	cs.luaOnDestroy=self.OnDestroy
	cs.luaOnUpdate=self.OnUpdate
	self._onBindPlayer = function(evt) self:OnBindPlayer(evt) end
end

function M:Start()
	EventMgr.single:AddListener("Game.Player", self._onBindPlayer);
	EventListener.Get(self.luaPlayer):AddOnClick(function(evt)  WindowMgr.single:GetWindow("PlayerWindow", true) end)
	EventListener.Get(self.luaTask):AddOnClick(function(evt)  WindowMgr.single:GetWindow("TaskWindow", true) end)
	EventListener.Get(self.luaBag):AddOnClick(function(evt)  WindowMgr.single:GetWindow("BagWindow", true) end)
	EventListener.Get(self.luaShop):AddOnClick(function(evt)  WindowMgr.single:GetWindow("ShopWindow", true) end)
	EventListener.Get(self.luaForgin):AddOnClick(function(evt)  WindowMgr.single:GetWindow("ForginWindow", true) end)
	EventListener.Get(self.luaSkill):AddOnClick(function(evt)  WindowMgr.single:GetWindow("SkillWindow", true) end)

	EventListener.Get(self.luaHero):AddOnClick(function(evt)  WindowMgr.single:GetWindow("RoleCreateWindow", true) end)
	EventListener.Get(self.luaLogout):AddOnClick(function(evt)  EventMgr.single:SendEvent("Game.Logout") end)
	self.luaJoyStick = self.luaJoyStick:GetComponent("UIStick")
	self.luaSkillBtn = self.luaSkillBtn:GetComponent("UISList")
	self.luaSkillBtn.onSelectedChange = function(selItem)
			local skill = selItem.mLO.mLT._skill
			if skill == nil then return end
			self._hero.skill:play(skill.mTID)
	end

	if _hero == nil then
		WindowMgr.single:GetWindow("RoleCreateWindow", true)
	end
end

function M:OnDestroy()
	EventMgr.single:RemoveListener("Game.Player", self._onBindPlayer);
end

function M:OnBindPlayer(evt)
	local list = self.luaSkillBtn
	list:clearItem()
    self._hero = evt.data
    local cameraCtr = CameraMgr.single:GetCamera("MainCamera"):GetComponent("CameraController")
    cameraCtr.mTarget = self._hero.transform

    local skills = self._hero.skill.skills;
    for i=1,skills.Count do
    	local skill = skills[i-1]
    	local it = list:addItem(skill)
    	it.mLO.mLT:SetData(skill)
    end
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
		hero.nav:stopMove ()
	else
    	hero.nav:move (dir)
    end
end

--========================
LMainWindow = M
createClass("LMainWindow",LMainWindow)
end