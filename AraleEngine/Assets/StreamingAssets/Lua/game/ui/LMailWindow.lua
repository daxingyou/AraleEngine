if not LMailWindow then print("same lua, reload ignore!!!") end
--======================
local M=
{
	_cs;
	_mail;
	mails = 
	{
		[1]={id=1001; state=0; playerid=1000; nick="系统"; title="五一送礼"; content="阿拉蕾的馈赠"; rewards={[1]={id=1;num=500;}; [2]={id=2;num=500;};};};
		[2]={id=1002; state=1; playerid=1000; nick="系统"; title="六一送礼"; content="阿拉蕾的馈赠"; rewards={[1]={id=1;num=500;};};};
		[3]={id=1003; state=2; playerid=1000; nick="系统"; title="七夕送礼"; content="阿拉蕾的馈赠"; rewards={[1]={id=1;num=500;}; [2]={id=2;num=500;};};};
		[4]={id=1004; state=0; playerid=1000; nick="系统"; title="停服公告"; content="阿拉蕾休假一天"; };
		[5]={id=1005; state=0; playerid=1001; nick="阿拉蕾"; title="见面礼"; content="阿拉蕾的馈赠"; rewards={[1]={id=1;num=500;}; [2]={id=2;num=500;};};};
	};
}

function M:new(cs)
	self._cs = cs;
	cs.luaOnStart = function() self:Start(); end
end

function M:Start()
	self.luaContent = self.luaContent:GetComponent("UISList");
	self.luaContent.onSelectedChange = function(selItem)
		local mail = selItem.mLO.mLT._mail
		self:ShowMailDesc(mail)
	end

	local sbs = UISwitch.getGroupSwitch ("mail1");
	for i=1, sbs.Count do
		sbs[i-1].onValueChange = function(sb) self:OnSwitchChange(sb) end
	end
	sbs[0].isOn = true

	EventListener.Get(self.luaMail):AddOnClick(function(evt)  self.luaMails:SetActive(true); self.luaMail:SetActive(false) end)
	EventListener.Get(self.luaMail):AddOnBeginDrag(function(evt)  evt.eligibleForClick = false end)
	EventListener.Get(self.luaGain2):AddOnClick(function(evt)  self:OnMailGainClick() end)
end

function M:OnSwitchChange(sb)
	if sb.isOn ~= true then return end
	self.luaMails:SetActive(true)
	self.luaMail:SetActive(false)
	self:ShowItems(sb._userData)
end

function M:ShowItems(itemType)
	local list = self.luaContent
	list:clearItem()
	if itemType == 0 then
		self:ShowSystemMail(list)
	else
		self:ShowPlayerMail(list)
	end
	
end

function M:ShowSystemMail(list)
	local mails = M.mails
	for i=1,#mails do
		local mail = mails[i]
		if mail.playerid == 1000 then
			local it = list:addItem(mail)
			it.mLO.mLT:SetData(mail,self.luaItem)
		end
	end
end

function M:ShowPlayerMail(list)
	local mails = M.mails
	for i=1,#mails do
		local mail = mails[i]
		if mail.playerid ~= 1000 then
			local it = list:addItem(mail)
			it.mLO.mLT:SetData(mail,self.luaItem)
		end
	end
end

function M:ShowMailDesc(mail)
	self._mail = mail
	self.luaMails:SetActive(false)
	self.luaMail:SetActive(true)
	self.luaNick2:GetComponent("Text").text = mail.nick
	self.luaTitle2:GetComponent("Text").text = mail.title
	self.luaDesc2:GetComponent("Text").text = mail.content
	if mail.rewards==nil or #mail.rewards < 1 then
		self.luaGain2:SetActive(false)
		self.luaReward2:SetActive(false)
	else
		self.luaGain2:SetActive(mail.state~=2)
		self.luaReward2:SetActive(true)
		local mount = self.luaReward2.transform
		GHelper.DestroyChilds(mount)
		for i=1,#mail.rewards do
			local reward = mail.rewards[i]
			local it = GameObject.Instantiate(self.luaItem)
			it.transform:SetParent(mount, false)
			local item = LTBItem[reward.id]
			it:GetComponent(typeof(UIItemSlot)):SetData(item.icon,item.name,reward.num)
		end
	end
end

function M:OnMailGainClick( ... )
	self:GainReward(self._mail)
end

function M:GainReward(mail)
	if mail.state == 2 then return end
	print("gain reward mailid="..mail.id)
	local ls = LuaHelp.List_object
	for i=1,#mail.rewards do
		local reward = mail.rewards[i]
		ls:Add(reward.id)
		ls:Add(reward.num)
	end
	LRewardWindow.Show(ls)
end
--=======================
LMailWindow = M
createClass("LMailWindow",LMailWindow);