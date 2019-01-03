if not LMailItem then

local M =
{
	_mail=nil;
}

function M:new(csobj)
	csobj.luaOnAwake = self.Awake
end

function M:Awake()
	--self.luaItem = self.luaItem:GetComponent("UIItemSlot")
	EventListener.Get(self.luaGain):AddOnClick(function(evt)  self:OnMailGainClick() end)
end

function M:SetData(mail,itemprefab)
	self._mail = mail
	self.luaNick:GetComponent("Text").text = mail.nick
	self.luaTitle:GetComponent("Text").text = mail.title
	if mail.rewards==nil or #mail.rewards < 1 then
		self.luaGain:SetActive(false)
		self.luaReward:SetActive(false)
	else
		self.luaGain:SetActive(true)
		self.luaReward:SetActive(true)
		local mount = self.luaReward.transform
		GHelper.DestroyChilds(mount)
		for i=1,#mail.rewards do
			local reward = mail.rewards[i]
			local it = GameObject.Instantiate(itemprefab)
			it.transform:SetParent(mount, false)
			it:GetComponent(typeof(UIItemSlot)):SetData(tostring(reward.id),reward.name,reward.num)
		end
	end
end

function M:OnMailGainClick( ... )
	if self._mail.state == 2 then return end
	print("gain reward mailid="..self._mail.id)
end
--========================
LMailItem = M
createClass("LMailItem",LMailItem)
end