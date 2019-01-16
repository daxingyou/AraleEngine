if not LLanLoginWindow then print("same lua, reload ignore!!!") end

local M =
{
	_onAddHost;
	_onDelHost;
}

function M:new(cs)
	cs.luaOnStart = self.Start;
	self._onAddHost = function(evt) self:OnAddHost(evt) end
	self._onDelHost = function(evt) self:OnDelHost(evt) end
end

function M:Start()
	EventListener.Get(self.luaCreateGame):AddOnClick(function(evt)  self:OnCreateGameClick() end)
	EventListener.Get(self.luaEnterGame):AddOnClick(function(evt)  self:OnEnterGameClick() end)

	local sbs = UISwitch.getGroupSwitch ("login1");
	for i=1, sbs.Count do
		sbs[i-1].onValueChange = function(sb) self:OnSwitchChange(sb) end
	end
	sbs[0].isOn = true
end

function M:OnSwitchChange(sb)
	if sb.isOn ~= true then return end
	if sb._userData == 0 then
		self.luaClientPage:SetActive(true)
		self.luaHostPage:SetActive(false)
	else
		self.luaClientPage:SetActive(false)
		self.luaHostPage:SetActive(true)
	end
end

function M:OnWindowEvent(evnetId)
	if evnetId == Window.Event.Create then
		self.luaHosts = self.luaHosts:GetComponent("UISList")
		self.luaHosts.onSelectedChange = function()
			local sel = self.luaHosts:getFirstSelected()
			if sel == nil then return end
			local ipport = it.data
			self.luaHostIP:GetComponent("InputField").text = ipport.ip ..":"..ipport.port
		end
		self.luaHostIP:GetComponent("InputField").text = "127.0.0.1:5003"
		self.luaGameName:GetComponent("InputField").text = "Arale的游戏"
		NetMgr.single:createLanClient();

		EventMgr.single:AddListener("Game.AddHost", self._onAddHost)
		EventMgr.single:AddListener("Game.DelHost", self._onDelHost)
	elseif evnetId == Window.Event.Destroy then
		EventMgr.single:RemoveListener("Game.AddHost", self._onAddHost)
		EventMgr.single:RemoveListener("Game.DelHost", self._onDelHost)
	end
end

function M:OnAddHost(evt)
	self.luaHosts:addItem(evt.data)
end


function M:OnDelHost(evt)
	self.luaHosts:delItem(evt.data)
end

function M:OnCreateGameClick()
	local gameName = self.luaGameName:GetComponent("InputField").text
	local ipport = self.luaHostIP:GetComponent("InputField").text
	local idx = string.find(ipport,":")
	local port = string.sub(ipport, idx+1)
	local ip = string.sub(ipport,0,idx-1)
	NetMgr.single:createLanHost(gameName)
    NetMgr.client:connet(ip, tonumber(port))
end

function M:OnEnterGameClick()
	NetMgr.client:connet("127.0.0.1", 5003);
end
--========================
LLanLoginWindow = M
createClass("LLanLoginWindow",LLanLoginWindow)