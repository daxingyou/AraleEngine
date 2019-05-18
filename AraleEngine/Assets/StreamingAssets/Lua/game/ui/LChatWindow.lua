if LChatWindow then print("same lua, reload ignore!!!") end

local M =
{
	_cs;
	_scrollToBottom;
}

function M:new(csobj)
	self._cs = csobj;
	csobj.luaOnStart = self.Start
end

function M:Start()
	local tb = 
	{
			[0] = function(tb)
				return tb.tip,WaitForSeconds(3)
			end;

			tip = function(tb)
				local it =  self.luaChat:addItem(nil)
				local nick = it.mLO.mLT.luaNick:GetComponent("Text")
				local time = it.mLO.mLT.luaTime:GetComponent("Text")
				local ctx  = it.mLO.mLT.luaText:GetComponent("UIImageText")
				nick.text = "阿拉蕾";
				time.text = RTime.R.localTime:ToShortTimeString()
				ctx.text = "欢迎访问游戏主页:⊕2http:\\www.arale.com"
				self._scrollToBottom=true
			end;
	}
	self._cs:startLuaCoroutine(tb)
	self.luaInput = self.luaInput:GetComponent("InputField")
	self.luaChat  = self.luaChat:GetComponent("UISList")
	self.luaChatScroll = self.luaChatScroll:GetComponent("ScrollRect")
	EventListener.Get(self.luaSend):AddOnClick(function(evt)  self:OnSendClick() end)
	EventListener.Get(self.luaShow):AddOnClick(function(evt)  self:OnShowClick() end)
	self.luaChatScroll.verticalScrollbar.onValueChanged:AddListener(function(val)
		if self._scrollToBottom then self.luaChatScroll.verticalScrollbar.value = 0 end
		self._scrollToBottom=false
	end)
end

function M:OnSendClick()
	local txt = self.luaInput.text
	local it =  self.luaChat:addItem(nil)
	local nick = it.mLO.mLT.luaNick:GetComponent("Text")
	local time = it.mLO.mLT.luaTime:GetComponent("Text")
	local ctx  = it.mLO.mLT.luaText:GetComponent("UIImageText")

	nick.text = "阿拉蕾";
	time.text = RTime.R.localTime:ToShortTimeString()
	ctx.text  = txt
	self._scrollToBottom=true
end

function M:OnShowClick()
	WindowMgr.single:GetWindow("BagWindow", true)
	local txt = self.luaInput.text
	local it =  self.luaChat:addItem(nil)
	local nick = it.mLO.mLT.luaNick:GetComponent("Text")
	local time = it.mLO.mLT.luaTime:GetComponent("Text")
	local ctx  = it.mLO.mLT.luaText:GetComponent("UIImageText")

	nick.text = "阿拉蕾";
	time.text = RTime.R.localTime:ToShortTimeString()
	ctx.text = "⊕1a⊕0123456789.123456789012345⊕11⊕31000⊕2http:\\www.arale.com"
	self._scrollToBottom=true
end
--========================
LChatWindow = M
createClass("LChatWindow",LChatWindow)